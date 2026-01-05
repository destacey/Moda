using FluentAssertions;
using Moda.Common.Domain.Models.ProjectPortfolioManagement;
using Moda.Common.Models;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.ProjectPortfolioManagement.Domain.Models;
using Moda.ProjectPortfolioManagement.Domain.Tests.Data;
using Moda.Tests.Shared;
using NodaTime.Extensions;
using NodaTime.Testing;

namespace Moda.ProjectPortfolioManagement.Domain.Tests.Sut.Models;

public class ProjectTests
{
    private readonly TestingDateTimeProvider _dateTimeProvider;
    private readonly ProjectFaker _projectFaker;
    private readonly StrategicThemeFaker _themeFaker;

    public ProjectTests()
    {
        _dateTimeProvider = new TestingDateTimeProvider(new FakeClock(DateTime.UtcNow.ToInstant()));
        _projectFaker = new ProjectFaker();
        _themeFaker = new StrategicThemeFaker();
    }

    #region Project Create and Update

    [Fact]
    public void Create_ShouldCreateProposedProjectSuccessfully()
    {
        // Arrange
        var name = "Test Project";
        var description = "Test Description";
        var key = new ProjectKey("TEST");
        var portfolioId = Guid.NewGuid();
        var expenditureCategoryId = 1;

        // Act
        var project = Project.Create(name, description, key, expenditureCategoryId, null, portfolioId, null, null, null, _dateTimeProvider.Now);

        // Assert
        project.Should().NotBeNull();
        project.Name.Should().Be(name);
        project.Description.Should().Be(description);
        project.Key.Value.Should().Be(key);
        project.Status.Should().Be(ProjectStatus.Proposed);
        project.ExpenditureCategoryId.Should().Be(expenditureCategoryId);
        project.PortfolioId.Should().Be(portfolioId);
        project.ProgramId.Should().BeNull();
        project.DateRange.Should().BeNull();
    }

    [Fact]
    public void UpdateDetails_ShouldFail_WhenNameIsEmpty()
    {
        // Arrange
        var project = _projectFaker.Generate();

        // Act
        Action action = () => project.UpdateDetails("", "Valid Description", project.ExpenditureCategoryId, _dateTimeProvider.Now);

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("Required input Name was empty. (Parameter 'Name')");
    }

    [Fact]
    public void UpdateDetails_ShouldFail_WhenDescriptionIsEmpty()
    {
        // Arrange
        var project = _projectFaker.Generate();

        // Act
        Action action = () => project.UpdateDetails("Valid Name", "", project.ExpenditureCategoryId, _dateTimeProvider.Now);

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("Required input Description was empty. (Parameter 'Description')");
    }

    #endregion Project Create and Update

    #region UpdateTimeline Tests

    [Fact]
    public void UpdateTimeline_ShouldUpdatePlannedDatesSuccessfully_WhenProjectIsProposed()
    {
        // Arrange
        var project = _projectFaker.Generate();
        var startDate = _dateTimeProvider.Today;
        var endDate = _dateTimeProvider.Today.PlusDays(30);
        var dateRange = new LocalDateRange(startDate, endDate);

        // Act
        var result = project.UpdateTimeline(dateRange);

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.DateRange.Should().NotBeNull();
        project.DateRange!.Start.Should().Be(startDate);
        project.DateRange.End.Should().Be(endDate);
    }

