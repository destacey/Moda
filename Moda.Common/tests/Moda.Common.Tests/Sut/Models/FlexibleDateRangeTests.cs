using Moda.Common.Models;
using NodaTime;

namespace Moda.Common.Tests.Sut.Models;
public sealed class FlexibleDateRangeTests
{
    [Fact]
    public void Constructor_ShouldSetStartAndEnd_WhenValidDatesProvided()
    {
        // Arrange
        var start = new LocalDate(2025, 1, 1);
        var end = new LocalDate(2025, 12, 31);

        // Act
        var range = new FlexibleDateRange(start, end);

        // Assert
        range.Start.Should().Be(start);
        range.End.Should().Be(end);
    }

    [Fact]
    public void Constructor_ShouldSetEndToNull_WhenEndNotProvided()
    {
        // Arrange
        var start = new LocalDate(2025, 1, 1);

        // Act
        var range = new FlexibleDateRange(start);

        // Assert
        range.End.Should().BeNull();
        range.EffectiveEnd.Should().Be(LocalDate.MaxIsoValue);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenEndIsBeforeStart()
    {
        // Arrange
        var start = new LocalDate(2025, 1, 1);
        var end = new LocalDate(2024, 12, 31);

        // Act
        Action act = () => new FlexibleDateRange(start, end);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("The start date must be on or before the end date.*");
    }

    [Fact]
    public void Includes_ShouldReturnTrue_WhenValueIsWithinRange()
    {
        // Arrange
        var range = new FlexibleDateRange(new LocalDate(2025, 1, 1), new LocalDate(2025, 12, 31));
        var value = new LocalDate(2025, 6, 15);

        // Act
        var result = range.Includes(value);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Includes_ShouldReturnFalse_WhenValueIsOutsideRange()
    {
        // Arrange
        var range = new FlexibleDateRange(new LocalDate(2025, 1, 1), new LocalDate(2025, 12, 31));
        var value = new LocalDate(2024, 12, 31);

        // Act
        var result = range.Includes(value);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Overlaps_ShouldReturnTrue_WhenRangesOverlap()
    {
        // Arrange
        var range1 = new FlexibleDateRange(new LocalDate(2025, 1, 1), new LocalDate(2025, 12, 31));
        var range2 = new LocalDateRange(new LocalDate(2025, 6, 1), new LocalDate(2026, 6, 1));

        // Act
        var result = range1.Overlaps(range2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Overlaps_ShouldReturnFalse_WhenRangesDoNotOverlap()
    {
        // Arrange
        var range1 = new FlexibleDateRange(new LocalDate(2025, 1, 1), new LocalDate(2025, 12, 31));
        var range2 = new LocalDateRange(new LocalDate(2026, 1, 1), new LocalDate(2026, 12, 31));

        // Act
        var result = range1.Overlaps(range2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Days_ShouldReturnCorrectNumberOfDays()
    {
        // Arrange
        var range = new FlexibleDateRange(new LocalDate(2025, 1, 1), new LocalDate(2025, 1, 31));

        // Act
        var days = range.Days;

        // Assert
        days.Should().Be(31);
    }

    [Fact]
    public void Days_ShouldReturnMaxDays_WhenEndIsNull()
    {
        // Arrange
        var range = new FlexibleDateRange(new LocalDate(2025, 1, 1));

        // Act
        var days = range.Days;

        // Assert
        days.Should().Be(Period.DaysBetween(new LocalDate(2025, 1, 1), LocalDate.MaxIsoValue) + 1);
    }

    [Fact]
    public void IsPastOn_ShouldReturnTrue_WhenDateIsAfterEffectiveEnd()
    {
        // Arrange
        var range = new FlexibleDateRange(new LocalDate(2025, 1, 1), new LocalDate(2025, 6, 30));
        var date = new LocalDate(2025, 7, 1);

        // Act
        var result = range.IsPastOn(date);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsPastOn_ShouldReturnFalse_WhenDateIsBeforeEffectiveEnd()
    {
        // Arrange
        var range = new FlexibleDateRange(new LocalDate(2025, 1, 1), new LocalDate(2025, 6, 30));
        var date = new LocalDate(2025, 6, 29);

        // Act
        var result = range.IsPastOn(date);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsActiveOn_ShouldReturnTrue_WhenDateIsWithinRange()
    {
        // Arrange
        var range = new FlexibleDateRange(new LocalDate(2025, 1, 1), new LocalDate(2025, 12, 31));
        var date = new LocalDate(2025, 6, 15);

        // Act
        var result = range.IsActiveOn(date);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsFutureOn_ShouldReturnTrue_WhenDateIsBeforeStart()
    {
        // Arrange
        var range = new FlexibleDateRange(new LocalDate(2025, 1, 1));
        var date = new LocalDate(2024, 12, 31);

        // Act
        var result = range.IsFutureOn(date);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsFutureOn_ShouldReturnFalse_WhenDateIsAfterStart()
    {
        // Arrange
        var range = new FlexibleDateRange(new LocalDate(2025, 1, 1));
        var date = new LocalDate(2025, 1, 2);

        // Act
        var result = range.IsFutureOn(date);

        // Assert
        result.Should().BeFalse();
    }
}
