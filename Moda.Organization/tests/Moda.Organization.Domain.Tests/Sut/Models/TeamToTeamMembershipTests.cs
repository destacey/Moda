using FluentAssertions;
using Moda.Organization.Domain.Models;
using NodaTime;

namespace Moda.Organization.Domain.Tests.Sut.Models;
public class TeamToTeamMembershipTests
{
    #region Create

    [Fact]
    public void Create_WhenValid_Success()
    {
        // Arrange
        var childId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var start = new LocalDate(2020, 1, 1);
        var end = new LocalDate(2020, 2, 1);
        var dateRange = new MembershipDateRange(start, end);

        // Act
        var sut = TeamMembership.Create(childId, parentId, dateRange);

        // Assert
        sut.SourceId.Should().Be(childId);
        sut.TargetId.Should().Be(parentId);
        sut.DateRange.Should().Be(dateRange);
    }

    [Fact]
    public void Create_WhenChildIdEqualsParentId_ThrowsArgumentException()
    {
        // Arrange
        var childId = Guid.NewGuid();
        var parentId = childId;
        var start = new LocalDate(2020, 1, 1);
        var end = new LocalDate(2020, 2, 1);
        var dateRange = new MembershipDateRange(start, end);

        // Act
        Action action = () => TeamMembership.Create(childId, parentId, dateRange);

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("A team or team of teams cannot have a membership with its self.");
    }

    #endregion Create

    #region IsPastOn

    [Theory]
    [MemberData(nameof(IsPastOnData))]
    public void IsPastOn(MembershipDateRange range, LocalDate now, bool expected)
    {
        // Arrange
        var sut = TeamMembership.Create(Guid.NewGuid(), Guid.NewGuid(), range);

        // ACT
        var result = sut.IsPastOn(now);

        // ASSERT
        result.Should().Be(expected);
    }
    public static IEnumerable<object[]> IsPastOnData()
    {
        yield return new object[]
        {
            new MembershipDateRange(
                new LocalDate(2020, 1, 1),
                new LocalDate(2020, 1, 10)),
            new LocalDate(2019, 12, 31),
            false
        };
        yield return new object[]
        {
            new MembershipDateRange(
                new LocalDate(2020, 1, 1),
                new LocalDate(2020, 1, 10)),
            new LocalDate(2020, 1, 1),
            false
        };
        yield return new object[]
        {
            new MembershipDateRange(
                new LocalDate(2020, 1, 1),
                new LocalDate(2020, 1, 10)),
            new LocalDate(2020, 1, 10),
            false
        };
        yield return new object[]
        {
            new MembershipDateRange(
                new LocalDate(2020, 1, 1),
                new LocalDate(2020, 1, 10)),
            new LocalDate(2020, 1, 11),
            true
        };
        // null end scenarios
        yield return new object[]
        {
            new MembershipDateRange(
                new LocalDate(2020, 1, 1),
                null),
            new LocalDate(2019, 12, 31),
            false
        };
        yield return new object[]
        {
            new MembershipDateRange(
                new LocalDate(2020, 1, 1),
                null),
            new LocalDate(2020, 1, 1),
            false
        };
        yield return new object[]
        {
            new MembershipDateRange(
                new LocalDate(2020, 1, 1),
                null),
            new LocalDate(2020, 1, 10),
            false
        };
    }

    #endregion IsActiveOn


    #region IsActiveOn

    [Theory]
    [MemberData(nameof(IsActiveOnData))]
    public void IsActiveOn(MembershipDateRange range, LocalDate now, bool expected)
    {
        // Arrange
        var sut = TeamMembership.Create(Guid.NewGuid(), Guid.NewGuid(), range);

        // ACT
        var result = sut.IsActiveOn(now);

        // ASSERT
        result.Should().Be(expected);
    }
    public static IEnumerable<object[]> IsActiveOnData()
    {
        yield return new object[]
        {
            new MembershipDateRange(
                new LocalDate(2020, 1, 1),
                new LocalDate(2020, 1, 10)),
            new LocalDate(2019, 12, 31),
            false
        };
        yield return new object[]
        {
            new MembershipDateRange(
                new LocalDate(2020, 1, 1),
                new LocalDate(2020, 1, 10)),
            new LocalDate(2020, 1, 1),
            true
        };
        yield return new object[]
        {
            new MembershipDateRange(
                new LocalDate(2020, 1, 1),
                new LocalDate(2020, 1, 10)),
            new LocalDate(2020, 1, 10),
            true
        };
        yield return new object[]
        {
            new MembershipDateRange(
                new LocalDate(2020, 1, 1),
                new LocalDate(2020, 1, 10)),
            new LocalDate(2020, 1, 11),
            false
        };
        // null end scenarios
        yield return new object[]
        {
            new MembershipDateRange(
                new LocalDate(2020, 1, 1),
                null),
            new LocalDate(2019, 12, 31),
            false
        };
        yield return new object[]
        {
            new MembershipDateRange(
                new LocalDate(2020, 1, 1),
                null),
            new LocalDate(2020, 1, 1),
            true
        };
        yield return new object[]
        {
            new MembershipDateRange(
                new LocalDate(2020, 1, 1),
                null),
            new LocalDate(2020, 1, 10),
            true
        };
    }

    #endregion IsActiveOn

    #region IsFutureOn

    [Theory]
    [MemberData(nameof(IsFutureOnData))]
    public void IsFutureOn(MembershipDateRange range, LocalDate now, bool expected)
    {
        // Arrange
        var sut = TeamMembership.Create(Guid.NewGuid(), Guid.NewGuid(), range);

        // ACT
        var result = sut.IsFutureOn(now);

        // ASSERT
        result.Should().Be(expected);
    }
    public static IEnumerable<object[]> IsFutureOnData()
    {
        yield return new object[]
        {
            new MembershipDateRange(
                new LocalDate(2020, 1, 1),
                new LocalDate(2020, 1, 10)),
            new LocalDate(2019, 12, 31),
            true
        };
        yield return new object[]
        {
            new MembershipDateRange(
                new LocalDate(2020, 1, 1),
                new LocalDate(2020, 1, 10)),
            new LocalDate(2020, 1, 1),
            false
        };
        yield return new object[]
        {
            new MembershipDateRange(
                new LocalDate(2020, 1, 1),
                new LocalDate(2020, 1, 10)),
            new LocalDate(2020, 1, 10),
            false
        };
        yield return new object[]
        {
            new MembershipDateRange(
                new LocalDate(2020, 1, 1),
                new LocalDate(2020, 1, 10)),
            new LocalDate(2020, 1, 11),
            false
        };
        // null end scenarios
        yield return new object[]
        {
            new MembershipDateRange(
                new LocalDate(2020, 1, 1),
                null),
            new LocalDate(2019, 12, 31),
            true
        };
        yield return new object[]
        {
            new MembershipDateRange(
                new LocalDate(2020, 1, 1),
                null),
            new LocalDate(2020, 1, 1),
            false
        };
        yield return new object[]
        {
            new MembershipDateRange(
                new LocalDate(2020, 1, 1),
                null),
            new LocalDate(2020, 1, 10),
            false
        };
    }

    #endregion IsActiveOn
}
