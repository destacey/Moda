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
}
