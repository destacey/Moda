using Moda.Common.Domain.Models;

namespace Moda.Common.Domain.Tests.Sut.Models;
public class MetadataAccessorTests
{
    [Fact]
    public void Upsert_InsertsNewMetadata_WhenNotExists()
    {
        // Arrange
        var objectId = Guid.NewGuid();
        var collection = new List<KeyValueObjectMetadata>();
        var accessor = new MetadataAccessor<KeyValueObjectMetadata>(() => objectId, () => collection);

        // Act
        var result = accessor.Upsert("key1", "value1");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(collection);
        var item = collection[0];
        Assert.Equal("key1", item.Name);
        Assert.Equal("value1", item.Value);
        Assert.Equal(objectId, item.ObjectId);
    }

    [Fact]
    public void Upsert_UpdatesExistingMetadata_WhenExists()
    {
        // Arrange
        var objectId = Guid.NewGuid();
        var existing = new KeyValueObjectMetadata(objectId, "key1", "initial");
        var collection = new List<KeyValueObjectMetadata> { existing };
        var accessor = new MetadataAccessor<KeyValueObjectMetadata>(() => objectId, () => collection);

        // Act
        var result = accessor.Upsert("key1", "updated");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(collection);
        Assert.Equal("updated", existing.Value);
    }

    [Fact]
    public void UpsertMany_AddsAllMetadata()
    {
        // Arrange
        var objectId = Guid.NewGuid();
        var collection = new List<KeyValueObjectMetadata>();
        var accessor = new MetadataAccessor<KeyValueObjectMetadata>(() => objectId, () => collection);
        var items = new[]
        {
            ("k1", "v1"),
            ("k2", "v2")
        };

        // Act
        var result = accessor.UpsertMany(items!);

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
        var accessor = new MetadataAccessor<KeyValueObjectMetadata>(() => objectId, () => collection);

        // Act
        var result = accessor.Remove("key1");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(collection);
    }

    [Fact]
    public void Remove_ReturnsFailure_WhenNotExists()
    {
        // Arrange
        var objectId = Guid.NewGuid();
        var collection = new List<KeyValueObjectMetadata>();
        var accessor = new MetadataAccessor<KeyValueObjectMetadata>(() => objectId, () => collection);

        // Act
        var result = accessor.Remove("missing");

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
        var accessor = new MetadataAccessor<KeyValueObjectMetadata>(() => objectId, () => collection);

        // Act
        var found = accessor.Get("k");
        var notFound = accessor.Get("x");

        // Assert
        Assert.Same(item, found);
        Assert.Null(notFound);
    }

    [Fact]
    public void GetAll_ReturnsCurrentCollectionSnapshot()
    {
        // Arrange
        var objectId = Guid.NewGuid();
        var collection = new List<KeyValueObjectMetadata>
        {
            new(objectId, "a", "1"),
            new(objectId, "b", "2")
        };
        var accessor = new MetadataAccessor<KeyValueObjectMetadata>(() => objectId, () => collection);

        // Act
        var snapshot = accessor.GetAll();

        // Assert
        Assert.Equal(2, snapshot.Count);
        Assert.Contains(snapshot, m => m.Name == "a" && m.Value == "1");
        Assert.Contains(snapshot, m => m.Name == "b" && m.Value == "2");
    }

    [Fact]
    public void CollectionProvider_ReflectsReplacedCollectionInstance()
    {
        // Arrange
        var objectId = Guid.NewGuid();
        List<KeyValueObjectMetadata> current = [];
        var accessor = new MetadataAccessor<KeyValueObjectMetadata>(() => objectId, () => current);

        // Initially empty
        Assert.Empty(accessor.GetAll());

        // Simulate EF replacing collection instance
        current =
        [
            new(objectId, "newKey", "newValue")
        ];

        // Act
        var snapshot = accessor.GetAll();

        // Assert
        Assert.Single(snapshot);
        Assert.Contains(snapshot, m => m.Name == "newKey" && m.Value == "newValue");
    }

    [Fact]
    public void Factory_IsUsed_WhenProvided()
    {
        // Arrange
        var objectId = Guid.NewGuid();
        var collection = new List<KeyValueObjectMetadata>();
        bool factoryCalled = false;

        KeyValueObjectMetadata Factory(Guid id, string name, string? value)
        {
            factoryCalled = true;
            return new KeyValueObjectMetadata(id, name + "_f", value);
        }

        var accessor = new MetadataAccessor<KeyValueObjectMetadata>(() => objectId, () => collection, Factory);

        // Act
        var result = accessor.Upsert("k", "v");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(factoryCalled);
        Assert.Single(collection);
        Assert.Equal("k_f", collection[0].Name);
    }
}
