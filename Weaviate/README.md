# LancerComet.Weaviate

A .NET client for [Weaviate](https://weaviate.io/), the open-source vector database.

## Features

- 🏗️ **Schema Management** - Create and manage Weaviate classes and properties
- 📄 **Object Operations** - Insert, retrieve, update, and delete objects
- 🔍 **Vector Search** - Perform similarity searches using vector embeddings
- 📦 **Batch Operations** - Efficiently insert multiple objects at once
- 🩺 **Health Checks** - Monitor Weaviate instance status
- 🎯 **Strongly Typed** - Full IntelliSense support with strongly typed models
- ⚡ **Async/Await** - Modern async programming patterns
- 🧪 **Well Tested** - Comprehensive unit and integration tests

## Installation

Install the package via NuGet Package Manager:

```powershell
Install-Package LancerComet.Weaviate
```

Or via .NET CLI:

```bash
dotnet add package LancerComet.Weaviate
```

## Quick Start

### Basic Setup

```csharp
using LancerComet.Weaviate;

// Create a client instance
var client = new WeaviateClient("http://localhost:8080");

// With API key authentication
var clientWithAuth = new WeaviateClient("http://localhost:8080", "your-api-key");

// With custom timeout
var clientWithTimeout = new WeaviateClient("http://localhost:8080", null, timeoutSec: 30);
```

### Health Check

```csharp
bool isHealthy = await client.CheckHealthAsync();
if (isHealthy) {
  Console.WriteLine("Weaviate is running!");
}
```

### Schema Management

```csharp
// Define a schema class
var articleClass = new WeaviateClass {
    Class = "Article",
    Description = "A news article",
    Properties = new List<WeaviateProperty>
    {
        new WeaviateProperty
        {
            Name = "title",
            DataType = new[] { "string" },
            Description = "The title of the article"
        },
        new WeaviateProperty
        {
            Name = "content",
            DataType = new[] { "text" },
            Description = "The content of the article"
        },
        new WeaviateProperty
        {
            Name = "publishDate",
            DataType = new[] { "date" },
            Description = "Publication date"
        }
    },
    Vectorizer = "text2vec-openai"
};

// Create the schema
bool created = await client.CreateSchemaAsync(articleClass);

// Get existing schema
var schema = await client.GetSchemaAsync();
```

### Object Operations

#### Insert Single Object

```csharp
var articleData = new {
    title = "Introduction to Vector Databases",
    content = "Vector databases are becoming increasingly important...",
    publishDate = DateTime.Now
};

var vector = new[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f };

string objectId = await client.InsertObjectAsync("Article", articleData, vector);
```

#### Batch Insert

```csharp
var articles = new List<(object properties, float[] vector)> {
    (new { title = "Article 1", content = "Content 1" }, new[] { 0.1f, 0.2f }),
    (new { title = "Article 2", content = "Content 2" }, new[] { 0.3f, 0.4f }),
    (new { title = "Article 3", content = "Content 3" }, new[] { 0.5f, 0.6f })
};

var insertedIds = await client.BatchInsertAsync("Article", articles);
```

#### Retrieve Object

```csharp
var retrievedObject = await client.GetObjectAsync(objectId);
if (retrievedObject != null) {
    Console.WriteLine($"Object Class: {retrievedObject.Class}");
    Console.WriteLine($"Object ID: {retrievedObject.Id}");
}
```

#### Delete Object

```csharp
bool deleted = await client.DeleteObjectAsync(objectId);
```

### Vector Similarity Search

```csharp
var queryVector = new[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f };

var searchResults = await client.VectorSearchAsync(
    className: "Article",
    queryVector: queryVector,
    limit: 10,
    maxDistance: 0.7f,
    fields: new[] { "title", "content", "_additional { id distance }" }
);

foreach (var result in searchResults) {
    Console.WriteLine($"ID: {result.Id}, Distance: {result.Distance:F3}");
}
```

### Clean Up

```csharp
// Delete a class and all its objects
await client.DeleteClassAsync("Article");

// Dispose the client
client.Dispose();
```

## Data Models

### WeaviateClass

Represents a Weaviate class schema:

```csharp
public class WeaviateClass {
    public string Class { get; set; } = "";
    public string Description { get; set; } = "";
    public List<WeaviateProperty> Properties { get; set; } = new();
    public string Vectorizer { get; set; } = "none";
}
```

### WeaviateProperty

Represents a property within a Weaviate class:

```csharp
public class WeaviateProperty {
    public string Name { get; set; } = "";
    public string[] DataType { get; set; } = Array.Empty<string>();
    public string Description { get; set; } = "";
}
```

### WeaviateObject

Represents a Weaviate object:

```csharp
public class WeaviateObject {
    public string Id { get; set; } = "";
    public string Class { get; set; } = "";
    public object Properties { get; set; } = new();
    public float[] Vector { get; set; } = Array.Empty<float>();
}
```

## Configuration

### Constructor Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `baseUrl` | string | Required | The base URL of your Weaviate instance |
| `apiKey` | string? | null | API key for authentication (optional) |
| `timeoutSec` | int | 10 | HTTP client timeout in seconds |

### JSON Serialization

The client uses `System.Text.Json` with the following options:
- `PropertyNamingPolicy`: `JsonNamingPolicy.CamelCase`
- `WriteIndented`: `true`

## Error Handling

The client methods are designed to be robust and handle common error scenarios:

- Network connectivity issues
- HTTP errors (4xx, 5xx responses)
- JSON serialization/deserialization errors
- Timeout scenarios

Most methods return `null`, `false`, or empty collections when errors occur, rather than throwing exceptions.

## Testing

```bash
# Run all tests
dotnet test

# Run unit tests only
dotnet test --filter "FullyQualifiedName!~IntegrationTests"

# Run integration tests only (requires Docker)
dotnet test --filter "FullyQualifiedName~IntegrationTests"
```

## Requirements

- .NET 9.0 or later
- Weaviate instance (local or remote)
- For integration tests: Docker Desktop

## License

This project is licensed under the Apache License Version 2.0 - see the [LICENSE](LICENSE) file for details.

## Others

[Weaviate Documentation](https://weaviate.io/developers/weaviate)