    [Fact]
    public void UpdateTimeline_ShouldFail_WhenProjectIsActive_AndDatesAreNull()
    {
        // Arrange
        var project = _projectFaker.AsActive(_dateTimeProvider, Guid.NewGuid());

        // Act
        var result = project.UpdateTimeline(null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Active and completed projects must have a start and end date.");
    }

    [Fact]
    public void UpdateTimeline_ShouldFail_WhenProjectIsCompleted_AndDatesAreNull()
    {
        // Arrange
        var project = _projectFaker.AsCompleted(_dateTimeProvider, Guid.NewGuid());

        // Act
        var result = project.UpdateTimeline(null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Active and completed projects must have a start and end date.");
    }

    [Fact]
    public void UpdateTimeline_ShouldUpdateSuccessfully_WhenProjectIsActive_AndDatesAreValid()
    {
        // Arrange
        var project = _projectFaker.AsActive(_dateTimeProvider, Guid.NewGuid());
        var startDate = _dateTimeProvider.Today;
        var endDate = _dateTimeProvider.Today.PlusDays(60);
        var dateRange = new LocalDateRange(startDate, endDate);

        // Act
        var result = project.UpdateTimeline(dateRange);

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.DateRange.Should().NotBeNull();
        project.DateRange!.Start.Should().Be(startDate);
        project.DateRange.End.Should().Be(endDate);
    }

    #endregion UpdateTimeline Tests

    #region Roles

    [Fact]
    public void AssignRole_ShouldAssignEmployeeToRoleSuccessfully()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var project = _projectFaker.Generate();

        // Act
        var result = project.AssignRole(ProjectRole.Owner, employeeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.Roles.Should().ContainSingle();
        project.Roles.First().Role.Should().Be(ProjectRole.Owner);
        project.Roles.First().EmployeeId.Should().Be(employeeId);
    }

    [Fact]
    public void AssignRole_ShouldFail_WhenEmployeeAlreadyAssignedToRole()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var project = _projectFaker.WithData(roles: new Dictionary<ProjectRole, HashSet<Guid>>
        {
            { ProjectRole.Owner, new HashSet<Guid> { employeeId } }
        }).Generate();

        // Act
        var result = project.AssignRole(ProjectRole.Owner, employeeId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Employee is already assigned to this role.");
    }

    [Fact]
    public void RemoveRole_WithOneRoleAssignment_ShouldRemoveEmployeeFromRoleSuccessfully()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var project = _projectFaker.WithData(roles: new Dictionary<ProjectRole, HashSet<Guid>>
        {
            { ProjectRole.Owner, new HashSet<Guid> { employeeId } }
        }).Generate();

        // Act
        var result = project.RemoveRole(ProjectRole.Owner, employeeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.Roles.Should().BeEmpty();
    }

    [Fact]
    public void RemoveRole_WithMultipleRoleAssignments_ShouldRemoveEmployeeFromRoleSuccessfully()
    {
        // Arrange
        var employeeId1 = Guid.NewGuid();
        var employeeId2 = Guid.NewGuid();
        var project = _projectFaker.WithData(roles: new Dictionary<ProjectRole, HashSet<Guid>>
        {
            { ProjectRole.Owner, new HashSet<Guid> { employeeId1, employeeId2 } }
        }).Generate();

        // Act
        var result = project.RemoveRole(ProjectRole.Owner, employeeId1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.Roles.Count.Should().Be(1);
        project.Roles.First().Role.Should().Be(ProjectRole.Owner);
        project.Roles.First().EmployeeId.Should().Be(employeeId2);
    }

    [Fact]
    public void RemoveRole_ShouldFail_WhenEmployeeNotAssignedToRole()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var project = _projectFaker.Generate();

        // Act
        var result = project.RemoveRole(ProjectRole.Owner, employeeId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Employee is not assigned to this role.");
    }


    [Fact]
    public void UpdateRoles_ShouldAssignNewRolesSuccessfully()
    {
        // Arrange
        var project = _projectFaker.Generate();
        var employee1 = Guid.NewGuid();
        var employee2 = Guid.NewGuid();
        var updatedRoles = new Dictionary<ProjectRole, HashSet<Guid>>
        {
            { ProjectRole.Manager, new HashSet<Guid> { employee1, employee2 } }
        };

        // Act
        var result = project.UpdateRoles(updatedRoles);

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.Roles.Should().Contain(role => role.Role == ProjectRole.Manager && role.EmployeeId == employee1);
        project.Roles.Should().Contain(role => role.Role == ProjectRole.Manager && role.EmployeeId == employee2);
    }

    [Fact]
    public void UpdateRoles_ShouldRemoveUnspecifiedRoles()
    {
        // Arrange
        var project = _projectFaker.WithData(roles: new Dictionary<ProjectRole, HashSet<Guid>>
        {
            { ProjectRole.Manager, new HashSet<Guid> { Guid.NewGuid(), Guid.NewGuid() } },
            { ProjectRole.Owner, new HashSet<Guid> { Guid.NewGuid() } }
        }).Generate();

        var updatedRoles = new Dictionary<ProjectRole, HashSet<Guid>>
        {
            { ProjectRole.Manager, new HashSet<Guid> { Guid.NewGuid() } }  // Remove Owner role
        };

        // Act
        var result = project.UpdateRoles(updatedRoles);

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.Roles.Should().Contain(role => role.Role == ProjectRole.Manager);
        project.Roles.Should().NotContain(role => role.Role == ProjectRole.Owner); // Removed role
    }

    [Fact]
    public void UpdateRoles_ShouldNotChange_WhenRolesAreUnchanged()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var project = _projectFaker.WithData(roles: new Dictionary<ProjectRole, HashSet<Guid>>
        {
            { ProjectRole.Sponsor, new HashSet<Guid> { employeeId } }
        }).Generate();

        var updatedRoles = new Dictionary<ProjectRole, HashSet<Guid>>
        {
            { ProjectRole.Sponsor, new HashSet<Guid> { employeeId } }
        };

        // Act
        var result = project.UpdateRoles(updatedRoles);

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.Roles.Count.Should().Be(1);
        project.Roles.Should().Contain(role => role.Role == ProjectRole.Sponsor && role.EmployeeId == employeeId);
    }

    [Fact]
    public void UpdateRoles_ShouldFail_WhenInvalidRoleProvided()
    {
        // Arrange
        var project = _projectFaker.Generate();
        var invalidRole = (ProjectRole)999;
        var updatedRoles = new Dictionary<ProjectRole, HashSet<Guid>>
        {
            { invalidRole, new HashSet<Guid> { Guid.NewGuid() } }
        };

        // Act
        var result = project.UpdateRoles(updatedRoles);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be($"Role is not a valid {nameof(ProjectRole)} value.");
    }

    #endregion Roles

    #region Lifecycle Tests

    [Fact]
    public void Activate_ShouldActivateProposedProjectSuccessfully()
    {
        // Arrange
        var dateRange = new LocalDateRange(_dateTimeProvider.Today, _dateTimeProvider.Today.PlusMonths(3));
        var project = _projectFaker.WithData(dateRange: dateRange).Generate();

        // Act
        var result = project.Activate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.Status.Should().Be(ProjectStatus.Active);
        project.DateRange.Should().NotBeNull();
        project.DateRange.Should().Be(dateRange);
    }

    [Fact]
    public void Activate_ShouldFail_WhenProjectIsNotProposed()
    {
        // Arrange
        var project = _projectFaker.AsActive(_dateTimeProvider, Guid.NewGuid());

        // Act
        var result = project.Activate();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Only proposed projects can be activated.");
    }

    [Fact]
    public void Complete_ShouldCompleteActiveProjectSuccessfully()
    {
        // Arrange
        var project = _projectFaker.AsActive(_dateTimeProvider, Guid.NewGuid());

        // Act
        var result = project.Complete();

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.Status.Should().Be(ProjectStatus.Completed);
    }

    [Fact]
    public void Complete_ShouldFail_WhenProjectIsNotActive()
    {
        // Arrange
        var project = _projectFaker.Generate();
        var endDate = _dateTimeProvider.Today.PlusDays(10);

        // Act
        var result = project.Complete();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Only active projects can be completed.");
    }

    [Fact]
    public void Cancel_ShouldCancelActiveProjectSuccessfully()
    {
        // Arrange
        var project = _projectFaker.AsActive(_dateTimeProvider, Guid.NewGuid());

        // Act
        var result = project.Cancel();

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.Status.Should().Be(ProjectStatus.Cancelled);
    }

    [Fact]
    public void Cancel_ShouldFail_WhenProjectIsAlreadyCompletedOrCancelled()
    {
        // Arrange
        var project = _projectFaker.AsCancelled(_dateTimeProvider, Guid.NewGuid());
        var endDate = _dateTimeProvider.Today.PlusDays(10);

        // Act
        var result = project.Cancel();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("The project is already completed or cancelled.");
    }

    #endregion Lifecycle Tests

    #region Program Association Tests

    [Fact]
    public void UpdateProgram_ShouldAssociateProjectWithProgramSuccessfully()
    {
        // Arrange
        var project = _projectFaker.Generate();
        var program = Program.Create("Test Program", "Description", null, project.PortfolioId, null, null, _dateTimeProvider.Now);

        // Act
        var result = project.UpdateProgram(program);

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.ProgramId.Should().Be(program.Id);
    }

    [Fact]
    public void UpdateProgram_ShouldFail_WhenProgramIsInDifferentPortfolio()
    {
        // Arrange
        var project = _projectFaker.Generate();
        var portfolioId = Guid.NewGuid();
        var program = Program.Create("Test Program", "Description", null, portfolioId, null, null, _dateTimeProvider.Now);

        // Act
        var result = project.UpdateProgram(program);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("The project must belong to the same portfolio as the program.");
    }

    [Fact]
    public void UpdateProgram_ShouldRemoveProgramAssociation_WhenNullProgramPassed()
    {
        // Arrange
        var project = _projectFaker.WithData(programId: Guid.NewGuid()).Generate();

        // Act
        var result = project.UpdateProgram(null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.ProgramId.Should().BeNull();
    }

    #endregion Program Association Tests

    #region Strategic Theme Management

    [Fact]
    public void UpdateStrategicThemes_ShouldUpdateThemesSuccessfully()
    {
        // Arrange
        var project = _projectFaker.Generate();
        var themes = _themeFaker.Generate(3); // Generate 3 unique themes

        // Act
        var result = project.UpdateStrategicThemes(themes.Select(t => t.Id).ToHashSet());

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.StrategicThemeTags.Should().HaveCount(3);
        project.StrategicThemeTags.Select(t => t.StrategicThemeId).Should().BeEquivalentTo(themes.Select(t => t.Id));
    }

    [Fact]
    public void UpdateStrategicThemes_ShouldRemoveExistingThemes_WhenNewThemesAreAdded()
    {
        // Arrange
        var project = _projectFaker.Generate();
        var initialThemes = _themeFaker.Generate(2);
        project.UpdateStrategicThemes(initialThemes.Select(t => t.Id).ToHashSet());

        var newThemes = _themeFaker.Generate(3); // Replace with different themes

        // Act
        var result = project.UpdateStrategicThemes(newThemes.Select(t => t.Id).ToHashSet());

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.StrategicThemeTags.Should().HaveCount(3);
        project.StrategicThemeTags.Select(t => t.StrategicThemeId).Should().BeEquivalentTo(newThemes.Select(t => t.Id));
    }

    [Fact]
    public void UpdateStrategicThemes_ShouldSucceed_WhenNoChangesAreMade()
    {
        // Arrange
        var project = _projectFaker.Generate();
        var themes = _themeFaker.Generate(2);
        project.UpdateStrategicThemes(themes.Select(t => t.Id).ToHashSet());

        // Act
        var result = project.UpdateStrategicThemes(themes.Select(t => t.Id).ToHashSet()); // Same themes

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.StrategicThemeTags.Should().HaveCount(2);
    }

    [Fact]
    public void UpdateStrategicThemes_ShouldHandleEmptyListCorrectly()
    {
        // Arrange
        var project = _projectFaker.Generate();
        var initialThemes = _themeFaker.Generate(2);
        project.UpdateStrategicThemes(initialThemes.Select(t => t.Id).ToHashSet());

        // Act
        var result = project.UpdateStrategicThemes([]);

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.StrategicThemeTags.Should().BeEmpty();
    }

    #endregion Strategic Theme Management

    #region Key Management

    [Fact]
    public void ChangeKey_ShouldUpdateProjectKeySuccessfully()
    {
        // Arrange
        var project = _projectFaker.Generate();
        var originalKey = project.Key;
        var newKey = new ProjectKey("NEWPROJ");

        // Act
        var result = project.ChangeKey(newKey);

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.Key.Should().Be(newKey);
        project.Key.Should().NotBe(originalKey);
    }

    [Fact]
    public void ChangeKey_ShouldBeNoOp_WhenKeyIsUnchanged()
    {
        // Arrange
        var project = _projectFaker.Generate();
        var originalKey = project.Key;

        // Act
        var result = project.ChangeKey(originalKey);

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.Key.Should().Be(originalKey);
    }

    [Fact]
    public void ChangeKey_ShouldUpdateAllTaskKeys_WhenProjectHasTasks()
    {
        // Arrange
        var project = _projectFaker.Generate();
        var task1 = project.CreateTask(
            nextNumber: 1,
            name: "Task 1",
            description: null,
            type: ProjectTaskType.Task,
            status: Enums.TaskStatus.NotStarted,
            priority: TaskPriority.Medium,
            progress: null,
            parentId: null,
            plannedDateRange: null,
            plannedDate: null,
            estimatedEffortHours: null,
            assignments: null).Value;

        var task2 = project.CreateTask(
            nextNumber: 2,
            name: "Task 2",
            description: null,
            type: ProjectTaskType.Task,
            status: Enums.TaskStatus.NotStarted,
            priority: TaskPriority.Medium,
            progress: null,
            parentId: null,
            plannedDateRange: null,
            plannedDate: null,
            estimatedEffortHours: null,
            assignments: null).Value;

        var newKey = new ProjectKey("NEWTASKS");

        // Act
        var result = project.ChangeKey(newKey);

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.Key.Should().Be(newKey);

        task1.Key.Value.Should().Be($"{newKey.Value}-1");
        task2.Key.Value.Should().Be($"{newKey.Value}-2");
    }

    #endregion Key Management
}
