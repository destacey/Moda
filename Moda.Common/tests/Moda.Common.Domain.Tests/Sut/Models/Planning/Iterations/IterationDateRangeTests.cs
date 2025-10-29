using Moda.Common.Domain.Models.Planning.Iterations;

namespace Moda.Common.Domain.Tests.Sut.Models.Planning.Iterations;

public sealed class IterationDateRangeTests
{
    [Fact]
    public void Constructor_ShouldSetStartAndEnd_WhenValidInstantsProvided()
    {
        // Arrange
        var start = Instant.FromUtc(2025, 1, 1, 0, 0);
        var end = Instant.FromUtc(2025, 12, 31, 23, 59);

        // Act
        var range = new IterationDateRange(start, end);

        // Assert
        range.Start.Should().Be(start);
        range.End.Should().Be(end);
    }

    [Fact]
    public void Constructor_ShouldAllowNullStartAndEnd()
    {
        // Act
        var range = new IterationDateRange(null, null);

        // Assert
        range.Start.Should().BeNull();
        range.End.Should().BeNull();
        range.EffectiveStart.Should().Be(Instant.MinValue);
        range.EffectiveEnd.Should().Be(Instant.MaxValue);
    }

    [Fact]
    public void Constructor_ShouldAllowNullStart_WhenEndProvided()
    {
        // Arrange
        var end = Instant.FromUtc(2025, 12, 31, 23, 59);

        // Act
        var range = new IterationDateRange(null, end);

        // Assert
        range.Start.Should().BeNull();
        range.End.Should().Be(end);
        range.EffectiveStart.Should().Be(Instant.MinValue);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenBothStartAndEndProvidedAndEndIsBeforeStart()
    {
        // Arrange
        var start = Instant.FromUtc(2025, 1, 1, 0, 0);
        var end = Instant.FromUtc(2024, 12, 31, 23, 59);

        // Act
        Action act = () => new IterationDateRange(start, end);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("The start date must be on or before the end date.*");
    }

    [Fact]
    public void Includes_ShouldReturnTrue_WhenValueIsWithinRange()
    {
        // Arrange
        var range = new IterationDateRange(
            Instant.FromUtc(2025, 1, 1, 0, 0),
            Instant.FromUtc(2025, 12, 31, 23, 59)
        );
        var value = Instant.FromUtc(2025, 6, 15, 12, 0);

        // Act
        var result = range.Includes(value);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Includes_ShouldReturnFalse_WhenValueIsOutsideRange()
    {
        // Arrange
        var range = new IterationDateRange(
            Instant.FromUtc(2025, 1, 1, 0, 0),
            Instant.FromUtc(2025, 12, 31, 23, 59)
        );
        var value = Instant.FromUtc(2024, 12, 31, 23, 59);

        // Act
        var result = range.Includes(value);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Includes_ShouldTreatNullAsInstantMinValue_WhenValueIsNull()
    {
        // Arrange
        var rangeWithStart = new IterationDateRange(Instant.FromUtc(2025, 1, 1, 0, 0), Instant.FromUtc(2025, 12, 31, 23, 59));
        var rangeWithNullStart = new IterationDateRange(null, Instant.FromUtc(2025, 12, 31, 23, 59));

        // Act
        var resultWhenStartPresent = rangeWithStart.Includes((Instant?)null);
        var resultWhenStartNull = rangeWithNullStart.Includes((Instant?)null);

        // Assert
        resultWhenStartPresent.Should().BeFalse();
        resultWhenStartNull.Should().BeTrue();
    }

    [Fact]
    public void Overlaps_ShouldReturnTrue_WhenRangesOverlap()
    {
        // Arrange
        var range1 = new IterationDateRange(
            Instant.FromUtc(2025, 1, 1, 0, 0),
            Instant.FromUtc(2025, 12, 31, 23, 59)
        );
        var range2 = new IterationDateRange(
            Instant.FromUtc(2025, 6, 1, 0, 0),
            Instant.FromUtc(2026, 6, 1, 23, 59)
        );

        // Act
        var result = range1.Overlaps(range2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Overlaps_ShouldReturnFalse_WhenRangesDoNotOverlap()
    {
        // Arrange
        var range1 = new IterationDateRange(
            Instant.FromUtc(2025, 1, 1, 0, 0),
            Instant.FromUtc(2025, 12, 31, 23, 59)
        );
        var range2 = new IterationDateRange(
            Instant.FromUtc(2026, 1, 1, 0, 0),
            Instant.FromUtc(2026, 12, 31, 23, 59)
        );

        // Act
        var result = range1.Overlaps(range2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Days_ShouldReturnCorrectNumberOfDays()
    {
        // Arrange
        var range = new IterationDateRange(
            Instant.FromUtc(2025, 1, 1, 0, 0),
            Instant.FromUtc(2025, 1, 31, 23, 59)
        );

        // Act
        var days = range.Days;

        // Assert
        days.Should().Be(31);
    }

    [Fact]
    public void Days_ShouldReturnMaxDays_WhenEndIsNull()
    {
        // Arrange
        var start = Instant.FromUtc(2025, 1, 1, 0, 0);
        var range = new IterationDateRange(start, null);

        // Act
        var days = range.Days;

        // Assert
        days.Should().Be((Instant.MaxValue - start).Days + 1);
    }

    [Fact]
    public void IsPastOn_ShouldReturnTrue_WhenDateIsAfterEffectiveEnd()
    {
        // Arrange
        var range = new IterationDateRange(
            Instant.FromUtc(2025, 1, 1, 0, 0),
            Instant.FromUtc(2025, 6, 30, 23, 59)
        );
        var date = Instant.FromUtc(2025, 7, 1, 0, 0);

        // Act
        var result = range.IsPastOn(date);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsPastOn_ShouldReturnFalse_WhenDateIsBeforeEffectiveEnd()
    {
        // Arrange
        var range = new IterationDateRange(
            Instant.FromUtc(2025, 1, 1, 0, 0),
            Instant.FromUtc(2025, 6, 30, 23, 59)
        );
        var date = Instant.FromUtc(2025, 6, 29, 23, 59);

        // Act
        var result = range.IsPastOn(date);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsActiveOn_ShouldReturnTrue_WhenDateIsWithinRange()
    {
        // Arrange
        var range = new IterationDateRange(
            Instant.FromUtc(2025, 1, 1, 0, 0),
            Instant.FromUtc(2025, 12, 31, 23, 59)
        );
        var date = Instant.FromUtc(2025, 6, 15, 12, 0);

        // Act
        var result = range.IsActiveOn(date);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsFutureOn_ShouldReturnTrue_WhenDateIsBeforeStart()
    {
        // Arrange
        var range = new IterationDateRange(Instant.FromUtc(2025, 1, 1, 0, 0), null);
        var date = Instant.FromUtc(2024, 12, 31, 23, 59);

        // Act
        var result = range.IsFutureOn(date);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsFutureOn_ShouldReturnFalse_WhenDateIsAfterStart()
    {
        // Arrange
        var range = new IterationDateRange(Instant.FromUtc(2025, 1, 1, 0, 0), null);
        var date = Instant.FromUtc(2025, 1, 2, 0, 0);

        // Act
        var result = range.IsFutureOn(date);

        // Assert
        result.Should().BeFalse();
    }
}
