﻿using FluentAssertions;
using Moda.Common.Models;
using NodaTime;

namespace Moda.Common.Tests.Sut.Models;
public class LocalDateRangeTests
{
    [Fact]
    public void New_WithGap_Success()
    {
        // ARRANGE
        LocalDate start = new(2020, 1, 1);
        LocalDate end = new(2020, 2, 1);
        var expectedDays = 32;

        // ACT
        var result = new LocalDateRange(start, end);

        // ASSERT
        result.Start.Should().Be(start);
        result.End.Should().Be(end);
        result.Days.Should().Be(expectedDays);
    }

    [Fact]
    public void New_NoGap_Success()
    {
        // ARRANGE
        LocalDate start = new(2020, 1, 1);
        LocalDate end = new(2020, 1, 2);
        var expectedDays = 2;

        // ACT
        var result = new LocalDateRange(start, end);

        // ASSERT
        result.Start.Should().Be(start);
        result.End.Should().Be(end);
        result.Days.Should().Be(expectedDays);
    }

    [Fact]
    public void New_SameValue_Success()
    {
        // ARRANGE
        LocalDate start = new(2020, 1, 1);
        LocalDate end = new(2020, 1, 1);
        var expectedDays = 1;

        // ACT
        var result = new LocalDateRange(start, end);

        // ASSERT
        result.Start.Should().Be(start);
        result.End.Should().Be(end);
        result.Days.Should().Be(expectedDays);
    }

    [Fact]
    public void New_EarlierEnd_Throws()
    {
        // ARRANGE
        LocalDate start = new(2020, 1, 2);
        LocalDate end = new(2020, 1, 1);

        // ACT & ASSERT
        var excpetion = Assert.Throws<ArgumentException>(() => new LocalDateRange(start, end));

        // ASSERT
        excpetion.Message.Should().Be("The start date must be on or before the end date. (Parameter 'LocalDateRange')");
    }

    [Fact]
    public void New_NullStart_Throws()
    {
        // ARRANGE
        LocalDate? start = null;
        LocalDate end = new(2020, 1, 1);

        // ACT & ASSERT
        // The Guard.Against clause should return an ArgumentNullException, but it isn't
        var excpetion = Assert.Throws<InvalidOperationException>(() => new LocalDateRange((LocalDate)start!, end));

        // ASSERT
        excpetion.Message.Should().Be("Nullable object must have a value.");
    }

    [Fact]
    public void New_NullEnd_Throws()
    {
        // ARRANGE
        LocalDate start = new(2020, 1, 1);
        LocalDate? end = null;

        // ACT & ASSERT
        // The Guard.Against clause should return an ArgumentNullException, but it isn't
        var excpetion = Assert.Throws<InvalidOperationException>(() => new LocalDateRange(start, (LocalDate)end!));

        // ASSERT
        excpetion.Message.Should().Be("Nullable object must have a value.");
    }

    [Theory]
    [MemberData(nameof(Includes_SingleData))]
    public void Includes_Single(LocalDate start, LocalDate end, LocalDate localDate, bool expected)
    {
        // ACT
        var range = new LocalDateRange(start, end);
        var result = range.Includes(localDate);

        // ASSERT
        result.Should().Be(expected);
    }
    public static IEnumerable<object[]> Includes_SingleData()
    {
        yield return new object[]
        {
            new LocalDate(2020, 1, 1),
            new LocalDate(2020, 1, 10),
            new LocalDate(2020, 1, 1),
            true
        };
        yield return new object[]
        {
            new LocalDate(2020, 1, 1),
            new LocalDate(2020, 1, 10),
            new LocalDate(2020, 1, 10),
            true
        };
        yield return new object[]
        {
            new LocalDate(2020, 1, 1),
            new LocalDate(2020, 1, 10),
            new LocalDate(2020, 1, 2),
            true
        };
        yield return new object[]
        {
            new LocalDate(2020, 1, 1),
            new LocalDate(2020, 1, 1),
            new LocalDate(2020, 1, 1),
            true
        };
        yield return new object[]
        {
            new LocalDate(2020, 1, 1),
            new LocalDate(2020, 1, 10),
            new LocalDate(2020, 1, 11),
            false
        };
        yield return new object[]
        {
            new LocalDate(2020, 1, 1),
            new LocalDate(2020, 1, 10),
            new LocalDate(2019, 1, 1),
            false
        };
    }

