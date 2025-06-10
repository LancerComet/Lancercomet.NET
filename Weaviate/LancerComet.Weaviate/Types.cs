using System.Text.Json;
using System.Text.Json.Serialization;

namespace LancerComet.Weaviate;

public class WeaviateClass {
  public string Class { get; set; } = "";
  public string Description { get; set; } = "";
  public List<WeaviateProperty> Properties { get; set; } = [];
  public string Vectorizer { get; set; } = "none";
}

public class WeaviateProperty {
  public string Name { get; set; } = "";
  public string[] DataType { get; set; } = [];
  public string Description { get; set; } = "";
}

public class WeaviateSchema {
  public List<WeaviateClass> Classes { get; set; } = [];
}

public class WeaviateObject {
  public string Id { get; set; } = "";
  public string Class { get; set; } = "";
  public object Properties { get; set; } = "";
  public float[] Vector { get; set; } = [];
}

public class WeaviateObjectResponse {
  public string Id { get; set; } = "";
  public string Class { get; set; } = "";
  public object? Properties { get; set; }
}

public class WeaviateBatchResponse {
  public List<WeaviateBatchResult> Results { get; set; } = [];
}

public class WeaviateBatchResult {
  public WeaviateBatchResultDetail Result { get; set; } = new();
}

public class WeaviateBatchResultDetail {
  public string Id { get; set; } = "";
  public string Status { get; set; } = "";
}

public class WeaviateSearchResult {
  [JsonPropertyName("_additional")]
  public Dictionary<string, object> Additional { get; set; } = new();

  public string? Id => this.Additional.GetValueOrDefault("id")?.ToString();

  public float Distance => float.TryParse(this.Additional.GetValueOrDefault("distance")?.ToString(), out var d) ? d : 1.0f;

  public float Similarity => 1.0f - this.Distance;
}

public class WeaviateGraphQlData {
  [JsonPropertyName("Get")]
  public JsonElement Get { get; set; } = new();
}

public class WeaviateGraphQlResponse {
  [JsonPropertyName("data")]
  public WeaviateGraphQlData Data { get; set; } = new();
}
