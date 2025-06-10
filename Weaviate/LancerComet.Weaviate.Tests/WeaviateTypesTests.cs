namespace LancerComet.Weaviate.Tests;

public class WeaviateTypesTests {
  [Fact]
  public void WeaviateClass_DefaultValues_ShouldBeCorrect () {
    // Arrange & Act
    var weaviateClass = new WeaviateClass();

    // Assert
    Assert.Equal("", weaviateClass.Class);
    Assert.Equal("", weaviateClass.Description);
    Assert.Empty(weaviateClass.Properties);
    Assert.Equal("none", weaviateClass.Vectorizer);
  }

  [Fact]
  public void WeaviateProperty_DefaultValues_ShouldBeCorrect () {
    // Arrange & Act
    var property = new WeaviateProperty();

    // Assert
    Assert.Equal("", property.Name);
    Assert.Empty(property.DataType);
    Assert.Equal("", property.Description);
  }

  [Fact]
  public void WeaviateSchema_DefaultValues_ShouldBeCorrect () {
    // Arrange & Act
    var schema = new WeaviateSchema();

    // Assert
    Assert.Empty(schema.Classes);
  }

  [Fact]
  public void WeaviateObject_DefaultValues_ShouldBeCorrect () {
    // Arrange & Act
    var weaviateObject = new WeaviateObject();

    // Assert
    Assert.Equal("", weaviateObject.Id);
    Assert.Equal("", weaviateObject.Class);
    Assert.Equal("", weaviateObject.Properties);
    Assert.Empty(weaviateObject.Vector);
  }

  [Fact]
  public void WeaviateClass_Serialization_ShouldWork () {
    // Arrange
    var weaviateClass = new WeaviateClass {
      Class = "Article",
      Description = "A news article",
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
      ],
      Vectorizer = "text2vec-contextionary"
    };

    // Act
    var json = JsonSerializer.Serialize(weaviateClass);
    var deserialized = JsonSerializer.Deserialize<WeaviateClass>(json);

    // Assert
    Assert.NotNull(deserialized);
    Assert.Equal(weaviateClass.Class, deserialized.Class);
    Assert.Equal(weaviateClass.Description, deserialized.Description);
    Assert.Equal(weaviateClass.Properties.Count, deserialized.Properties.Count);
    Assert.Equal(weaviateClass.Vectorizer, deserialized.Vectorizer);
  }

  [Fact]
  public void WeaviateSchema_Serialization_ShouldWork () {
    // Arrange
    var schema = new WeaviateSchema {
      Classes = [
        new WeaviateClass {
          Class = "Article",
          Description = "A news article"
        },
        new WeaviateClass {
          Class = "Author",
          Description = "An author"
        }
      ]
    };

    // Act
    var json = JsonSerializer.Serialize(schema);
    var deserialized = JsonSerializer.Deserialize<WeaviateSchema>(json);

    // Assert
    Assert.NotNull(deserialized);
    Assert.Equal(2, deserialized.Classes.Count);
    Assert.Equal("Article", deserialized.Classes[0].Class);
    Assert.Equal("Author", deserialized.Classes[1].Class);
  }
}
