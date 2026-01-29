using Moda.Common.Domain.Enums.Organization;
using Moda.Common.Domain.Events.Organization;
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

        sut.PostPersistenceActions.Should().NotBeEmpty();

        // get the first action and execute it to ensure it's a valid TeamCreatedEvent
        var action = sut.PostPersistenceActions.First();
        action.Should().NotBeNull();
        action();
        sut.DomainEvents.Should().NotBeEmpty();
        sut.DomainEvents.Should().ContainSingle(e => e is TeamCreatedEvent);

        // get the event and verify its properties
        var createdEvent = sut.DomainEvents.OfType<TeamCreatedEvent>().First();
        createdEvent.Id.Should().Be(sut.Id);
        createdEvent.Key.Should().Be(sut.Key);
        createdEvent.Code.Should().Be(sut.Code);
        createdEvent.Name.Should().Be(sut.Name);
        createdEvent.Description.Should().Be(sut.Description);
        createdEvent.Type.Should().Be(TeamType.Team);
        createdEvent.ActiveDate.Should().Be(sut.ActiveDate);
        createdEvent.InactiveDate.Should().BeNull();
        createdEvent.IsActive.Should().BeTrue();
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
        team.DomainEvents.Should().ContainSingle(e => e is TeamUpdatedEvent);
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
        team.DomainEvents.Should().ContainSingle(e => e is TeamUpdatedEvent);
    }

    #endregion Update


    #region IActivatable

    [Fact]
    public void Activate_WhenActive_SetsIsActiveToFalse()
    {
        // Arrange
        var inactiveDate = _dateTimeProvider.Now.Minus(Duration.FromDays(10)).InUtc().LocalDateTime.Date;
        var team = _teamFaker.AsInactive(inactiveDate).Generate();

        // Act
        team.Activate(_dateTimeProvider.Now);

        // Assert
        team.IsActive.Should().BeTrue();

        team.DomainEvents.Should().NotBeEmpty();
        team.DomainEvents.Should().ContainSingle(e => e is TeamActivatedEvent);
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
        team.DomainEvents.Should().ContainSingle(e => e is TeamDeactivatedEvent);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_Failure()
    {
        // Arrange
        var inactiveDate = _dateTimeProvider.Now.Minus(Duration.FromDays(10)).InUtc().LocalDateTime.Date;
        var team = _teamFaker.AsInactive(inactiveDate).Generate();
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
        var team = _teamFaker
            .WithActiveDate(activeDate)
            .AsInactive(inactiveDate)
            .Generate();
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
        var teamOfTeams = _teamOfTeamsFaker.AsInactive(inactiveDate).Generate();
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


    #region OperatingModels

    [Fact]
    public void SetOperatingModel_WhenNoExistingModels_Success()
    {
        // Arrange
        var team = _teamFaker.Generate();
        var startDate = new LocalDate(2024, 1, 1);
        var methodology = Moda.Organization.Domain.Enums.Methodology.Scrum;
        var sizingMethod = Moda.Organization.Domain.Enums.SizingMethod.StoryPoints;

        // Act
        var result = team.SetOperatingModel(startDate, methodology, sizingMethod);

        // Assert
        result.IsSuccess.Should().BeTrue();
        team.OperatingModels.Should().HaveCount(1);
        
        var model = team.OperatingModels.First();
        model.TeamId.Should().Be(team.Id);
        model.DateRange.Start.Should().Be(startDate);
        model.DateRange.End.Should().BeNull();
        model.Methodology.Should().Be(methodology);
        model.SizingMethod.Should().Be(sizingMethod);
        model.IsCurrent.Should().BeTrue();
    }

    [Fact]
    public void SetOperatingModel_WhenCurrentModelExists_ClosesCurrentAndCreatesNew()
    {
        // Arrange
        var team = _teamFaker.Generate();
        var firstStartDate = new LocalDate(2023, 1, 1);
        var secondStartDate = new LocalDate(2024, 1, 1);
        var methodology1 = Moda.Organization.Domain.Enums.Methodology.Scrum;
        var methodology2 = Moda.Organization.Domain.Enums.Methodology.Kanban;
        var sizingMethod1 = Moda.Organization.Domain.Enums.SizingMethod.StoryPoints;
        var sizingMethod2 = Moda.Organization.Domain.Enums.SizingMethod.Count;

        // Create first operating model
        var result1 = team.SetOperatingModel(firstStartDate, methodology1, sizingMethod1);

        // Act - Create second operating model
        var result2 = team.SetOperatingModel(secondStartDate, methodology2, sizingMethod2);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        team.OperatingModels.Should().HaveCount(2);

        // First model should be closed
        var firstModel = team.OperatingModels.First();
        firstModel.IsCurrent.Should().BeFalse();
        firstModel.DateRange.Start.Should().Be(firstStartDate);
        firstModel.DateRange.End.Should().Be(new LocalDate(2023, 12, 31));
        firstModel.Methodology.Should().Be(methodology1);
        firstModel.SizingMethod.Should().Be(sizingMethod1);

        // Second model should be current
        var secondModel = team.OperatingModels.Last();
        secondModel.IsCurrent.Should().BeTrue();
        secondModel.DateRange.Start.Should().Be(secondStartDate);
        secondModel.DateRange.End.Should().BeNull();
        secondModel.Methodology.Should().Be(methodology2);
        secondModel.SizingMethod.Should().Be(sizingMethod2);
    }

    [Fact]
    public void SetOperatingModel_WithStartDateBeforeCurrentStart_ReturnsFailure()
    {
        // Arrange
        var team = _teamFaker.Generate();
        var firstStartDate = new LocalDate(2024, 1, 1);
        var secondStartDate = new LocalDate(2023, 12, 31); // Before first
        var methodology = Moda.Organization.Domain.Enums.Methodology.Scrum;
        var sizingMethod = Moda.Organization.Domain.Enums.SizingMethod.StoryPoints;

        // Create first operating model
        team.SetOperatingModel(firstStartDate, methodology, sizingMethod);

        // Act - Try to create second operating model with earlier start date
        var result = team.SetOperatingModel(secondStartDate, methodology, sizingMethod);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("New operating model start date must be after the current model's start date.");
        team.OperatingModels.Should().HaveCount(1); // Only the first model should exist
    }

    [Fact]
    public void SetOperatingModel_WithMultipleModelsOverTime_Success()
    {
        // Arrange
        var team = _teamFaker.Generate();
        var date1 = new LocalDate(2022, 1, 1);
        var date2 = new LocalDate(2023, 1, 1);
        var date3 = new LocalDate(2024, 1, 1);

        // Act - Create three operating models over time
        var result1 = team.SetOperatingModel(date1, Moda.Organization.Domain.Enums.Methodology.Scrum, Moda.Organization.Domain.Enums.SizingMethod.StoryPoints);
        var result2 = team.SetOperatingModel(date2, Moda.Organization.Domain.Enums.Methodology.Kanban, Moda.Organization.Domain.Enums.SizingMethod.Count);
        var result3 = team.SetOperatingModel(date3, Moda.Organization.Domain.Enums.Methodology.Scrum, Moda.Organization.Domain.Enums.SizingMethod.StoryPoints);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result3.IsSuccess.Should().BeTrue();
        team.OperatingModels.Should().HaveCount(3);

        // All but the last should be closed
        var models = team.OperatingModels.OrderBy(m => m.DateRange.Start).ToList();
        
        models[0].IsCurrent.Should().BeFalse();
        models[0].DateRange.Start.Should().Be(date1);
        models[0].DateRange.End.Should().Be(new LocalDate(2022, 12, 31));

        models[1].IsCurrent.Should().BeFalse();
        models[1].DateRange.Start.Should().Be(date2);
        models[1].DateRange.End.Should().Be(new LocalDate(2023, 12, 31));

        models[2].IsCurrent.Should().BeTrue();
        models[2].DateRange.Start.Should().Be(date3);
        models[2].DateRange.End.Should().BeNull();
    }

    [Fact]
    public void RemoveOperatingModel_WhenModelExists_Success()
    {
        // Arrange
        var team = _teamFaker.Generate();
        var startDate = new LocalDate(2024, 1, 1);
        var methodology = Moda.Organization.Domain.Enums.Methodology.Scrum;
        var sizingMethod = Moda.Organization.Domain.Enums.SizingMethod.StoryPoints;

        var createResult = team.SetOperatingModel(startDate, methodology, sizingMethod);
        
        // Set a unique ID for the model (simulating what EF Core would do)
        var modelId = Guid.NewGuid();
        createResult.Value.SetPrivate(m => m.Id, modelId);

        // Act
        var result = team.RemoveOperatingModel(modelId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        team.OperatingModels.Should().BeEmpty();
    }

    [Fact]
    public void RemoveOperatingModel_WhenModelDoesNotExist_ReturnsFailure()
    {
        // Arrange
        var team = _teamFaker.Generate();
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = team.RemoveOperatingModel(nonExistentId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be($"Operating model with Id {nonExistentId} not found for this team.");
    }

    [Fact]
    public void RemoveOperatingModel_WhenMultipleModelsExist_RemovesOnlySpecified()
    {
        // Arrange
        var team = _teamFaker.Generate();
        var date1 = new LocalDate(2023, 1, 1);
        var date2 = new LocalDate(2024, 1, 1);

        var result1 = team.SetOperatingModel(date1, Moda.Organization.Domain.Enums.Methodology.Scrum, Moda.Organization.Domain.Enums.SizingMethod.StoryPoints);
        var result2 = team.SetOperatingModel(date2, Moda.Organization.Domain.Enums.Methodology.Kanban, Moda.Organization.Domain.Enums.SizingMethod.Count);

        // Set unique IDs for the models (simulating what EF Core would do)
        var model1Id = Guid.NewGuid();
        var model2Id = Guid.NewGuid();
        result1.Value.SetPrivate(m => m.Id, model1Id);
        result2.Value.SetPrivate(m => m.Id, model2Id);

        // Act - Remove the first model
        var removeResult = team.RemoveOperatingModel(model1Id);

        // Assert
        removeResult.IsSuccess.Should().BeTrue();
        team.OperatingModels.Should().HaveCount(1);
        team.OperatingModels.First().Id.Should().Be(model2Id);
    }

    [Fact]
    public void OperatingModels_InitiallyEmpty()
    {
        // Arrange & Act
        var team = _teamFaker.Generate();

        // Assert
        team.OperatingModels.Should().NotBeNull();
        team.OperatingModels.Should().BeEmpty();
    }

    [Fact]
    public void OperatingModels_IsReadOnly()
    {
        // Arrange
        var team = _teamFaker.Generate();

        // Act & Assert
        team.OperatingModels.Should().BeAssignableTo<IReadOnlyCollection<TeamOperatingModel>>();
    }

    #endregion OperatingModels
}
