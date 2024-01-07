using System.Security.Cryptography;
using Moda.Common.Domain.Enums.Organization;
using Moda.Common.Domain.Events;
using Moda.Organization.Domain.Models;
using Moda.Tests.Shared.Extensions;
using NodaTime;

namespace Moda.Organization.Domain.Tests.Sut.Models;
public class TeamTests
{
    private readonly Instant _now = Instant.FromUtc(2020, 1, 1, 8, 0);
    private readonly TeamCode _genericTeamCode = new("Test");

    private Team GenerateTeam(bool isActive)
    {
        int key = RandomNumberGenerator.GetInt32(1, 100000);

        var team = Team.Create("Test Team", _genericTeamCode, "This is a description.", _now);
        team.SetPrivate(t => t.Id, Guid.NewGuid());
        team.SetPrivate(t => t.Key, key);

        if (!isActive)
        {
            team.Deactivate(_now);
        }

        team.ClearDomainEvents();

        return team;
    }

    private TeamOfTeams GenerateTeamOfTeams(bool isActive)
    {
        int key = RandomNumberGenerator.GetInt32(1, 100000);

        var team = TeamOfTeams.Create("Test Team of Teams", new TeamCode("TOT"), null, _now);
        team.SetPrivate(t => t.Id, Guid.NewGuid());
        team.SetPrivate(t => t.Key, key);

        if (!isActive)
        {
            team.Deactivate(_now);
        }

        team.ClearDomainEvents();

        return team;
    }

    #region Create

    [Fact]
    public void Create_WhenValid_Success()
    {
        // Arrange
        var name = "Test Team ";
        var description = "Test Team Description";

        // Act
        var sut = Team.Create(name, _genericTeamCode, description, _now);

        // Assert
        sut.Type.Should().Be(TeamType.Team);
        sut.Code.Should().Be(_genericTeamCode);
        sut.Name.Should().Be(name.Trim());
        sut.Description.Should().Be(description);
        sut.IsActive.Should().BeTrue();
        sut.ParentMemberships.Should().BeEmpty();

        sut.DomainEvents.Should().NotBeEmpty();
        sut.DomainEvents.Should().ContainSingle(e => e is EntityCreatedEvent<Team>);
    }

