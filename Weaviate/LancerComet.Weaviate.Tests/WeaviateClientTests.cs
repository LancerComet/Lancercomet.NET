namespace LancerComet.Weaviate.Tests;

public class WeaviateClientTests : IDisposable {
  private readonly WeaviateClient _weaviateClient;

  public WeaviateClientTests () {
    this._weaviateClient = new WeaviateClient("http://localhost:8080");
  }

  [Fact]
  public async Task CheckHealthAsync_WithoutServer_ShouldThrowException () {
    // Act & Assert
    // Since there's no server running and try-catch was removed, this should throw an exception
    await Assert.ThrowsAsync<HttpRequestException>(
      () => this._weaviateClient.CheckHealthAsync()
    );
  }

  [Fact]
  public async Task CreateSchemaAsync_WithoutServer_ShouldThrowException () {
    // Arrange
    var schemaClass = new WeaviateClass {
      Class = "TestClass",
      Description = "Test description",
      Properties = [
        new WeaviateProperty {
          Name = "name",
          DataType = ["string"],
          Description = "Name property"
        }
      ]
    };

    // Act & Assert
    // This should throw an exception since there's no server running
    await Assert.ThrowsAsync<HttpRequestException>(
      () => this._weaviateClient.CreateSchemaAsync(schemaClass)
    );
  }

  [Fact]
  public async Task GetSchemaAsync_WithoutServer_ShouldThrowException () {
    // Act & Assert
    // This should throw an exception since there's no server running
    await Assert.ThrowsAsync<HttpRequestException>(
      () => this._weaviateClient.GetSchemaAsync()
    );
  }
  public void Dispose () {
    this._weaviateClient?.Dispose();
  }
}
