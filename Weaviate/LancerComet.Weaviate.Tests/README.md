# LancerComet.Weaviate Test Project

This is the test project for **LancerComet.Weaviate Client**, including unit tests and integration tests.

## Project Structure

```
LancerComet.Weaviate.Tests/
├── GlobalUsings.cs                    # Global using declarations
├── WeaviateClientTests.cs             # Basic functionality tests for WeaviateClient
├── WeaviateClientMethodTests.cs       # Method tests for WeaviateClient
├── WeaviateTypesTests.cs              # Data type tests
└── WeaviateClientIntegrationTests.cs  # Integration tests (requires Docker)
```

## Test Types

### 1. Unit Tests

* **WeaviateClientTests.cs**: Tests basic features of `WeaviateClient`
* **WeaviateClientMethodTests.cs**: Tests various methods of `WeaviateClient`
* **WeaviateTypesTests.cs**: Tests serialization and default values of data types

### 2. Integration Tests

* **WeaviateClientIntegrationTests.cs**: Runs real Weaviate instance using Testcontainers for integration testing

### 3. Test Utilities

* **TestHelper.cs**: Provides helper methods for creating test data

## Running the Tests

### Prerequisites

* .NET 9.0 SDK
* Docker Desktop (required for integration tests)

### Run All Tests

```powershell
dotnet test
```

### Run Specific Test Class

```powershell
# Run only unit tests
dotnet test --filter "FullyQualifiedName!~IntegrationTests"

# Run only integration tests
dotnet test --filter "FullyQualifiedName~IntegrationTests"

# Run a specific test class
dotnet test --filter "ClassName=WeaviateTypesTests"
```

### Run Tests with Coverage Report

```powershell
dotnet test --collect:"XPlat Code Coverage"
```

## Test Details

### Unit Test Notes

Since the current `WeaviateClient` implementation does not support dependency injection, unit tests mainly verify:

* Methods can be invoked properly
* Expected error states are returned when no server is available
* Parameter validation logic
* Data type serialization/deserialization

### Integration Test Notes

Integration tests use the `Testcontainers.Weaviate` package to:

* Start a real Weaviate Docker container
* Test full workflows
* Validate interaction with a real Weaviate server

**Note**: Integration tests require Docker Desktop. The Weaviate Docker image will be pulled during the first run.

## Covered Features

### WeaviateClient Methods

* ✅ `CheckHealthAsync()` - Health check
* ✅ `CreateSchemaAsync()` - Create schema
* ✅ `GetSchemaAsync()` - Retrieve schema
* ✅ `InsertObjectAsync()` - Insert single object
* ✅ `BatchInsertAsync()` - Batch insert objects
* ✅ `VectorSearchAsync()` - Vector search
* ✅ `GetObjectAsync()` - Retrieve object
* ✅ `DeleteObjectAsync()` - Delete object
* ✅ `DeleteClassAsync()` - Delete class

### Data Types

* ✅ `WeaviateClass` - Schema class definition
* ✅ `WeaviateProperty` - Property definition
* ✅ `WeaviateSchema` - Schema response
* ✅ `WeaviateObject` - Object definition
* ✅ `WeaviateSearchResult` - Search result
