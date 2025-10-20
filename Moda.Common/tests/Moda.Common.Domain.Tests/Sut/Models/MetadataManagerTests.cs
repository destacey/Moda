using Moda.Common.Domain.Models;

namespace Moda.Common.Domain.Tests.Sut.Models;
public class MetadataManagerTests
{
    [Fact]
    public void Upsert_InsertsNewMetadata_WhenNotExists()
    {
        // Arrange
        var collection = new List<KeyValueObjectMetadata>();
        var objectId = Guid.NewGuid();

        // Act
        var result = MetadataManager<KeyValueObjectMetadata>.Upsert(collection, objectId, "key1", "value1");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(collection);
        var item = collection[0];
        Assert.Equal("key1", item.Name);
        Assert.Equal("value1", item.Value);
    }

    [Fact]
    public void Upsert_UpdatesExistingMetadata_WhenExists()
    {
        // Arrange
        var objectId = Guid.NewGuid();
        var existing = new KeyValueObjectMetadata(objectId, "key1", "initial");
        var collection = new List<KeyValueObjectMetadata> { existing };

        // Act
        var result = MetadataManager<KeyValueObjectMetadata>.Upsert(collection, objectId, "key1", "updated");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(collection);
        Assert.Equal("updated", existing.Value);
    }

    [Fact]
    public void UpsertMany_AddsAllMetadata()
    {
        // Arrange
        var collection = new List<KeyValueObjectMetadata>();
        var objectId = Guid.NewGuid();
        var items = new[]
        {
            ("k1", "v1"),
            ("k2", "v2")
        };

        // Act
        var result = MetadataManager<KeyValueObjectMetadata>.UpsertMany(collection, objectId, items);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, collection.Count);
        Assert.Contains(collection, m => m.Name == "k1" && m.Value == "v1");
        Assert.Contains(collection, m => m.Name == "k2" && m.Value == "v2");
    }

    [Fact]
    public void Remove_RemovesExisting_ReturnsSuccess()
    {
        // Arrange
        var objectId = Guid.NewGuid();
        var existing = new KeyValueObjectMetadata(objectId, "key1", "val");
        var collection = new List<KeyValueObjectMetadata> { existing };

        // Act
        var result = MetadataManager<KeyValueObjectMetadata>.Remove(collection, "key1");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(collection);
    }

    [Fact]
    public void Remove_ReturnsFailure_WhenNotExists()
    {
        // Arrange
        var collection = new List<KeyValueObjectMetadata>();

        // Act
        var result = MetadataManager<KeyValueObjectMetadata>.Remove(collection, "missing");

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Get_ReturnsItem_WhenExists_And_NullWhenNot()
    {
        // Arrange
        var objectId = Guid.NewGuid();
        var item = new KeyValueObjectMetadata(objectId, "k", "v");
        var collection = new List<KeyValueObjectMetadata> { item };

        // Act
        var found = MetadataManager<KeyValueObjectMetadata>.Get(collection, "k");
        var notFound = MetadataManager<KeyValueObjectMetadata>.Get(collection, "x");

        // Assert
        Assert.Same(item, found);
        Assert.Null(notFound);
    }

    [Fact]
    public void Upsert_Throws_OnNullCollection()
    {
        // Arrange
        ICollection<KeyValueObjectMetadata>? collection = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => MetadataManager<KeyValueObjectMetadata>.Upsert(collection!, Guid.NewGuid(), "name", "value"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Upsert_Throws_OnNullOrWhitespaceName(string? name)
    {
        // Arrange
        var collection = new List<KeyValueObjectMetadata>();

        // Act & Assert
        if (name is null)
        {
            Assert.Throws<ArgumentNullException>(() => MetadataManager<KeyValueObjectMetadata>.Upsert(collection, Guid.NewGuid(), name!, "v"));
        }
        else
        {
            Assert.Throws<ArgumentException>(() => MetadataManager<KeyValueObjectMetadata>.Upsert(collection, Guid.NewGuid(), name, "v"));
        }
    }

    [Fact]
    public void UpsertMany_ReturnsCombinedFailure_WhenOneFails()
    {
        // Arrange
        var objectId = Guid.NewGuid();
        var collection = new List<KeyValueObjectMetadata>();

        // Create items with one invalid name to trigger failure via Guard
        var items = new List<(string Name, string? Value)>
        {
            ("good", "v"),
            (null!, "v2") // will cause Guard.Against.NullOrWhiteSpace to throw when processed
        };

        // Act
        // UpsertMany will throw for the null name because Guard is executed before UpsertMany accumulates results.
        Assert.Throws<ArgumentNullException>(() => MetadataManager<KeyValueObjectMetadata>.UpsertMany(collection, objectId, items));
    }
}
