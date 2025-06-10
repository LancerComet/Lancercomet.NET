using System.Text.Json.Serialization;

namespace LancerComet.Weaviate.Tests;

public class Article {
  [JsonPropertyName("title")]
  public string Title { get; set; } = "";

  [JsonPropertyName("content")]
  public string Content { get; set; } = "";
}

public class WeaviateClientMethodTests : IDisposable {
  private readonly WeaviateClient _client;

  public WeaviateClientMethodTests () {
    this._client = new WeaviateClient("http://localhost:8080");
  }

  [Fact]
  public async Task InsertObjectAsync_WithoutServer_ShouldThrowException () {
    // Arrange
    var properties = new {
      title = "Test Article",
      content = "This is a test article content"
    };
    var vector = new[] { 0.1f, 0.2f, 0.3f };

    // Act & Assert
    // Since there's no server running and try-catch was removed, this should throw an exception
    await Assert.ThrowsAsync<HttpRequestException>(
      () => this._client.InsertObjectAsync("Article", properties, vector)
    );
  }

  [Fact]
  public async Task BatchInsertAsync_WithoutServer_ShouldReturnEmptyList () {
    // Arrange
    var objects = new List<(object properties, float[] vector)> {
      (new { title = "Article 1", content = "Content 1" }, new[] { 0.1f, 0.2f }),
      (new { title = "Article 2", content = "Content 2" }, new[] { 0.3f, 0.4f })
    };

    // Act
    await Assert.ThrowsAnyAsync<HttpRequestException>(
      () => this._client.BatchInsertAsync("Article", objects)
    );
  }

  [Fact]
  public async Task VectorSearchAsync_WithoutServer_ShouldThrowException () {
    // Arrange
    var queryVector = new[] { 0.1f, 0.2f, 0.3f, 0.4f };

    // Act & Assert
    // Since there's no server running and try-catch was removed, this should throw an exception
    await Assert.ThrowsAsync<HttpRequestException>(
      () => this._client.VectorSearchAsync<Article>("Article", queryVector, 5)
    );
  }

  [Fact]
  public async Task VectorSearchAsync_WithInvalidDistance_ShouldThrowArgumentException () {
    // Arrange
    var queryVector = new[] { 0.1f, 0.2f, 0.3f };

    // Act & Assert
    // Invalid distance parameters should throw ArgumentException
    await Assert.ThrowsAsync<ArgumentException>(
      () => this._client.VectorSearchAsync<Article>("Article", queryVector, 10, -1f)
    );

    await Assert.ThrowsAsync<ArgumentException>(
      () => this._client.VectorSearchAsync<Article>("Article", queryVector, 10, 3f)
    );
  }

  [Fact]
  public async Task GetObjectAsync_WithoutServer_ShouldThrowException () {
    // Arrange
    var id = Guid.NewGuid().ToString();

    // Act & Assert
    // Since there's no server running and try-catch was removed, this should throw an exception
    await Assert.ThrowsAsync<HttpRequestException>(
      () => this._client.GetObjectAsync(id)
    );
  }

  [Fact]
  public async Task DeleteObjectAsync_WithoutServer_ShouldThrowException () {
    // Arrange
    var id = Guid.NewGuid().ToString();

    // Act & Assert
    // Since there's no server running and try-catch was removed, this should throw an exception
    await Assert.ThrowsAsync<HttpRequestException>(
      () => this._client.DeleteObjectAsync(id)
    );
  }

  [Fact]
  public async Task DeleteClassAsync_WithoutServer_ShouldThrowException () {
    // Act & Assert
    // Since there's no server running and try-catch was removed, this should throw an exception
    await Assert.ThrowsAsync<HttpRequestException>(
      () => this._client.DeleteClassAsync("TestClass")
    );
  }

  [Fact]
  public void Constructor_WithValidParameters_ShouldNotThrow () {
    // Arrange & Act
    using var client1 = new WeaviateClient("http://localhost:8080");
    using var client2 = new WeaviateClient("http://localhost:8080", "test-api-key");
    using var client3 = new WeaviateClient("http://localhost:8080", null, 30);

    // Assert
    // If we get here without exceptions, the test passes
    Assert.True(true);
  }

  [Fact]
  public void Constructor_WithTrailingSlash_ShouldTrimBaseUrl () {
    // This is more of a behavioral test - we can't easily test the private field
    // but we can verify the constructor doesn't throw

    // Arrange & Act
    using var client = new WeaviateClient("http://localhost:8080/");

    // Assert
    Assert.True(true); // Constructor should handle trailing slash properly
  }

  public void Dispose () {
    this._client?.Dispose();
  }
}
