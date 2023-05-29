using NodaTime;

namespace Moda.Common.Tests.Sut.Extensions;
public class LocalDateExtensionsTests
{
    [Fact]
    public void ToInstant()
    {
        // Arrange
        var localDate = new LocalDate(2023, 1, 1);
        var expected = Instant.FromUtc(2023, 1, 1, 0, 0, 0);

        // Act
        var result = localDate.ToInstant();

        // Assert
        Assert.Equal(expected, result);
    }
}