    [Fact]
    public void Create_WithNullName_Throws()
    {
        // Arrange
        string? name = null;

        // Act
        Action action = () => Team.Create(name!, _genericTeamCode, null, _now);

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("Value cannot be null. (Parameter 'Name')");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_WithInvalidName_Throws(string name)
    {
        // Act
        Action action = () => Team.Create(name, _genericTeamCode, null, _now);

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("Required input Name was empty. (Parameter 'Name')");
    }

    [Fact]
    public void Create_WithNullCode_Throws()
    {
        // Arrange
        TeamCode code = null!;

        // Act
        Action action = () => Team.Create("Test", code, null, _now);

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("Value cannot be null. (Parameter 'Code')");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithInvalidDescription_IsNull(string? description)
    {
        // Act
        var sut = Team.Create("Team", _genericTeamCode, description, _now);

        // Assert
        sut.Description.Should().BeNull();
    }

    #endregion Create


    #region Update

    [Fact]
    public void Update_WhenValid_Success()
    {
        // Arrange
        Team team = GenerateTeam(true);

        var name = "New Team Name ";
        var code = new TeamCode("NEWTEST");
        var description = "New Description ";

        // Act
        team.Update(name, code, description, _now);

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
        Team team = GenerateTeam(true);
        string name = null!;

        // Act
        var result = team.Update(name, team.Code, team.Description, _now);

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
        Team team = GenerateTeam(true);

        // Act
        var result = team.Update(name, team.Code, team.Description, _now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().StartWith("System.ArgumentException: Required input Name was empty. (Parameter 'Name')");

        team.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Update_WithNullCode_ReturnsFailedResult()
    {
        // Arrange
        Team team = GenerateTeam(true);
        TeamCode code = null!;

        // Act
        var result = team.Update(team.Name, code, team.Description, _now);

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
        Team team = GenerateTeam(true);

        // Act
        var result = team.Update(team.Name, team.Code, description, _now);

        // Assert
        result.IsSuccess.Should().BeTrue();
        team.Description.Should().BeNull();

        team.DomainEvents.Should().NotBeEmpty();
        team.DomainEvents.Should().ContainSingle(e => e is EntityUpdatedEvent<Team>);
    }

    #endregion Update


    #region IActivatable

    [Fact]
    public void Deactivate_WhenActive_SetsIsActiveToFalse()
    {
        // Arrange
        var team = GenerateTeam(true);

        // Act
        team.Deactivate(_now);

        // Assert
        team.IsActive.Should().BeFalse();

        team.DomainEvents.Should().NotBeEmpty();
        team.DomainEvents.Should().ContainSingle(e => e is EntityDeactivatedEvent<Team>);
    }

    [Fact]
    public void Activate_WhenActive_SetsIsActiveToFalse()
    {
        // Arrange
        var team = GenerateTeam(false);

        // Act
        team.Activate(_now);

        // Assert
        team.IsActive.Should().BeTrue();

        team.DomainEvents.Should().NotBeEmpty();
        team.DomainEvents.Should().ContainSingle(e => e is EntityActivatedEvent<Team>);
    }

    #endregion IActivatable

    #region Memberships

    [Fact]
    public void AddTeamToTeamMembership_WhenNoExistingMemberships_Success()
    {
        // Arrange
        var team = GenerateTeam(true);
        var teamOfTeams = GenerateTeamOfTeams(true);
        LocalDate start = new(2023, 1, 1);
        LocalDate? end = new(2023, 5, 1); ;
        MembershipDateRange dateRange = new(start, end);

        // Act
        var result = team.AddTeamMembership(teamOfTeams, dateRange, _now);

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
        var team = GenerateTeam(true);
        var teamOfTeams = GenerateTeamOfTeams(true);
        LocalDate start = new(2023, 1, 1);
        LocalDate? end = null;
        MembershipDateRange dateRange = new(start, end);

        // Act
        var result = team.AddTeamMembership(teamOfTeams, dateRange, _now);

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
        var team = GenerateTeam(false);
        var teamOfTeams = GenerateTeamOfTeams(true);
        LocalDate start = new(2023, 1, 1);
        LocalDate? end = null;
        MembershipDateRange dateRange = new(start, end);

        var expectedErrorMessage = $"Memberships can not be added to inactive teams. {team.Name} is inactive.";

        // Act
        var result = team.AddTeamMembership(teamOfTeams, dateRange, _now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(expectedErrorMessage);
        team.ParentMemberships.Count.Should().Be(0);
    }

    [Fact]
    public void AddTeamToTeamMembership_WhenTargetInactive_Failure()
    {
        // Arrange
        var team = GenerateTeam(true);
        var teamOfTeams = GenerateTeamOfTeams(false);
        LocalDate start = new(2023, 1, 1);
        LocalDate? end = null;
        MembershipDateRange dateRange = new(start, end);

        var expectedErrorMessage = $"Memberships can not be added to inactive teams. {teamOfTeams.Name} is inactive.";

        // Act
        var result = team.AddTeamMembership(teamOfTeams, dateRange, _now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(expectedErrorMessage);
        team.ParentMemberships.Count.Should().Be(0);
    }

    [Fact]
    public void UpdateTeamToTeamMembership_WithValidDates_Success()
    {
        // Arrange
        var team = GenerateTeam(true);
        var teamOfTeams = GenerateTeamOfTeams(true);
        LocalDate start = new(2023, 1, 1);
        LocalDate? end = new(2023, 5, 1);
        MembershipDateRange dateRange = new(start, end);
        var createresult = team.AddTeamMembership(teamOfTeams, dateRange, _now);

        var membership = team.ParentMemberships.First();
        membership.SetPrivate(m => m.Id, Guid.NewGuid());
        membership.SetPrivate(m => m.Source, team);
        membership.SetPrivate(m => m.Target, teamOfTeams);

        LocalDate updatedStart = new(2023, 2, 1);
        LocalDate? updatedEnd = new(2023, 5, 10);
        MembershipDateRange updatedDateRange = new(updatedStart, updatedEnd);

        // Act
        var result = team.UpdateTeamMembership(membership.Id, updatedDateRange, _now);

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
        var team = GenerateTeam(true);
        var teamOfTeams = GenerateTeamOfTeams(true);
        LocalDate start = new(2023, 1, 1);
        LocalDate? end = new(2023, 5, 1);
        MembershipDateRange dateRange = new(start, end);
        var createresult = team.AddTeamMembership(teamOfTeams, dateRange, _now);

        var membership = team.ParentMemberships.First();
        membership.SetPrivate(m => m.Id, Guid.NewGuid());
        membership.SetPrivate(m => m.Source, team);
        membership.SetPrivate(m => m.Target, teamOfTeams);

        LocalDate updatedStart = new(2023, 2, 1);
        LocalDate? updatedEnd = new(2023, 5, 10);
        MembershipDateRange updatedDateRange = new(updatedStart, updatedEnd);

        team.SetPrivate(m => m.IsActive, false);

        // Act
        var result = team.UpdateTeamMembership(membership.Id, updatedDateRange, _now);

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
        var team = GenerateTeam(true);
        var teamOfTeams = GenerateTeamOfTeams(true);
        LocalDate start = new(2023, 1, 1);
        LocalDate? end = new(2023, 5, 1);
        MembershipDateRange dateRange = new(start, end);
        var createresult = team.AddTeamMembership(teamOfTeams, dateRange, _now);

        var membership = team.ParentMemberships.First();
        membership.SetPrivate(m => m.Id, Guid.NewGuid());
        membership.SetPrivate(m => m.Source, team);
        membership.SetPrivate(m => m.Target, teamOfTeams);

        LocalDate updatedStart = new(2023, 2, 1);
        LocalDate? updatedEnd = new(2023, 5, 10);
        MembershipDateRange updatedDateRange = new(updatedStart, updatedEnd);

        teamOfTeams.SetPrivate(m => m.IsActive, false);

        // Act
        var result = team.UpdateTeamMembership(membership.Id, updatedDateRange, _now);

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
        var team = GenerateTeam(true);
        var teamOfTeams = GenerateTeamOfTeams(true);
        LocalDate start = new(2023, 1, 1);
        LocalDate? end = new(2023, 5, 1);
        MembershipDateRange dateRange = new(start, end);
        var createresult = team.AddTeamMembership(teamOfTeams, dateRange, _now);

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
        var team = GenerateTeam(true);
        var teamOfTeams = GenerateTeamOfTeams(true);
        LocalDate start = new(2023, 1, 1);
        LocalDate? end = new(2023, 5, 1);
        MembershipDateRange dateRange = new(start, end);
        var createresult = team.AddTeamMembership(teamOfTeams, dateRange, _now);

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
        var team = GenerateTeam(true);
        var teamOfTeams = GenerateTeamOfTeams(true);
        LocalDate start = new(2023, 1, 1);
        LocalDate? end = new(2023, 5, 1);
        MembershipDateRange dateRange = new(start, end);
        var createresult = team.AddTeamMembership(teamOfTeams, dateRange, _now);

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
