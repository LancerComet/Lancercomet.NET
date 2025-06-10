using System.Text;
using System.Text.Json;

namespace LancerComet.Weaviate;

public class WeaviateClient : IDisposable {
  private readonly HttpClient _httpClient;
  private readonly string _baseUrl;
  private readonly JsonSerializerOptions? _jsonOptions;

  public async Task<bool> CheckHealthAsync () {
    var response = await this._httpClient.GetAsync($"{this._baseUrl}/v1/.well-known/ready");
    return response.IsSuccessStatusCode;
  }

  public async Task CreateSchemaAsync (WeaviateClass schemaClass) {
    var json = JsonSerializer.Serialize(schemaClass, this._jsonOptions);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    var response = await this._httpClient.PostAsync($"{this._baseUrl}/v1/schema", content);
    response.EnsureSuccessStatusCode();
  }

  public async Task<WeaviateSchema?> GetSchemaAsync () {
    var response = await this._httpClient.GetAsync($"{this._baseUrl}/v1/schema");
    response.EnsureSuccessStatusCode();

    var json = await response.Content.ReadAsStringAsync();
    return JsonSerializer.Deserialize<WeaviateSchema>(json, this._jsonOptions);
  }

  public async Task<string?> InsertObjectAsync (string className, object properties, float[] vector) {
    var obj = new WeaviateObject {
      Class = className,
      Properties = properties,
      Vector = vector
    };

    var json = JsonSerializer.Serialize(obj, this._jsonOptions);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    var response = await this._httpClient.PostAsync($"{this._baseUrl}/v1/objects", content);
    response.EnsureSuccessStatusCode();

    var result = await response.Content.ReadAsStringAsync();
    var resultObj = JsonSerializer.Deserialize<WeaviateObjectResponse>(result, this._jsonOptions);
    return resultObj?.Id;
  }

  public async Task<List<string>> BatchInsertAsync (
    string className,
    List<(object properties, float[] vector)> objects
  ) {
    var ids = new List<string>();

    var batchObjects = objects.Select(obj => new WeaviateObject {
      Class = className,
      Properties = obj.properties,
      Vector = obj.vector
    }).ToArray();

    var batch = new { objects = batchObjects };
    var json = JsonSerializer.Serialize(batch, this._jsonOptions);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    var response = await this._httpClient.PostAsync($"{this._baseUrl}/v1/batch/objects", content);
    response.EnsureSuccessStatusCode();

    var result = await response.Content.ReadAsStringAsync();
    var batchResult = JsonSerializer.Deserialize<WeaviateBatchResponse>(result, this._jsonOptions);

    if (batchResult?.Results != null) {
      ids.AddRange(batchResult.Results
        .Where(r => r.Result.Status == "SUCCESS")
        .Select(r => r.Result.Id)
        .Where(id => !string.IsNullOrEmpty(id))
      );
    }

    return ids;
  }

  public async Task<List<T>?> VectorSearchAsync<T> (
    string className,
    float[] queryVector,
    int limit = 10,
    float maxDistance = 1f,
    string[]? fields = null
  ) {
    if (maxDistance is < 0 or > 2.0f) {
      throw new ArgumentException("Distance must be between 0.0 and 2.0", nameof(maxDistance));
    }

    fields ??= ["_additional { id distance }"];

    var query = $@"{{
      Get {{
        {className}(
          nearVector: {{
            vector: [{string.Join(",", queryVector)}]
            distance: {maxDistance:F2}
          }}
          limit: {limit}
        ) {{
          {string.Join(" ", fields)}
        }}
      }}
    }}";

    var requestBody = new { query };
    var json = JsonSerializer.Serialize(requestBody, this._jsonOptions);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    var response = await this._httpClient.PostAsync($"{this._baseUrl}/v1/graphql", content);
    response.EnsureSuccessStatusCode();

    var result = await response.Content.ReadAsStringAsync();
    var graphqlResponse = JsonSerializer.Deserialize<WeaviateGraphQlResponse>(result, this._jsonOptions);

    if (graphqlResponse?.Data.Get != null) {
      var classData = graphqlResponse.Data.Get.GetProperty(className);
      var results = JsonSerializer.Deserialize<List<T>>(classData.GetRawText(), this._jsonOptions);
      return results;
    }

    return [];
  }

  public async Task<WeaviateObject?> GetObjectAsync (string id) {
    var response = await this._httpClient.GetAsync($"{this._baseUrl}/v1/objects/{id}");
    response.EnsureSuccessStatusCode();
    var json = await response.Content.ReadAsStringAsync();
    return JsonSerializer.Deserialize<WeaviateObject>(json, this._jsonOptions);
  }

  public async Task DeleteObjectAsync (string id) {
    var response = await this._httpClient.DeleteAsync($"{this._baseUrl}/v1/objects/{id}");
    response.EnsureSuccessStatusCode();
  }

  public async Task DeleteClassAsync (string className) {
    var response = await this._httpClient.DeleteAsync($"{this._baseUrl}/v1/schema/{className}");
    response.EnsureSuccessStatusCode();
  }

  public void Dispose () {
    this._httpClient?.Dispose();
  }

  public WeaviateClient (
    string baseUrl,
    string? apiKey = null,
    int timeoutSec = 10
  ) {
    this._baseUrl = baseUrl.TrimEnd('/');
    this._httpClient = new HttpClient {
      Timeout = TimeSpan.FromSeconds(timeoutSec)
    };

    var authKey = apiKey ?? "";
    if (!string.IsNullOrEmpty(authKey)) {
      this._httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {authKey}");
    }

    this._jsonOptions = new JsonSerializerOptions {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      WriteIndented = true
    };
  }
}
