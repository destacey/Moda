namespace Moda.Common.Tests.Sut.Extensions;
public class GenericExtensionsTests
{
    [Fact]
    public void FlattenHierarchy_WhenNoChildren_ReturnsRoot()
    {
        // Arrange
        DepthFirstFlattenTestModelWithNullableChildren root = new() { Name = "root" };

        // Act
        var result = root.FlattenHierarchy(n => n.Children);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be(root);
    }

    [Fact]
    public void FlattenHierarchy_WhenNullChildren_ReturnsRoot()
    {
        // Arrange
        DepthFirstFlattenTestModel root = new() { Name = "root" };

        // Act
        var result = root.FlattenHierarchy(n => n.Children);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be(root);
    }

    [Fact]
    public void FlattenHierarchy_WithFiveAncestors_ReturnsSix()
    {
        // Arrange
        DepthFirstFlattenTestModel twoOne = new() { Name = "twoOne" };
        DepthFirstFlattenTestModel twoTwo = new() { Name = "twoTwo" };
        DepthFirstFlattenTestModel threeOne = new() { Name = "threeOne" };
        DepthFirstFlattenTestModel threeTwo = new() { Name = "threeTwo" };

        DepthFirstFlattenTestModel one = new() { Name = "one" };
        DepthFirstFlattenTestModel two = new()
        {
            Name = "two",
            Children = new() { twoOne, twoTwo }
        };
        DepthFirstFlattenTestModel three = new()
        {
            Name = "three",
            Children = new() { threeOne, threeTwo }
        };

        DepthFirstFlattenTestModel root = new()
        {
            Name = "root",
            Children = new() { one, two, three }
        };

        // Act
        var result = root.FlattenHierarchy(n => n.Children);

        // Assert
        result.Should().HaveCount(8)
            .And.Contain(root)
            .And.Contain(one)
            .And.Contain(two)
            .And.Contain(twoOne)
            .And.Contain(twoTwo)
            .And.Contain(three)
            .And.Contain(threeOne)
            .And.Contain(threeTwo);
    }
}

file class DepthFirstFlattenTestModel
{
    public required string Name { get; set; }
    public List<DepthFirstFlattenTestModel> Children { get; set; } = [];
}

file class DepthFirstFlattenTestModelWithNullableChildren
{
    public required string Name { get; set; }
    public List<DepthFirstFlattenTestModelWithNullableChildren>? Children { get; set; }
}
