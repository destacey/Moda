using Moda.Common.Domain.Enums.Organization;
using Moda.Common.Domain.Events;
using Moda.Common.Domain.Models.Organizations;
using Moda.Organization.Domain.Models;
using Moda.Organization.Domain.Tests.Data;
using Moda.Tests.Shared;
using Moda.Tests.Shared.Extensions;
using NodaTime;
using NodaTime.Extensions;
using NodaTime.Testing;

namespace Moda.Organization.Domain.Tests.Sut.Models;
public class TeamTests
{
    private readonly TestingDateTimeProvider _dateTimeProvider;
    private readonly TeamFaker _teamFaker;
    private readonly TeamOfTeamsFaker _teamOfTeamsFaker;

    public TeamTests()
    {
        _dateTimeProvider = new(new FakeClock(DateTime.UtcNow.ToInstant()));
        _teamFaker = new();
        _teamOfTeamsFaker = new();
    }

    #region Create

    [Fact]
    public void Create_WhenValid_Success()
    {
        // Arrange
        var fakeTeam = _teamFaker.Generate();

        // Act
        var sut = Team.Create(fakeTeam.Name, fakeTeam.Code, fakeTeam.Description, fakeTeam.ActiveDate, _dateTimeProvider.Now);

        // Assert
        sut.Type.Should().Be(TeamType.Team);
        sut.Code.Should().Be(fakeTeam.Code);
        sut.Name.Should().Be(fakeTeam.Name);
        sut.Description.Should().Be(fakeTeam.Description);
        sut.IsActive.Should().BeTrue();
        sut.ParentMemberships.Should().BeEmpty();

        sut.DomainEvents.Should().NotBeEmpty();
        sut.DomainEvents.Should().ContainSingle(e => e is EntityCreatedEvent<Team>);
    }

