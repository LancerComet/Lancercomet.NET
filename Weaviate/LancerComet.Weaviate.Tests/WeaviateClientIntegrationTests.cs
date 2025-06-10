using Testcontainers.Weaviate;

namespace LancerComet.Weaviate.Tests;

public class WeaviateClientIntegrationTests : IAsyncLifetime {
  private WeaviateContainer? _weaviateContainer;
  private WeaviateClient? _weaviateClient;

  public async Task InitializeAsync () {
    this._weaviateContainer = new WeaviateBuilder()
      .WithEnvironment("PERSISTENCE_DATA_PATH", "/var/lib/weaviate")
      .WithEnvironment("DEFAULT_VECTORIZER_MODULE", "none")
      .WithEnvironment("CLUSTER_HOSTNAME", "node1")
      .Build();

    await this._weaviateContainer.StartAsync();

    var host = this._weaviateContainer.Hostname;
    var port = this._weaviateContainer.GetMappedPublicPort(8080);
    var connectionString = $"http://{host}:{port}";
    this._weaviateClient = new WeaviateClient(connectionString);
  }

  public async Task DisposeAsync () {
    this._weaviateClient?.Dispose();
    if (this._weaviateContainer != null) {
      await this._weaviateContainer.DisposeAsync();
    }
  }

  [Fact]
  public async Task HealthCheck_ShouldReturnTrue () {
    // Act
    var isHealthy = await this._weaviateClient!.CheckHealthAsync();

    // Assert
    Assert.True(isHealthy);
  }

  [Fact]
  public async Task CreateAndGetSchema_ShouldWork () {
    // Arrange
    var testClass = new WeaviateClass {
      Class = "TestArticle",
      Description = "A test article class",
      Properties = [
        new WeaviateProperty {
          Name = "title",
          DataType = ["string"],
          Description = "The title of the article"
        },
        new WeaviateProperty {
          Name = "content",
          DataType = ["text"],
          Description = "The content of the article"
        }
      ]
    };

    // Act
    await this._weaviateClient!.CreateSchemaAsync(testClass);
    var schema = await this._weaviateClient.GetSchemaAsync();

    // Assert
    Assert.NotNull(schema);
    Assert.Contains(schema.Classes, c => c.Class == "TestArticle");
  }

  [Fact]
  public async Task CompleteWorkflow_ShouldWork () {
    // Arrange
    var articleClass = new WeaviateClass {
      Class = "IntegrationTestArticle",
      Description = "An article for integration testing",
      Properties = [
        new WeaviateProperty {
          Name = "title",
          DataType = ["string"],
          Description = "Article title"
        },
        new WeaviateProperty {
          Name = "author",
          DataType = ["string"],
          Description = "Article author"
        },
        new WeaviateProperty {
          Name = "publishDate",
          DataType = ["date"],
          Description = "Publication date"
        }
      ]
    };

    // Act & Assert
    // 1. Create schema
    await this._weaviateClient!.CreateSchemaAsync(articleClass);

    // 2. Verify schema exists
    var schema = await this._weaviateClient.GetSchemaAsync();
    Assert.NotNull(schema);
    Assert.Contains(schema.Classes, c => c.Class == "IntegrationTestArticle");

    // 3. Verify class properties
    var createdClass = schema.Classes.First(c => c.Class == "IntegrationTestArticle");
    Assert.Equal(3, createdClass.Properties.Count);
    Assert.Contains(createdClass.Properties, p => p.Name == "title");
    Assert.Contains(createdClass.Properties, p => p.Name == "author");
    Assert.Contains(createdClass.Properties, p => p.Name == "publishDate");
  }
}
