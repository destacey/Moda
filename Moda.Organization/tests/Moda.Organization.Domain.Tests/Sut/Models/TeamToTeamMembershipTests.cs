using FluentAssertions;
using Moda.Organization.Domain.Models;
using NodaTime;

namespace Moda.Organization.Domain.Tests.Sut.Models;
public class TeamToTeamMembershipTests
{
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
        var sut = TeamToTeamMembership.Create(childId, parentId, dateRange);

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
        Action action = () => TeamToTeamMembership.Create(childId, parentId, dateRange);

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("A team or team of teams cannot have a membership with its self.");
    }
}
