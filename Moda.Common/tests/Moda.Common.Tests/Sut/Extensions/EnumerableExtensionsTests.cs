using FluentAssertions;

namespace Moda.Common.Tests.Sut.Extensions;
public class EnumerableExtensionsTests
{
    [Theory]
    [InlineData(100, 10, 10, 10)]
    [InlineData(27, 5, 6, 2)]
    [InlineData(9, 10, 1, 9)]
    [InlineData(10, 10, 1, 10)]
    public void Batch(int count, int batchSize, int expectedBatches, int lastBatchSize)
    {
        // Arrange
        var source = Enumerable.Range(1, count);

        // Act
        var result = source.Batch(batchSize);

        // Assert
        result.Should().HaveCount(expectedBatches);
        result.Last().Should().HaveCount(lastBatchSize);
    }

    [Fact]
    public void Batch_WhenNullSource_ThrowsException()
    {
        // Arrange
        List<int> source = null!;

        // Assert
        Assert.Throws<ArgumentNullException>(() => source!.Batch(10).ToList());
    }

    [Fact]
    public void Batch_WhenEmptySource_ThrowsException()
    {
        // Arrange
        List<int> source = new();

        // Act
        var result = source.Batch(10);

        // Assert
        result.Should().HaveCount(0);
    }

    [Theory]
    [MemberData(nameof(NotNullAndAnyArrayData))]
    public void NotNullAndAny_WithArray(IEnumerable<int> source, bool expectedResult)
    {
        // Act
        var result = source.NotNullAndAny();

        // Assert
        result.Should().Be(expectedResult);
    }

    public static IEnumerable<object[]> NotNullAndAnyArrayData
    {
        get
        {
            yield return new object[] { new int[] { 1, 2, 3 }, true };
            yield return new object[] { new int[] { 1}, true };
            yield return new object[] { new int[5], true };
            yield return new object[] { Array.Empty<int>(), false };
            yield return new object[] { null!, false };
        }
    }

    [Theory]
    [MemberData(nameof(NotNullAndAnyListData))]
    public void NotNullAndAny_WithList(IEnumerable<int> source, bool expectedResult)
    {
        // Act
        var result = source.NotNullAndAny();

        // Assert
        result.Should().Be(expectedResult);
    }

    // TODO Guid not IXunitSerializable, only shows as one test
    public static IEnumerable<object[]> NotNullAndAnyListData
    {
        get
        {
            yield return new object[] { new List<int>() { 1, 2, 3 }, true };
            yield return new object[] { new List<int>() { 1 }, true };
            yield return new object[] { new List<int>(5), false };
            yield return new object[] { Enumerable.Empty<int>(), false };
            yield return new object[] { null!, false };
        }
    }
}
