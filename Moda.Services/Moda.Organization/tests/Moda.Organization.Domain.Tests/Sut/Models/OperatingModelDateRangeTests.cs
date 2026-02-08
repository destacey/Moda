using Moda.Organization.Domain.Models;
using NodaTime;

namespace Moda.Organization.Domain.Tests.Sut.Models;

public class OperatingModelDateRangeTests
{
    #region Constructor

    [Fact]
    public void New_WithGap_Success()
    {
        // ARRANGE
        LocalDate start = new(2020, 1, 1);
        LocalDate? end = new(2020, 2, 1);

        // ACT
        var result = new OperatingModelDateRange(start, end);

        // ASSERT
        result.Start.Should().Be(start);
        result.End.Should().Be(end);
        result.IsCurrent.Should().BeFalse();
    }

    [Fact]
    public void New_NoGap_Success()
    {
        // ARRANGE
        LocalDate start = new(2020, 1, 1);
        LocalDate? end = new(2020, 1, 2);

        // ACT
        var result = new OperatingModelDateRange(start, end);

        // ASSERT
        result.Start.Should().Be(start);
        result.End.Should().Be(end);
        result.IsCurrent.Should().BeFalse();
    }

    [Fact]
    public void New_SameValue_Success()
    {
        // ARRANGE
        LocalDate start = new(2020, 1, 1);
        LocalDate? end = new(2020, 1, 1);

        // ACT
        var result = new OperatingModelDateRange(start, end);

        // ASSERT
        result.Start.Should().Be(start);
        result.End.Should().Be(end);
        result.IsCurrent.Should().BeFalse();
    }

    [Fact]
    public void New_NullEnd_Success()
    {
        // ARRANGE
        LocalDate start = new(2020, 1, 1);

        // ACT
        var result = new OperatingModelDateRange(start, null);

        // ASSERT
        result.Start.Should().Be(start);
        result.End.Should().BeNull();
        result.IsCurrent.Should().BeTrue();
    }

    [Fact]
    public void New_EarlierEnd_Throws()
    {
        // ARRANGE
        LocalDate start = new(2020, 1, 2);
        LocalDate? end = new(2020, 1, 1);

        // ACT & ASSERT
        var exception = Assert.Throws<ArgumentException>(() => new OperatingModelDateRange(start, end));

        // ASSERT
        exception.Message.Should().Contain("The start date must be on or before the end date.");
        exception.ParamName.Should().Be("start");
    }

    [Fact]
    public void New_NullStart_Throws()
    {
        // ARRANGE
        LocalDate? start = null;
        LocalDate? end = new(2020, 1, 1);

        // ACT & ASSERT
        var exception = Assert.Throws<InvalidOperationException>(() => new OperatingModelDateRange((LocalDate)start!, end));

        // ASSERT
        exception.Message.Should().Be("Nullable object must have a value.");
    }

    #endregion Constructor

    #region IsCurrent

    [Fact]
    public void IsCurrent_WithNullEnd_ReturnsTrue()
    {
        // ARRANGE
        var range = new OperatingModelDateRange(new LocalDate(2020, 1, 1), null);

        // ACT & ASSERT
        range.IsCurrent.Should().BeTrue();
    }

    [Fact]
    public void IsCurrent_WithEnd_ReturnsFalse()
    {
        // ARRANGE
        var range = new OperatingModelDateRange(new LocalDate(2020, 1, 1), new LocalDate(2020, 12, 31));

        // ACT & ASSERT
        range.IsCurrent.Should().BeFalse();
    }

    #endregion IsCurrent

    #region Includes

    [Theory]
    [MemberData(nameof(Includes_SingleDateData))]
    public void Includes_SingleDate(LocalDate start, LocalDate? end, LocalDate dateToCheck, bool expected)
    {
        // ARRANGE
        var range = new OperatingModelDateRange(start, end);

        // ACT
        var result = range.Includes(dateToCheck);

        // ASSERT
        result.Should().Be(expected);
    }

    public static IEnumerable<object[]> Includes_SingleDateData()
    {
        // Date on start boundary
        yield return new object[]
        {
            new LocalDate(2020, 1, 1),
            new LocalDate(2020, 1, 10),
            new LocalDate(2020, 1, 1),
            true
        };

        // Date on end boundary
        yield return new object[]
        {
            new LocalDate(2020, 1, 1),
            new LocalDate(2020, 1, 10),
            new LocalDate(2020, 1, 10),
            true
        };

        // Date within range
        yield return new object[]
        {
            new LocalDate(2020, 1, 1),
            new LocalDate(2020, 1, 10),
            new LocalDate(2020, 1, 5),
            true
        };

        // Single day range
        yield return new object[]
        {
            new LocalDate(2020, 1, 1),
            new LocalDate(2020, 1, 1),
            new LocalDate(2020, 1, 1),
            true
        };

        // Date after range
        yield return new object[]
        {
            new LocalDate(2020, 1, 1),
            new LocalDate(2020, 1, 10),
            new LocalDate(2020, 1, 11),
            false
        };

        // Date before range
        yield return new object[]
        {
            new LocalDate(2020, 1, 1),
            new LocalDate(2020, 1, 10),
            new LocalDate(2019, 12, 31),
            false
        };

        // Null end - date after start (current model)
        yield return new object[]
        {
            new LocalDate(2020, 1, 1),
            null!,
            new LocalDate(2020, 3, 1),
            true
        };

        // Null end - date on start
        yield return new object[]
        {
            new LocalDate(2020, 1, 1),
            null!,
            new LocalDate(2020, 1, 1),
            true
        };

        // Null end - date before start
        yield return new object[]
        {
            new LocalDate(2020, 3, 1),
            null!,
            new LocalDate(2020, 1, 1),
            false
        };
    }

