using System.Security.Cryptography;
using FluentAssertions;
using Moda.Organization.Domain.Enums;
using Moda.Organization.Domain.Models;
using Moda.Organization.Domain.Tests.Extensions;
using NodaTime;

namespace Moda.Organization.Domain.Tests.Sut.Models;
public class TeamOfTeamsTests
{
    private readonly Instant _now = Instant.FromUtc(2020, 1, 1, 8, 0);
    private readonly TeamCode _genericTeamCode = new("TOT1");

    private TeamOfTeams GenerateTeamOfTeams(bool isActive)
    {
        int localId = RandomNumberGenerator.GetInt32(1, 100000);

        var team = TeamOfTeams.Create("Test Team of Teams", _genericTeamCode, null);
        team.SetPrivate(t => t.Id, Guid.NewGuid());
        team.SetPrivate(t => t.LocalId, localId);

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
        var sut = TeamOfTeams.Create(name, _genericTeamCode, description);

        // Assert
        sut.Type.Should().Be(TeamType.TeamOfTeams);
        sut.Code.Should().Be(_genericTeamCode);
        sut.Name.Should().Be(name.Trim());
        sut.Description.Should().Be(description);
        sut.IsActive.Should().BeTrue();
        sut.ParentMemberships.Should().BeEmpty();
        sut.ChildMemberships.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithNullName_Throws()
    {
        // Arrange
        string? name = null;

        // Act
        Action action = () => TeamOfTeams.Create(name!, _genericTeamCode, null);

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("Value cannot be null. (Parameter 'Name')");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_WithInvalidName_Throws(string name)
    {
        // Act
        Action action = () => TeamOfTeams.Create(name, _genericTeamCode, null);

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("Required input Name was empty. (Parameter 'Name')");
    }

    [Fact]
    public void Create_WithNullCode_Throws()
    {
        // Arrange
        TeamCode code = null!;

        // Act
        Action action = () => TeamOfTeams.Create("Test", code, null);

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
        var sut = TeamOfTeams.Create("Team", _genericTeamCode, description);

        // Assert
        sut.Description.Should().BeNull();
    }

    #endregion Create


    #region Update

    [Fact]
    public void Update_WhenValid_Success()
    {
        // Arrange
        TeamOfTeams team = GenerateTeamOfTeams(true);

        var name = "New Team Name ";
        var code = new TeamCode("NEWTEST");
        var description = "New Description ";

        // Act
        team.Update(name, code, description);

        // Assert
        team.Type.Should().Be(TeamType.TeamOfTeams);
        team.Code.Should().Be(code);
        team.Name.Should().Be(name.Trim());
        team.Description.Should().Be(description.Trim());
        team.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Update_WithNullName_ReturnsFailedResult()
    {
        // Arrange
        TeamOfTeams team = GenerateTeamOfTeams(true);
        string name = null!;

        // Act
        var result = team.Update(name, team.Code, team.Description);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().StartWith("System.ArgumentNullException: Value cannot be null. (Parameter 'Name')");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Update_WithInvalidName_ReturnsFailedResult(string name)
    {
        // Arrange
        TeamOfTeams team = GenerateTeamOfTeams(true);

        // Act
        var result = team.Update(name, team.Code, team.Description);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().StartWith("System.ArgumentException: Required input Name was empty. (Parameter 'Name')");
    }

    [Fact]
    public void Update_WithNullCode_ReturnsFailedResult()
    {
        // Arrange
        TeamOfTeams team = GenerateTeamOfTeams(true);
        TeamCode code = null!;

        // Act
        var result = team.Update(team.Name, code, team.Description);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().StartWith("System.ArgumentNullException: Value cannot be null. (Parameter 'Code')");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Update_WithInvalidDescription_IsNull(string? description)
    {
        // Arrange
        TeamOfTeams team = GenerateTeamOfTeams(true);

        // Act
        var result = team.Update(team.Name, team.Code, description);

        // Assert
        result.IsSuccess.Should().BeTrue();
        team.Description.Should().BeNull();
    }

    #endregion Update


    #region IActivatable

    [Fact]
    public void Deactivate_WhenActive_SetsIsActiveToFalse()
    {
        // Arrange
        TeamOfTeams team = GenerateTeamOfTeams(true);

        // Act
        team.Deactivate(_now);

        // Assert
        team.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_WhenActive_SetsIsActiveToFalse()
    {
        // Arrange
        TeamOfTeams team = GenerateTeamOfTeams(false);

        // Act
        team.Activate(_now);

        // Assert
        team.IsActive.Should().BeTrue();
    }

    #endregion IActivatable

    #region Memberships

    [Fact]
    public void AddTeamToTeamMembership_WhenNoExistingMemberships_Success()
    {
        // Arrange
        var source = GenerateTeamOfTeams(true);
        var target = GenerateTeamOfTeams(true);
        LocalDate start = new(2023, 1, 1);
        LocalDate? end = new(2023, 5, 1); ;
        MembershipDateRange dateRange = new(start, end);

        // Act
        var result = source.AddTeamMembership(target, dateRange, _now);

        // Assert
        result.IsSuccess.Should().BeTrue();
        source.ParentMemberships.Count.Should().Be(1);
        source.ParentMemberships.First().SourceId.Should().Be(source.Id);
        source.ParentMemberships.First().TargetId.Should().Be(target.Id);
        source.ParentMemberships.First().DateRange.Start.Should().Be(start);
        source.ParentMemberships.First().DateRange.End.Should().Be(end);
    }

    [Fact]
    public void AddTeamToTeamMembership_WhenNoExistingMembershipsAndWithNullEnd_Success()
    {
        // Arrange
        var source = GenerateTeamOfTeams(true);
        var target = GenerateTeamOfTeams(true);
        LocalDate start = new(2023, 1, 1);
        LocalDate? end = null;
        MembershipDateRange dateRange = new(start, end);

        // Act
        var result = source.AddTeamMembership(target, dateRange, _now);

        // Assert
        result.IsSuccess.Should().BeTrue();
        source.ParentMemberships.Count.Should().Be(1);
        source.ParentMemberships.First().SourceId.Should().Be(source.Id);
        source.ParentMemberships.First().TargetId.Should().Be(target.Id);
        source.ParentMemberships.First().DateRange.Start.Should().Be(start);
        source.ParentMemberships.First().DateRange.End.Should().BeNull();
    }

    [Fact]
    public void AddTeamToTeamMembership_WhenSourceInactive_Failure()
    {
        // Arrange
        var source = GenerateTeamOfTeams(false);
        var target = GenerateTeamOfTeams(true);
        LocalDate start = new(2023, 1, 1);
        LocalDate? end = null;
        MembershipDateRange dateRange = new(start, end);

        var expectedErrorMessage = $"Memberships can not be added to inactive teams. {source.Name} is inactive.";

        // Act
        var result = source.AddTeamMembership(target, dateRange, _now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(expectedErrorMessage);
        source.ParentMemberships.Count.Should().Be(0);
    }

    [Fact]
    public void AddTeamToTeamMembership_WhenTargetInactive_Failure()
    {
        // Arrange
        var source = GenerateTeamOfTeams(true);
        var target = GenerateTeamOfTeams(false);
        LocalDate start = new(2023, 1, 1);
        LocalDate? end = null;
        MembershipDateRange dateRange = new(start, end);

        var expectedErrorMessage = $"Memberships can not be added to inactive teams. {target.Name} is inactive.";

        // Act
        var result = source.AddTeamMembership(target, dateRange, _now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(expectedErrorMessage);
        source.ParentMemberships.Count.Should().Be(0);
    }

    [Fact]
    public void UpdateTeamToTeamMembership_WithValidDates_Success()
    {
        // Arrange
        var source = GenerateTeamOfTeams(true);
        var target = GenerateTeamOfTeams(true);
        LocalDate start = new(2023, 1, 1);
        LocalDate? end = new(2023, 5, 1);
        MembershipDateRange dateRange = new(start, end);
        var createresult = source.AddTeamMembership(target, dateRange, _now);

        var membership = source.ParentMemberships.First();
        membership.SetPrivate(m => m.Id, Guid.NewGuid());
        membership.SetPrivate(m => m.Source, source);
        membership.SetPrivate(m => m.Target, target);

        LocalDate updatedStart = new(2023, 2, 1);
        LocalDate? updatedEnd = new(2023, 5, 10);
        MembershipDateRange updatedDateRange = new(updatedStart, updatedEnd);

        // Act
        var result = source.UpdateTeamMembership(membership.Id, updatedDateRange, _now);

        // Assert
        result.IsSuccess.Should().BeTrue();
        source.ParentMemberships.Count.Should().Be(1);
        source.ParentMemberships.First().SourceId.Should().Be(source.Id);
        source.ParentMemberships.First().TargetId.Should().Be(target.Id);
        source.ParentMemberships.First().DateRange.Start.Should().Be(updatedStart);
        source.ParentMemberships.First().DateRange.End.Should().Be(updatedEnd);
    }

    [Fact]
    public void UpdateTeamToTeamMembership_WhenSourceIsInactive_Failure()
    {
        // Arrange
        var source = GenerateTeamOfTeams(true);
        var target = GenerateTeamOfTeams(true);
        LocalDate start = new(2023, 1, 1);
        LocalDate? end = new(2023, 5, 1);
        MembershipDateRange dateRange = new(start, end);
        var createresult = source.AddTeamMembership(target, dateRange, _now);

        var membership = source.ParentMemberships.First();
        membership.SetPrivate(m => m.Id, Guid.NewGuid());
        membership.SetPrivate(m => m.Source, source);
        membership.SetPrivate(m => m.Target, target);

        LocalDate updatedStart = new(2023, 2, 1);
        LocalDate? updatedEnd = new(2023, 5, 10);
        MembershipDateRange updatedDateRange = new(updatedStart, updatedEnd);

        source.SetPrivate(m => m.IsActive, false);

        // Act
        var result = source.UpdateTeamMembership(membership.Id, updatedDateRange, _now);

        // Assert
        result.IsFailure.Should().BeTrue();
        source.ParentMemberships.Count.Should().Be(1);
        source.ParentMemberships.First().DateRange.Start.Should().Be(start);
        source.ParentMemberships.First().DateRange.End.Should().Be(end);
    }

    [Fact]
    public void UpdateTeamToTeamMembership_WhenTargetIsInactive_Failure()
    {
        // Arrange
        var source = GenerateTeamOfTeams(true);
        var target = GenerateTeamOfTeams(true);
        LocalDate start = new(2023, 1, 1);
        LocalDate? end = new(2023, 5, 1);
        MembershipDateRange dateRange = new(start, end);
        var createresult = source.AddTeamMembership(target, dateRange, _now);

        var membership = source.ParentMemberships.First();
        membership.SetPrivate(m => m.Id, Guid.NewGuid());
        membership.SetPrivate(m => m.Source, source);
        membership.SetPrivate(m => m.Target, target);

        LocalDate updatedStart = new(2023, 2, 1);
        LocalDate? updatedEnd = new(2023, 5, 10);
        MembershipDateRange updatedDateRange = new(updatedStart, updatedEnd);

        target.SetPrivate(m => m.IsActive, false);

        // Act
        var result = source.UpdateTeamMembership(membership.Id, updatedDateRange, _now);

        // Assert
        result.IsFailure.Should().BeTrue();
        source.ParentMemberships.Count.Should().Be(1);
        source.ParentMemberships.First().DateRange.Start.Should().Be(start);
        source.ParentMemberships.First().DateRange.End.Should().Be(end);
    }

    [Fact]
    public void RemoveTeamToTeamMembership_WhenValid_Success()
    {
        // Arrange
        var source = GenerateTeamOfTeams(true);
        var target = GenerateTeamOfTeams(true);
        LocalDate start = new(2023, 1, 1);
        LocalDate? end = new(2023, 5, 1);
        MembershipDateRange dateRange = new(start, end);
        var createresult = source.AddTeamMembership(target, dateRange, _now);

        var membership = source.ParentMemberships.First();
        membership.SetPrivate(m => m.Id, Guid.NewGuid());
        membership.SetPrivate(m => m.Source, source);
        membership.SetPrivate(m => m.Target, target);

        // Act
        var result = source.RemoveTeamMembership(membership.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        source.ParentMemberships.Count.Should().Be(0);
    }

    [Fact]
    public void RemoveTeamToTeamMembership_WhenSourceIsInactive_Failure()
    {
        // Arrange
        var source = GenerateTeamOfTeams(true);
        var target = GenerateTeamOfTeams(true);
        LocalDate start = new(2023, 1, 1);
        LocalDate? end = new(2023, 5, 1);
        MembershipDateRange dateRange = new(start, end);
        var createresult = source.AddTeamMembership(target, dateRange, _now);

        var membership = source.ParentMemberships.First();
        membership.SetPrivate(m => m.Id, Guid.NewGuid());
        membership.SetPrivate(m => m.Source, source);
        membership.SetPrivate(m => m.Target, target);

        source.SetPrivate(m => m.IsActive, false);

        // Act
        var result = source.RemoveTeamMembership(membership.Id);

        // Assert
        result.IsFailure.Should().BeTrue();
        source.ParentMemberships.Count.Should().Be(1);
    }

    [Fact]
    public void RemoveTeamToTeamMembership_WhenTargetIsInactive_Failure()
    {
        // Arrange
        var source = GenerateTeamOfTeams(true);
        var target = GenerateTeamOfTeams(true);
        LocalDate start = new(2023, 1, 1);
        LocalDate? end = new(2023, 5, 1);
        MembershipDateRange dateRange = new(start, end);
        var createresult = source.AddTeamMembership(target, dateRange, _now);

        var membership = source.ParentMemberships.First();
        membership.SetPrivate(m => m.Id, Guid.NewGuid());
        membership.SetPrivate(m => m.Source, source);
        membership.SetPrivate(m => m.Target, target);

        target.SetPrivate(m => m.IsActive, false);

        // Act
        var result = source.RemoveTeamMembership(membership.Id);

        // Assert
        result.IsFailure.Should().BeTrue();
        source.ParentMemberships.Count.Should().Be(1);
    }


    #endregion Memberships
}