    [Fact]
    public void Create_WithNullName_Throws()
    {
        // Arrange
        var fakeTeam = _teamFaker.Generate();
        string? name = null;

        // Act
        Action action = () => Team.Create(name!, fakeTeam.Code, fakeTeam.Description, fakeTeam.ActiveDate, _dateTimeProvider.Now);

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("Value cannot be null. (Parameter 'Name')");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_WithInvalidName_Throws(string name)
    {
        // Arrange
        var fakeTeam = _teamFaker.Generate();

        // Act
        Action action = () => Team.Create(name!, fakeTeam.Code, fakeTeam.Description, fakeTeam.ActiveDate, _dateTimeProvider.Now);

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("Required input Name was empty. (Parameter 'Name')");
    }

    [Fact]
    public void Create_WithNullCode_Throws()
    {
        // Arrange
        var fakeTeam = _teamFaker.Generate();
        TeamCode code = null!;

        // Act
        Action action = () => Team.Create(fakeTeam.Name, code, fakeTeam.Description, fakeTeam.ActiveDate, _dateTimeProvider.Now);

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("Value cannot be null. (Parameter 'Code')");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithInvalidDescription_IsNull(string? description)
    {
        // Arrange
        var fakeTeam = _teamFaker.Generate();

        // Act
        var sut = Team.Create(fakeTeam.Name, fakeTeam.Code, description, fakeTeam.ActiveDate, _dateTimeProvider.Now);

        // Assert
        sut.Description.Should().BeNull();
    }

    #endregion Create


    #region Update

    [Fact]
    public void Update_WhenValid_Success()
    {
        // Arrange
        var team = _teamFaker.Generate();

        var name = "New Team Name ";
        var code = new TeamCode("NEWTEST");
        var description = "New Description ";

        // Act
        team.Update(name, code, description, _dateTimeProvider.Now);

        // Assert
        team.Type.Should().Be(TeamType.Team);
        team.Code.Should().Be(code);
        team.Name.Should().Be(name.Trim());
        team.Description.Should().Be(description.Trim());
        team.IsActive.Should().BeTrue();

        team.DomainEvents.Should().NotBeEmpty();
        team.DomainEvents.Should().ContainSingle(e => e is EntityUpdatedEvent<Team>);
    }

    [Fact]
    public void Update_WithNullName_ReturnsFailedResult()
    {
        // Arrange
        var team = _teamFaker.Generate();

        string name = null!;

        // Act
        var result = team.Update(name, team.Code, team.Description, _dateTimeProvider.Now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().StartWith("System.ArgumentNullException: Value cannot be null. (Parameter 'Name')");

        team.DomainEvents.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Update_WithInvalidName_ReturnsFailedResult(string name)
    {
        // Arrange
        var team = _teamFaker.Generate();

        // Act
        var result = team.Update(name, team.Code, team.Description, _dateTimeProvider.Now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().StartWith("System.ArgumentException: Required input Name was empty. (Parameter 'Name')");

        team.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Update_WithNullCode_ReturnsFailedResult()
    {
        // Arrange
        var team = _teamFaker.Generate();
        TeamCode code = null!;

        // Act
        var result = team.Update(team.Name, code, team.Description, _dateTimeProvider.Now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().StartWith("System.ArgumentNullException: Value cannot be null. (Parameter 'Code')");

        team.DomainEvents.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Update_WithInvalidDescription_IsNull(string? description)
    {
        // Arrange
        var team = _teamFaker.Generate();

        // Act
        var result = team.Update(team.Name, team.Code, description, _dateTimeProvider.Now);

        // Assert
        result.IsSuccess.Should().BeTrue();
        team.Description.Should().BeNull();

        team.DomainEvents.Should().NotBeEmpty();
        team.DomainEvents.Should().ContainSingle(e => e is EntityUpdatedEvent<Team>);
    }

    #endregion Update


    #region IActivatable

    [Fact]
    public void Activate_WhenActive_SetsIsActiveToFalse()
    {
        // Arrange
        var inactiveDate = _dateTimeProvider.Now.Minus(Duration.FromDays(10)).InUtc().LocalDateTime.Date;
        var team = _teamFaker.WithData(isActive: false, inactiveDate: inactiveDate).Generate();

        // Act
        team.Activate(_dateTimeProvider.Now);

        // Assert
        team.IsActive.Should().BeTrue();

        team.DomainEvents.Should().NotBeEmpty();
        team.DomainEvents.Should().ContainSingle(e => e is EntityActivatedEvent<Team>);
    }

    [Fact]
    public void Deactivate_WhenActiveAndNoActiveMemberships_Success()
    {
        // Arrange
        var team = _teamFaker.Generate();
        var inactiveDate = _dateTimeProvider.Now.InUtc().LocalDateTime.Date;
        var args = TeamDeactivatableArgs.Create(inactiveDate, _dateTimeProvider.Now);

        // Act
        var result = team.Deactivate(args);

        // Assert
        result.IsSuccess.Should().BeTrue();
        team.IsActive.Should().BeFalse();
        team.InactiveDate.Should().Be(inactiveDate);
        team.DomainEvents.Should().ContainSingle(e => e is EntityDeactivatedEvent<Team>);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_Failure()
    {
        // Arrange
        var inactiveDate = _dateTimeProvider.Now.Minus(Duration.FromDays(10)).InUtc().LocalDateTime.Date;
        var team = _teamFaker.WithData(isActive: false, inactiveDate: inactiveDate).Generate();
        var args = TeamDeactivatableArgs.Create(inactiveDate, _dateTimeProvider.Now);

        // Act
        var result = team.Deactivate(args);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("The team is already inactive.");
        team.IsActive.Should().BeFalse();
        team.InactiveDate.Should().NotBeNull();
        team.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Deactivate_WhenActiveAndAsOfDateBeforeActiveDate_Failure()
    {
        // Arrange
        var team = _teamFaker.Generate();
        var inactiveDate = team.ActiveDate.PlusDays(-5);
        var args = TeamDeactivatableArgs.Create(inactiveDate, _dateTimeProvider.Now);

        // Act
        var result = team.Deactivate(args);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("The inactive date cannot be on or before the active date.");
        team.IsActive.Should().BeTrue();
        team.InactiveDate.Should().BeNull();
    }

    [Fact]
    public void Deactivate_WhenActiveAndHasActiveMemberships_Failure()
    {
        // Arrange
        var team = _teamFaker.Generate();
        var teamOfTeams = _teamOfTeamsFaker.Generate();
        LocalDate start = team.ActiveDate.PlusDays(5);
        MembershipDateRange dateRange = new(start, null);
        team.AddTeamMembership(teamOfTeams, dateRange, _dateTimeProvider.Now);

        var inactiveDate = start.PlusDays(10);
        var args = TeamDeactivatableArgs.Create(inactiveDate, _dateTimeProvider.Now);

        // Act
        var result = team.Deactivate(args);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("The inactive date must be on or after the end date of the last team membership.");
        team.IsActive.Should().BeTrue();
        team.InactiveDate.Should().BeNull();
    }

    [Fact]
    public void Deactivate_WhenActiveAndAsOfDateBeforeLatestMembershipEnd_Failure()
    {
        // Arrange
        var team = _teamFaker.Generate();
        var teamOfTeams = _teamOfTeamsFaker.Generate();
        LocalDate start = team.ActiveDate.PlusDays(5);
        LocalDate? end = start.PlusDays(90);
        MembershipDateRange dateRange = new(start, end);
        team.AddTeamMembership(teamOfTeams, dateRange, _dateTimeProvider.Now);

        var inactiveDate = start.PlusDays(10);
        var args = TeamDeactivatableArgs.Create(inactiveDate, _dateTimeProvider.Now);

        // Act
        var result = team.Deactivate(args);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("The inactive date must be on or after the end date of the last team membership.");
        team.IsActive.Should().BeTrue();
        team.InactiveDate.Should().BeNull();
    }

    [Fact]
    public void Deactivate_WhenActiveAndAsOfDateOnLatestMembershipEnd_Success()
    {
        // Arrange
        var team = _teamFaker.Generate();
        var teamOfTeams = _teamOfTeamsFaker.Generate();
        LocalDate start = team.ActiveDate.PlusDays(5);
        LocalDate? end = start.PlusDays(90);
        MembershipDateRange dateRange = new(start, end);
        team.AddTeamMembership(teamOfTeams, dateRange, _dateTimeProvider.Now);

        var args = TeamDeactivatableArgs.Create(end.Value, _dateTimeProvider.Now);

        // Act
        var result = team.Deactivate(args);

        // Assert
        result.IsSuccess.Should().BeTrue();
        team.IsActive.Should().BeFalse();
        team.InactiveDate.Should().Be(end);
    }

    [Fact]
    public void Deactivate_WhenActiveAndAsOfDateAfterLatestMembershipEnd_Success()
    {
        // Arrange
        var team = _teamFaker.Generate();
        var teamOfTeams = _teamOfTeamsFaker.Generate();
        LocalDate start = team.ActiveDate.PlusDays(5);
        LocalDate? end = start.PlusDays(90);
        MembershipDateRange dateRange = new(start, end);
        team.AddTeamMembership(teamOfTeams, dateRange, _dateTimeProvider.Now);

        var inactiveDate = start.PlusDays(100);
        var args = TeamDeactivatableArgs.Create(inactiveDate, _dateTimeProvider.Now);

        // Act
        var result = team.Deactivate(args);

        // Assert
        result.IsSuccess.Should().BeTrue();
        team.IsActive.Should().BeFalse();
        team.InactiveDate.Should().Be(inactiveDate);
    }

    #endregion IActivatable


    #region Memberships

    [Fact]
    public void AddTeamToTeamMembership_WhenNoExistingMemberships_Success()
    {
        // Arrange
        var team = _teamFaker.Generate();
        var teamOfTeams = _teamOfTeamsFaker.Generate();
        LocalDate start = team.ActiveDate.PlusDays(10);
        LocalDate? end = start.PlusDays(100);
        MembershipDateRange dateRange = new(start, end);

        // Act
        var result = team.AddTeamMembership(teamOfTeams, dateRange, _dateTimeProvider.Now);

        // Assert
        result.IsSuccess.Should().BeTrue();
        team.ParentMemberships.Count.Should().Be(1);
        team.ParentMemberships.First().SourceId.Should().Be(team.Id);
        team.ParentMemberships.First().TargetId.Should().Be(teamOfTeams.Id);
        team.ParentMemberships.First().DateRange.Start.Should().Be(start);
        team.ParentMemberships.First().DateRange.End.Should().Be(end);
    }

    [Fact]
    public void AddTeamToTeamMembership_WhenNoExistingMembershipsAndWithNullEnd_Success()
    {
        // Arrange
        var team = _teamFaker.Generate();
        var teamOfTeams = _teamOfTeamsFaker.Generate();
        LocalDate start = team.ActiveDate.PlusDays(10);
        LocalDate? end = null;
        MembershipDateRange dateRange = new(start, end);

        // Act
        var result = team.AddTeamMembership(teamOfTeams, dateRange, _dateTimeProvider.Now);

        // Assert
        result.IsSuccess.Should().BeTrue();
        team.ParentMemberships.Count.Should().Be(1);
        team.ParentMemberships.First().SourceId.Should().Be(team.Id);
        team.ParentMemberships.First().TargetId.Should().Be(teamOfTeams.Id);
        team.ParentMemberships.First().DateRange.Start.Should().Be(start);
        team.ParentMemberships.First().DateRange.End.Should().BeNull();
    }

    [Fact]
    public void AddTeamToTeamMembership_WhenSourceInactive_Failure()
    {
        // Arrange
        var activeDate = new LocalDate(2022, 1, 1);
        var inactiveDate = new LocalDate(2022, 12, 1);
        var team = _teamFaker.WithData(activeDate: activeDate, isActive: false, inactiveDate: inactiveDate).Generate();
        var teamOfTeams = _teamOfTeamsFaker.Generate();
        LocalDate start = new(2023, 1, 1);
        LocalDate? end = null;
        MembershipDateRange dateRange = new(start, end);

        var expectedErrorMessage = $"Memberships can not be added to inactive teams. {team.Name} is inactive.";

        // Act
        var result = team.AddTeamMembership(teamOfTeams, dateRange, _dateTimeProvider.Now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(expectedErrorMessage);
        team.ParentMemberships.Count.Should().Be(0);
    }

    [Fact]
    public void AddTeamToTeamMembership_WhenTargetInactive_Failure()
    {
        // Arrange
        var team = _teamFaker.Generate();
        var inactiveDate = _dateTimeProvider.Now.Minus(Duration.FromDays(10)).InUtc().LocalDateTime.Date;
        var teamOfTeams = _teamOfTeamsFaker.WithData(isActive: false, inactiveDate: inactiveDate).Generate();
        LocalDate start = team.ActiveDate.PlusDays(10);
        LocalDate? end = null;
        MembershipDateRange dateRange = new(start, end);

        var expectedErrorMessage = $"Memberships can not be added to inactive teams. {teamOfTeams.Name} is inactive.";

        // Act
        var result = team.AddTeamMembership(teamOfTeams, dateRange, _dateTimeProvider.Now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(expectedErrorMessage);
        team.ParentMemberships.Count.Should().Be(0);
    }

    [Fact]
    public void UpdateTeamToTeamMembership_WithValidDates_Success()
    {
        // Arrange
        var team = _teamFaker.Generate();
        var teamOfTeams = _teamOfTeamsFaker.Generate();
        LocalDate start = team.ActiveDate.PlusDays(10);
        LocalDate end = start.PlusDays(100);
        MembershipDateRange dateRange = new(start, end);
        var createresult = team.AddTeamMembership(teamOfTeams, dateRange, _dateTimeProvider.Now);

        var membership = team.ParentMemberships.First();
        membership.SetPrivate(m => m.Id, Guid.NewGuid());
        membership.SetPrivate(m => m.Source, team);
        membership.SetPrivate(m => m.Target, teamOfTeams);

        LocalDate updatedStart = start.PlusMonths(1);
        LocalDate? updatedEnd = end.PlusMonths(1);
        MembershipDateRange updatedDateRange = new(updatedStart, updatedEnd);

        // Act
        var result = team.UpdateTeamMembership(membership.Id, updatedDateRange, _dateTimeProvider.Now);

        // Assert
        result.IsSuccess.Should().BeTrue();
        team.ParentMemberships.Count.Should().Be(1);
        team.ParentMemberships.First().SourceId.Should().Be(team.Id);
        team.ParentMemberships.First().TargetId.Should().Be(teamOfTeams.Id);
        team.ParentMemberships.First().DateRange.Start.Should().Be(updatedStart);
        team.ParentMemberships.First().DateRange.End.Should().Be(updatedEnd);
    }

    [Fact]
    public void UpdateTeamToTeamMembership_WhenSourceIsInactive_Failure()
    {
        // Arrange
        var team = _teamFaker.Generate();
        var teamOfTeams = _teamOfTeamsFaker.Generate();
        LocalDate start = team.ActiveDate.PlusDays(10);
        LocalDate end = start.PlusDays(100);
        MembershipDateRange dateRange = new(start, end);
        var createresult = team.AddTeamMembership(teamOfTeams, dateRange, _dateTimeProvider.Now);

        var membership = team.ParentMemberships.First();
        membership.SetPrivate(m => m.Id, Guid.NewGuid());
        membership.SetPrivate(m => m.Source, team);
        membership.SetPrivate(m => m.Target, teamOfTeams);

        LocalDate updatedStart = start.PlusMonths(1);
        LocalDate? updatedEnd = end.PlusMonths(1);
        MembershipDateRange updatedDateRange = new(updatedStart, updatedEnd);

        team.SetPrivate(m => m.IsActive, false);

        // Act
        var result = team.UpdateTeamMembership(membership.Id, updatedDateRange, _dateTimeProvider.Now);

        // Assert
        result.IsFailure.Should().BeTrue();
        team.ParentMemberships.Count.Should().Be(1);
        team.ParentMemberships.First().DateRange.Start.Should().Be(start);
        team.ParentMemberships.First().DateRange.End.Should().Be(end);
    }

    [Fact]
    public void UpdateTeamToTeamMembership_WhenTargetIsInactive_Failure()
    {
        // Arrange
        var team = _teamFaker.Generate();
        var teamOfTeams = _teamOfTeamsFaker.Generate();
        LocalDate start = team.ActiveDate.PlusDays(10);
        LocalDate end = start.PlusDays(100);
        MembershipDateRange dateRange = new(start, end);
        var createresult = team.AddTeamMembership(teamOfTeams, dateRange, _dateTimeProvider.Now);

        var membership = team.ParentMemberships.First();
        membership.SetPrivate(m => m.Id, Guid.NewGuid());
        membership.SetPrivate(m => m.Source, team);
        membership.SetPrivate(m => m.Target, teamOfTeams);

        LocalDate updatedStart = start.PlusMonths(1);
        LocalDate? updatedEnd = end.PlusMonths(1);
        MembershipDateRange updatedDateRange = new(updatedStart, updatedEnd);

        teamOfTeams.SetPrivate(m => m.IsActive, false);

        // Act
        var result = team.UpdateTeamMembership(membership.Id, updatedDateRange, _dateTimeProvider.Now);

        // Assert
        result.IsFailure.Should().BeTrue();
        team.ParentMemberships.Count.Should().Be(1);
        team.ParentMemberships.First().DateRange.Start.Should().Be(start);
        team.ParentMemberships.First().DateRange.End.Should().Be(end);
    }

    [Fact]
    public void RemoveTeamToTeamMembership_WhenValid_Success()
    {
        // Arrange
        var team = _teamFaker.Generate();
        var teamOfTeams = _teamOfTeamsFaker.Generate();
        LocalDate start = team.ActiveDate.PlusDays(10);
        LocalDate end = start.PlusDays(100);
        MembershipDateRange dateRange = new(start, end);
        var createresult = team.AddTeamMembership(teamOfTeams, dateRange, _dateTimeProvider.Now);

        var membership = team.ParentMemberships.First();
        membership.SetPrivate(m => m.Id, Guid.NewGuid());
        membership.SetPrivate(m => m.Source, team);
        membership.SetPrivate(m => m.Target, teamOfTeams);

        // Act
        var result = team.RemoveTeamMembership(membership.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        team.ParentMemberships.Count.Should().Be(0);
    }

    [Fact]
    public void RemoveTeamToTeamMembership_WhenSourceIsInactive_Failure()
    {
        // Arrange
        var team = _teamFaker.Generate();
        var teamOfTeams = _teamOfTeamsFaker.Generate();
        LocalDate start = team.ActiveDate.PlusDays(10);
        LocalDate end = start.PlusDays(100);
        MembershipDateRange dateRange = new(start, end);
        var createresult = team.AddTeamMembership(teamOfTeams, dateRange, _dateTimeProvider.Now);

        var membership = team.ParentMemberships.First();
        membership.SetPrivate(m => m.Id, Guid.NewGuid());
        membership.SetPrivate(m => m.Source, team);
        membership.SetPrivate(m => m.Target, teamOfTeams);

        team.SetPrivate(m => m.IsActive, false);

        // Act
        var result = team.RemoveTeamMembership(membership.Id);

        // Assert
        result.IsFailure.Should().BeTrue();
        team.ParentMemberships.Count.Should().Be(1);
    }

    [Fact]
    public void RemoveTeamToTeamMembership_WhenTargetIsInactive_Failure()
    {
        // Arrange
        var team = _teamFaker.Generate();
        var teamOfTeams = _teamOfTeamsFaker.Generate();
        LocalDate start = team.ActiveDate.PlusDays(10);
        LocalDate end = start.PlusDays(100);
        MembershipDateRange dateRange = new(start, end);
        var createresult = team.AddTeamMembership(teamOfTeams, dateRange, _dateTimeProvider.Now);

        var membership = team.ParentMemberships.First();
        membership.SetPrivate(m => m.Id, Guid.NewGuid());
        membership.SetPrivate(m => m.Source, team);
        membership.SetPrivate(m => m.Target, teamOfTeams);

        teamOfTeams.SetPrivate(m => m.IsActive, false);

        // Act
        var result = team.RemoveTeamMembership(membership.Id);

        // Assert
        result.IsFailure.Should().BeTrue();
        team.ParentMemberships.Count.Should().Be(1);
    }


    #endregion Memberships
}