    #endregion Includes

    #region Overlaps

    [Theory]
    [MemberData(nameof(Overlaps_Data))]
    public void Overlaps_VariousScenarios(OperatingModelDateRange range1, OperatingModelDateRange range2, bool expected)
    {
        // ACT
        var result = range1.Overlaps(range2);

        // ASSERT
        result.Should().Be(expected);
    }

    public static IEnumerable<object[]> Overlaps_Data()
    {
        // Exact same range
        yield return new object[]
        {
            new OperatingModelDateRange(new LocalDate(2020, 1, 1), new LocalDate(2020, 1, 10)),
            new OperatingModelDateRange(new LocalDate(2020, 1, 1), new LocalDate(2020, 1, 10)),
            true
        };

        // Range2 inside Range1
        yield return new object[]
        {
            new OperatingModelDateRange(new LocalDate(2020, 1, 1), new LocalDate(2020, 1, 10)),
            new OperatingModelDateRange(new LocalDate(2020, 1, 3), new LocalDate(2020, 1, 7)),
            true
        };

        // Range1 inside Range2
        yield return new object[]
        {
            new OperatingModelDateRange(new LocalDate(2020, 1, 3), new LocalDate(2020, 1, 7)),
            new OperatingModelDateRange(new LocalDate(2020, 1, 1), new LocalDate(2020, 1, 10)),
            true
        };

        // Overlapping at end
        yield return new object[]
        {
            new OperatingModelDateRange(new LocalDate(2020, 1, 1), new LocalDate(2020, 1, 10)),
            new OperatingModelDateRange(new LocalDate(2020, 1, 10), new LocalDate(2020, 1, 20)),
            true
        };

        // Overlapping at start
        yield return new object[]
        {
            new OperatingModelDateRange(new LocalDate(2020, 1, 10), new LocalDate(2020, 1, 20)),
            new OperatingModelDateRange(new LocalDate(2020, 1, 1), new LocalDate(2020, 1, 10)),
            true
        };

        // Single day overlap
        yield return new object[]
        {
            new OperatingModelDateRange(new LocalDate(2020, 1, 1), new LocalDate(2020, 1, 5)),
            new OperatingModelDateRange(new LocalDate(2020, 1, 5), new LocalDate(2020, 1, 10)),
            true
        };

        // Adjacent but not overlapping (gap of 1 day)
        yield return new object[]
        {
            new OperatingModelDateRange(new LocalDate(2020, 1, 1), new LocalDate(2020, 1, 5)),
            new OperatingModelDateRange(new LocalDate(2020, 1, 6), new LocalDate(2020, 1, 10)),
            false
        };

        // Completely separate ranges
        yield return new object[]
        {
            new OperatingModelDateRange(new LocalDate(2020, 1, 1), new LocalDate(2020, 1, 5)),
            new OperatingModelDateRange(new LocalDate(2020, 1, 10), new LocalDate(2020, 1, 15)),
            false
        };

        // Range1 before Range2
        yield return new object[]
        {
            new OperatingModelDateRange(new LocalDate(2020, 1, 1), new LocalDate(2020, 1, 5)),
            new OperatingModelDateRange(new LocalDate(2020, 2, 1), new LocalDate(2020, 2, 10)),
            false
        };

        // Both ranges have null end (both current) - always overlap
        yield return new object[]
        {
            new OperatingModelDateRange(new LocalDate(2020, 1, 1), null),
            new OperatingModelDateRange(new LocalDate(2020, 6, 1), null),
            true
        };

        // Range1 has null end, Range2 has end before Range1 start - no overlap
        yield return new object[]
        {
            new OperatingModelDateRange(new LocalDate(2020, 6, 1), null),
            new OperatingModelDateRange(new LocalDate(2020, 1, 1), new LocalDate(2020, 5, 31)),
            false
        };

        // Range1 has null end, Range2 has end on Range1 start - overlap
        yield return new object[]
        {
            new OperatingModelDateRange(new LocalDate(2020, 6, 1), null),
            new OperatingModelDateRange(new LocalDate(2020, 1, 1), new LocalDate(2020, 6, 1)),
            true
        };

        // Range1 has null end, Range2 has end after Range1 start - overlap
        yield return new object[]
        {
            new OperatingModelDateRange(new LocalDate(2020, 1, 1), null),
            new OperatingModelDateRange(new LocalDate(2020, 1, 1), new LocalDate(2020, 6, 1)),
            true
        };

        // Range1 has end, Range2 has null end before Range1 end - overlap
        yield return new object[]
        {
            new OperatingModelDateRange(new LocalDate(2020, 1, 1), new LocalDate(2020, 12, 31)),
            new OperatingModelDateRange(new LocalDate(2020, 6, 1), null),
            true
        };

        // Range1 has end, Range2 has null end after Range1 end - no overlap
        yield return new object[]
        {
            new OperatingModelDateRange(new LocalDate(2020, 1, 1), new LocalDate(2020, 5, 31)),
            new OperatingModelDateRange(new LocalDate(2020, 6, 1), null),
            false
        };
    }