    [Theory]
    [MemberData(nameof(Includes_RangeData))]
    public void Includes_Range(LocalDateRange range1, LocalDateRange range2, bool expected)
    {
        // ACT
        var result = range1.Includes(range2);

        // ASSERT
        result.Should().Be(expected);
    }
    public static IEnumerable<object[]> Includes_RangeData()
    {
        yield return new object[]
        {
            new LocalDateRange(
                new LocalDate(2020, 1, 1),
                new LocalDate(2020, 1, 10)),
            new LocalDateRange(
                new LocalDate(2020, 1, 1),
                new LocalDate(2020, 1, 10)),
            true
        };
        yield return new object[]
        {
            new LocalDateRange(
                new LocalDate(2020, 1, 1),
                new LocalDate(2020, 1, 10)),
            new LocalDateRange(
                new LocalDate(2020, 1, 5),
                new LocalDate(2020, 1, 7)),
            true
        };
        yield return new object[]
        {
            new LocalDateRange(
                new LocalDate(2020, 1, 1),
                new LocalDate(2020, 1, 10)),
            new LocalDateRange(
                new LocalDate(2020, 1, 1),
                new LocalDate(2020, 1, 9)),
            true
        };
        yield return new object[]
        {
            new LocalDateRange(
                new LocalDate(2020, 1, 1),
                new LocalDate(2020, 1, 10)),
            new LocalDateRange(
                new LocalDate(2020, 1, 9),
                new LocalDate(2020, 1, 10)),
            true
        };
        yield return new object[]
        {
            new LocalDateRange(
                new LocalDate(2020, 1, 1),
                new LocalDate(2020, 1, 1)),
            new LocalDateRange(
                new LocalDate(2020, 1, 1),
                new LocalDate(2020, 1, 1)),
            true
        };
        yield return new object[]
        {
            new LocalDateRange(
                new LocalDate(2020, 1, 2),
                new LocalDate(2020, 1, 10)),
            new LocalDateRange(
                new LocalDate(2020, 1, 1),
                new LocalDate(2020, 1, 10)),
            false
        };
        yield return new object[]
        {
            new LocalDateRange(
                new LocalDate(2020, 1, 1),
                new LocalDate(2020, 1, 10)),
            new LocalDateRange(
                new LocalDate(2020, 1, 1),
                new LocalDate(2020, 1, 11)),
            false
        };
        yield return new object[]
        {
            new LocalDateRange(
                new LocalDate(2020, 1, 10),
                new LocalDate(2020, 1, 20)),
            new LocalDateRange(
                new LocalDate(2020, 1, 1),
                new LocalDate(2020, 1, 9)),
            false
        };
        yield return new object[]
        {
            new LocalDateRange(
                new LocalDate(2020, 1, 1),
                new LocalDate(2020, 1, 9)),
            new LocalDateRange(
                new LocalDate(2020, 1, 10),
                new LocalDate(2020, 1, 20)),
            false
        };
    }

    [Theory]
    [MemberData(nameof(Overlaps_RangeData))]
    public void Overlaps_Range(LocalDateRange range1, LocalDateRange range2, bool expected)
    {
        // ACT
        var result = range1.Overlaps(range2);

        // ASSERT
        result.Should().Be(expected);
    }
    public static IEnumerable<object[]> Overlaps_RangeData()
    {
        yield return new object[]
        {
            new LocalDateRange(
                new LocalDate(2020, 1, 1),
                new LocalDate(2020, 1, 10)),
            new LocalDateRange(
                new LocalDate(2020, 1, 1),
                new LocalDate(2020, 1, 10)),
            true
        };
        yield return new object[]
        {
            new LocalDateRange(
                new LocalDate(2020, 1, 1),
                new LocalDate(2020, 1, 10)),
            new LocalDateRange(
                new LocalDate(2020, 1, 5),
                new LocalDate(2020, 1, 7)),
            true
        };
        yield return new object[]
        {
            new LocalDateRange(
                new LocalDate(2020, 1, 5),
                new LocalDate(2020, 1, 7)),
            new LocalDateRange(
                new LocalDate(2020, 1, 1),
                new LocalDate(2020, 1, 10)),
            true
        };
        yield return new object[]
        {
            new LocalDateRange(
                new LocalDate(2020, 1, 10),
                new LocalDate(2020, 1, 20)),
            new LocalDateRange(
                new LocalDate(2020, 1, 5),
                new LocalDate(2020, 1, 10)),
            true
        };
        yield return new object[]
        {
            new LocalDateRange(
                new LocalDate(2020, 1, 10),
                new LocalDate(2020, 1, 20)),
            new LocalDateRange(
                new LocalDate(2020, 1, 5),
                new LocalDate(2020, 1, 11)),
            true
        };
        yield return new object[]
        {
            new LocalDateRange(
                new LocalDate(2020, 1, 10),
                new LocalDate(2020, 1, 20)),
            new LocalDateRange(
                new LocalDate(2020, 1, 18),
                new LocalDate(2020, 1, 25)),
            true
        };
        yield return new object[]
        {
            new LocalDateRange(
                new LocalDate(2020, 1, 10),
                new LocalDate(2020, 1, 20)),
            new LocalDateRange(
                new LocalDate(2020, 1, 20),
                new LocalDate(2020, 1, 25)),
            true
        };
        yield return new object[]
        {
            new LocalDateRange(
                new LocalDate(2020, 1, 10),
                new LocalDate(2020, 1, 20)),
            new LocalDateRange(
                new LocalDate(2020, 1, 1),
                new LocalDate(2020, 1, 9)),
            false
        };
        yield return new object[]
        {
            new LocalDateRange(
                new LocalDate(2020, 1, 1),
                new LocalDate(2020, 1, 9)),
            new LocalDateRange(
                new LocalDate(2020, 1, 10),
                new LocalDate(2020, 1, 20)),
            false
        };
    }

    [Fact]
    public void GetEqualityComponents_SameValue_EqualityTrue()
    {
        // ARRANGE
        LocalDate start = new(2020, 1, 1);
        LocalDate end = new(2020, 1, 1);

        // ACT
        var result1 = new LocalDateRange(start, end);
        var result2 = new LocalDateRange(start, end);

        // ASSERT
        result1.Should().Be(result2);
        result1.GetHashCode().Should().Be(result2.GetHashCode());
        result1.Start.Should().Be(result2.Start);
        result1.End.Should().Be(result2.End);
    }


    [Fact]
    public void GetEqualityComponents_DifferentValue_EqualityFalse()
    {
        // ARRANGE
        LocalDate start = new(2020, 1, 1);
        LocalDate end = new(2020, 1, 1);
        LocalDate start2 = new(2020, 1, 1);
        LocalDate end2 = new(2020, 1, 2);

        // ACT
        var result1 = new LocalDateRange(start, end);
        var result2 = new LocalDateRange(start2, end2);

        // ASSERT
        result1.Should().NotBe(result2);
        result1.GetHashCode().Should().NotBe(result2.GetHashCode());
    }
}