    #endregion Overlaps

    #region SetEnd

    [Fact]
    public void SetEnd_WithValidEndDate_Success()
    {
        // ARRANGE
        var start = new LocalDate(2020, 1, 1);
        var range = new OperatingModelDateRange(start, null);
        var endDate = new LocalDate(2020, 12, 31);

        // ACT
        range.SetEnd(endDate);

        // ASSERT
        range.End.Should().Be(endDate);
        range.IsCurrent.Should().BeFalse();
    }

    [Fact]
    public void SetEnd_WithEndEqualToStart_Success()
    {
        // ARRANGE
        var start = new LocalDate(2020, 1, 1);
        var range = new OperatingModelDateRange(start, null);
        var endDate = new LocalDate(2020, 1, 1);

        // ACT
        range.SetEnd(endDate);

        // ASSERT
        range.End.Should().Be(endDate);
        range.IsCurrent.Should().BeFalse();
    }

    [Fact]
    public void SetEnd_WithEndBeforeStart_Throws()
    {
        // ARRANGE
        var start = new LocalDate(2020, 1, 10);
        var range = new OperatingModelDateRange(start, null);
        var endDate = new LocalDate(2020, 1, 9);

        // ACT & ASSERT
        var exception = Assert.Throws<ArgumentException>(() => range.SetEnd(endDate));

        // ASSERT
        exception.Message.Should().Contain("The end date cannot be before the start date.");
        exception.ParamName.Should().Be("endDate");
    }

    [Fact]
    public void SetEnd_OnAlreadyClosedRange_UpdatesEnd()
    {
        // ARRANGE
        var start = new LocalDate(2020, 1, 1);
        var originalEnd = new LocalDate(2020, 6, 30);
        var range = new OperatingModelDateRange(start, originalEnd);
        var newEnd = new LocalDate(2020, 12, 31);

        // ACT
        range.SetEnd(newEnd);

        // ASSERT
        range.End.Should().Be(newEnd);
        range.IsCurrent.Should().BeFalse();
    }

    #endregion SetEnd

    #region Equality

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        // ARRANGE
        var range1 = new OperatingModelDateRange(new LocalDate(2020, 1, 1), new LocalDate(2020, 12, 31));
        var range2 = new OperatingModelDateRange(new LocalDate(2020, 1, 1), new LocalDate(2020, 12, 31));

        // ACT & ASSERT
        range1.Should().Be(range2);
        (range1 == range2).Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentStart_AreNotEqual()
    {
        // ARRANGE
        var range1 = new OperatingModelDateRange(new LocalDate(2020, 1, 1), new LocalDate(2020, 12, 31));
        var range2 = new OperatingModelDateRange(new LocalDate(2020, 1, 2), new LocalDate(2020, 12, 31));

        // ACT & ASSERT
        range1.Should().NotBe(range2);
        (range1 != range2).Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentEnd_AreNotEqual()
    {
        // ARRANGE
        var range1 = new OperatingModelDateRange(new LocalDate(2020, 1, 1), new LocalDate(2020, 12, 31));
        var range2 = new OperatingModelDateRange(new LocalDate(2020, 1, 1), new LocalDate(2020, 12, 30));

        // ACT & ASSERT
        range1.Should().NotBe(range2);
        (range1 != range2).Should().BeTrue();
    }

    [Fact]
    public void Equality_OneNullEnd_AreNotEqual()
    {
        // ARRANGE
        var range1 = new OperatingModelDateRange(new LocalDate(2020, 1, 1), null);
        var range2 = new OperatingModelDateRange(new LocalDate(2020, 1, 1), new LocalDate(2020, 12, 31));

        // ACT & ASSERT
        range1.Should().NotBe(range2);
        (range1 != range2).Should().BeTrue();
    }

    [Fact]
    public void Equality_BothNullEnd_AreEqual()
    {
        // ARRANGE
        var range1 = new OperatingModelDateRange(new LocalDate(2020, 1, 1), null);
        var range2 = new OperatingModelDateRange(new LocalDate(2020, 1, 1), null);

        // ACT & ASSERT
        range1.Should().Be(range2);
        (range1 == range2).Should().BeTrue();
    }

    #endregion Equality
}
