﻿using FluentAssertions;
using Moda.Common.Domain.Employees;
using Moda.Common.Models;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.ProjectPortfolioManagement.Domain.Models;
using Moda.ProjectPortfolioManagement.Domain.Tests.Data;
using Moda.Tests.Shared;
using NodaTime.Extensions;
using NodaTime.Testing;

namespace Moda.ProjectPortfolioManagement.Domain.Tests.Sut.Models;

public class ProjectPortfolioTests
{
    private readonly TestingDateTimeProvider _dateTimeProvider;
    private readonly ProjectPortfolioFaker _portfolioFaker;
    private readonly ProgramFaker _programFaker;
    private readonly ProjectFaker _projectFaker;

    public ProjectPortfolioTests()
    {
        _dateTimeProvider = new TestingDateTimeProvider(new FakeClock(DateTime.UtcNow.ToInstant()));
        _portfolioFaker = new ProjectPortfolioFaker();
        _programFaker = new ProgramFaker();
        _projectFaker = new ProjectFaker();
    }

    #region Portfolio Create and Update

    [Fact]
    public void Create_ShouldCreateProposedPortfolioSuccessfully()
    {
        // Arrange
        var name = "Test Portfolio";
        var description = "Test Description";

        // Act
        var portfolio = ProjectPortfolio.Create(name, description);

        // Assert
        portfolio.Should().NotBeNull();
        portfolio.Name.Should().Be(name);
        portfolio.Description.Should().Be(description);
        portfolio.Status.Should().Be(ProjectPortfolioStatus.Proposed);
        portfolio.DateRange.Should().BeNull();
        portfolio.Projects.Should().BeEmpty();
        portfolio.Programs.Should().BeEmpty();
    }

    [Fact]
    public void Update_ShouldUpdatePortfolioSuccessfully()
    {
        // Arrange
        var portfolio = _portfolioFaker.Generate();
        var updatedName = "Updated Portfolio";
        var updatedDescription = "Updated Description";

        // Act
        var result = portfolio.UpdateDetails(updatedName, updatedDescription);

        // Assert
        result.IsSuccess.Should().BeTrue();
        portfolio.Name.Should().Be(updatedName);
        portfolio.Description.Should().Be(updatedDescription);
    }

    [Fact]
    public void Update_ShouldFail_WhenPortfolioIsReadonly()
    {
        // Arrange
        var portfolio = _portfolioFaker.ArchivedPortfolio(_dateTimeProvider);
        var updatedName = "Updated Portfolio";
        var updatedDescription = "Updated Description";

        // Act
        var result = portfolio.UpdateDetails(updatedName, updatedDescription);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Project Portfolio is readonly and cannot be updated.");
    }

    #endregion Portfolio Create and Update

    #region Roles

    [Fact]
    public void AssignRole_ShouldAssignEmployeeToPortfolioRoleSuccessfully()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var portfolio = _portfolioFaker.Generate();

        // Act
        var result = portfolio.AssignRole(ProjectPortfolioRole.Owner, employeeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        portfolio.Roles.Should().ContainSingle();
        portfolio.Roles.First().Role.Should().Be(ProjectPortfolioRole.Owner);
        portfolio.Roles.First().EmployeeId.Should().Be(employeeId);
    }

    [Fact]
    public void AssignRole_ShouldFail_WhenEmployeeAlreadyAssignedToRole()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var portfolio = _portfolioFaker.WithData(roles: new Dictionary<ProjectPortfolioRole, HashSet<Guid>>
        {
            { ProjectPortfolioRole.Owner, new HashSet<Guid> { employeeId } }
        }).Generate();

        // Act
        var result = portfolio.AssignRole(ProjectPortfolioRole.Owner, employeeId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Employee is already assigned to this role.");
    }

    [Fact]
    public void AssignRole_ShouldFail_WhenPortfolioIsReadonly()
    {
        // Arrange
        var portfolio = _portfolioFaker.ArchivedPortfolio(_dateTimeProvider);

        // Act
        var result = portfolio.AssignRole(ProjectPortfolioRole.Owner, Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Project Portfolio is readonly and cannot be updated.");
    }

    [Fact]
    public void RemoveRole_WithOneRoleAssignment_ShouldRemoveEmployeeFromPortfolioRoleSuccessfully()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var portfolio = _portfolioFaker.WithData(roles: new Dictionary<ProjectPortfolioRole, HashSet<Guid>>
        {
            { ProjectPortfolioRole.Owner, new HashSet<Guid> { employeeId } }
        }).Generate();

        // Act
        var result = portfolio.RemoveRole(ProjectPortfolioRole.Owner, employeeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        portfolio.Roles.Should().BeEmpty();
    }

    [Fact]
    public void RemoveRole_WithMultipleRoleAssignments_ShouldRemoveEmployeeFromPortfolioRoleSuccessfully()
    {
        // Arrange
        var employeeId1 = Guid.NewGuid();
        var employeeId2 = Guid.NewGuid();
        var portfolio = _portfolioFaker.WithData(roles: new Dictionary<ProjectPortfolioRole, HashSet<Guid>>
        {
            { ProjectPortfolioRole.Owner, new HashSet<Guid> { employeeId1, employeeId2 } }
        }).Generate();

        // Act
        var result = portfolio.RemoveRole(ProjectPortfolioRole.Owner, employeeId1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        portfolio.Roles.Count.Should().Be(1);
        portfolio.Roles.First().Role.Should().Be(ProjectPortfolioRole.Owner);
        portfolio.Roles.First().EmployeeId.Should().Be(employeeId2);
    }

    [Fact]
    public void RemoveRole_ShouldFail_WhenEmployeeNotAssignedToRole()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var portfolio = _portfolioFaker.Generate();

        // Act
        var result = portfolio.RemoveRole(ProjectPortfolioRole.Owner, employeeId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Employee is not assigned to this role.");
    }

    [Fact]
    public void RemoveRole_ShouldFail_WhenPortfolioIsReadonly()
    {
        // Arrange
        var portfolio = _portfolioFaker.ArchivedPortfolio(_dateTimeProvider);

        // Act
        var result = portfolio.RemoveRole(ProjectPortfolioRole.Owner, Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Project Portfolio is readonly and cannot be updated.");
    }

    [Fact]
    public void UpdateRoles_ShouldAssignNewRolesSuccessfully()
    {
        // Arrange
        var portfolio = _portfolioFaker.Generate();
        var employee1 = Guid.NewGuid();
        var employee2 = Guid.NewGuid();
        var updatedRoles = new Dictionary<ProjectPortfolioRole, HashSet<Guid>>
        {
            { ProjectPortfolioRole.Manager, new HashSet<Guid> { employee1, employee2 } }
        };

        // Act
        var result = portfolio.UpdateRoles(updatedRoles);

        // Assert
        result.IsSuccess.Should().BeTrue();
        portfolio.Roles.Should().Contain(role => role.Role == ProjectPortfolioRole.Manager && role.EmployeeId == employee1);
        portfolio.Roles.Should().Contain(role => role.Role == ProjectPortfolioRole.Manager && role.EmployeeId == employee2);
    }

    [Fact]
    public void UpdateRoles_ShouldRemoveUnspecifiedRoles()
    {
        // Arrange
        var portfolio = _portfolioFaker.WithData(roles: new Dictionary<ProjectPortfolioRole, HashSet<Guid>>
        {
            { ProjectPortfolioRole.Manager, new HashSet<Guid> { Guid.NewGuid(), Guid.NewGuid() } },
            { ProjectPortfolioRole.Owner, new HashSet<Guid> { Guid.NewGuid() } }
        }).Generate();

        var updatedRoles = new Dictionary<ProjectPortfolioRole, HashSet<Guid>>
        {
            { ProjectPortfolioRole.Manager, new HashSet<Guid> { Guid.NewGuid() } }  // Remove Owner role
        };

        // Act
        var result = portfolio.UpdateRoles(updatedRoles);

        // Assert
        result.IsSuccess.Should().BeTrue();
        portfolio.Roles.Should().Contain(role => role.Role == ProjectPortfolioRole.Manager);
        portfolio.Roles.Should().NotContain(role => role.Role == ProjectPortfolioRole.Owner); // Removed role
    }

    [Fact]
    public void UpdateRoles_ShouldNotChange_WhenRolesAreUnchanged()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var portfolio = _portfolioFaker.WithData(roles: new Dictionary<ProjectPortfolioRole, HashSet<Guid>>
        {
            { ProjectPortfolioRole.Sponsor, new HashSet<Guid> { employeeId } }
        }).Generate();

        var updatedRoles = new Dictionary<ProjectPortfolioRole, HashSet<Guid>>
        {
            { ProjectPortfolioRole.Sponsor, new HashSet<Guid> { employeeId } }
        };

        // Act
        var result = portfolio.UpdateRoles(updatedRoles);

        // Assert
        result.IsSuccess.Should().BeTrue();
        portfolio.Roles.Count.Should().Be(1);
        portfolio.Roles.Should().Contain(role => role.Role == ProjectPortfolioRole.Sponsor && role.EmployeeId == employeeId);
    }

    [Fact]
    public void UpdateRoles_ShouldFail_WhenInvalidRoleProvided()
    {
        // Arrange
        var portfolio = _portfolioFaker.Generate();
        var invalidRole = (ProjectPortfolioRole)999;
        var updatedRoles = new Dictionary<ProjectPortfolioRole, HashSet<Guid>>
        {
            { invalidRole, new HashSet<Guid> { Guid.NewGuid() } }
        };

        // Act
        var result = portfolio.UpdateRoles(updatedRoles);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be($"Role is not a valid {nameof(ProjectPortfolioRole)} value.");
    }

    [Fact]
    public void UpdateRoles_ShouldFail_WhenPortfolioIsReadonly()
    {
        // Arrange
        var portfolio = _portfolioFaker.ArchivedPortfolio(_dateTimeProvider);

        var updatedRoles = new Dictionary<ProjectPortfolioRole, HashSet<Guid>>
        {
            { ProjectPortfolioRole.Sponsor, new HashSet<Guid> { Guid.NewGuid() } }
        };

        // Act
        var result = portfolio.UpdateRoles(updatedRoles);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Project Portfolio is readonly and cannot be updated.");
    }

    #endregion Roles

    #region Lifecycle Tests

    [Fact]
    public void Activate_ShouldActivateProposedPortfolioSuccessfully()
    {
        // Arrange
        var portfolio = _portfolioFaker.Generate();
        var startDate = _dateTimeProvider.Today;

        // Act
        var result = portfolio.Activate(startDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        portfolio.Status.Should().Be(ProjectPortfolioStatus.Active);
        portfolio.DateRange.Should().NotBeNull();
        portfolio.DateRange!.Start.Should().Be(startDate);
    }

    [Fact]
    public void Close_ShouldFail_WhenPortfolioHasOpenProjectsOrPrograms()
    {
        // Arrange
        var portfolio = _portfolioFaker.ActivePortfolio(_dateTimeProvider);
        var project = _projectFaker.ActiveProject(_dateTimeProvider, portfolio.Id);
        portfolio.CreateProject(project.Name, project.Description, 1, null);

        var endDate = _dateTimeProvider.Today.PlusDays(10);

        // Act
        var result = portfolio.Close(endDate);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("All projects must be completed or canceled before the portfolio can be closed.");
    }

    [Fact]
    public void Close_ShouldClosePortfolioSuccessfully()
    {
        // Arrange
        var portfolio = _portfolioFaker.ActivePortfolio(_dateTimeProvider);

        var fakeProject = _projectFaker.ProposedProject(portfolio.Id);
        var projectDateRange = new LocalDateRange(_dateTimeProvider.Today, _dateTimeProvider.Today.PlusMonths(3));
        var createProjectReult = portfolio.CreateProject(fakeProject.Name, fakeProject.Description, 1, projectDateRange);
        var project = createProjectReult.Value;

        var endDate = _dateTimeProvider.Today.PlusDays(10);

        var activateProjectResult = project.Activate();
        activateProjectResult.IsSuccess.Should().BeTrue();
        var completeProjectResult = project.Complete();
        completeProjectResult.IsSuccess.Should().BeTrue();

        // Act
        var result = portfolio.Close(endDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        portfolio.Status.Should().Be(ProjectPortfolioStatus.Closed);
        portfolio.DateRange.Should().NotBeNull();
        portfolio.DateRange!.End.Should().Be(endDate);
    }

    [Fact]
    public void Archive_ShouldFail_WhenPortfolioIsNotClosed()
    {
        // Arrange
        var portfolio = _portfolioFaker.ActivePortfolio(_dateTimeProvider);

        // Act
        var result = portfolio.Archive();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Only closed portfolios can be archived.");
    }

    [Fact]
    public void Archive_ShouldArchiveCompletedPortfolioSuccessfully()
    {
        // Arrange
        var portfolio = _portfolioFaker.ClosedPortfolio(_dateTimeProvider);

        // Act
        var result = portfolio.Archive();

        // Assert
        result.IsSuccess.Should().BeTrue();
        portfolio.Status.Should().Be(ProjectPortfolioStatus.Archived);
    }

    #endregion Lifecycle Tests

    #region Program Management

    [Fact]
    public void CreateProgram_ShouldFail_WhenPortfolioIsNotActiveOrOnHold()
    {
        // Arrange
        var portfolio = _portfolioFaker.Generate();

        // Act
        var result = portfolio.CreateProgram("Test Program", "Test Description", null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Programs can only be created in active or on-hold portfolios.");
    }

    [Fact]
    public void CreateProgram_ShouldAddProgramToPortfolio()
    {
        // Arrange
        var portfolio = _portfolioFaker.ActivePortfolio(_dateTimeProvider);

        // Act
        var result = portfolio.CreateProgram("Test Program", "Test Description", null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        portfolio.Programs.Should().ContainSingle();
        portfolio.Programs.First().Name.Should().Be("Test Program");
    }

    [Fact]
    public void Close_ShouldFail_WhenPortfolioHasProgramsWithOpenProjects()
    {
        // Arrange
        var portfolio = _portfolioFaker.ActivePortfolio(_dateTimeProvider);
        var program = portfolio.CreateProgram("Test Program", "Description", null).Value;
        var project = _projectFaker.ActiveProject(_dateTimeProvider, portfolio.Id);

        program.AddProject(project);

        var endDate = _dateTimeProvider.Today.PlusDays(10);

        // Act
        var result = portfolio.Close(endDate);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("All programs must be completed or canceled before the portfolio can be closed.");
    }


    #endregion Program Management

    #region Project Management

    [Fact]
    public void CreateProject_ShouldAddProjectToPortfolio()
    {
        // Arrange
        var portfolio = _portfolioFaker.ActivePortfolio(_dateTimeProvider);

        // Act
        var result = portfolio.CreateProject("Test Project", "Test Description", 1, null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        portfolio.Projects.Should().ContainSingle();
        portfolio.Projects.First().Name.Should().Be("Test Project");
    }

    [Fact]
    public void CreateProject_ShouldFail_WhenProgramDoesNotBelongToPortfolio()
    {
        // Arrange
        var portfolio = _portfolioFaker.ActivePortfolio(_dateTimeProvider);
        var program = _programFaker.Generate();

        // Act
        var result = portfolio.CreateProject("Test Project", "Test Description", 1, null, program.Id);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("The specified program does not belong to this portfolio.");
    }

    [Fact]
    public void CreateProject_ShouldFail_WhenProgramIsNotAcceptingProjects()
    {
        // Arrange
        var portfolio = _portfolioFaker.ActivePortfolio(_dateTimeProvider);
        var fakeProgram = _programFaker.Generate();

        var createProgramResult = portfolio.CreateProgram(fakeProgram.Name, fakeProgram.Description, null);
        createProgramResult.IsSuccess.Should().BeTrue();
        var program = createProgramResult.Value;

        // Act
        var result = portfolio.CreateProject("Test Project", "Test Description", 1, null, program.Id);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("The specified program is not in a valid state to accept projects.");
    }

    [Fact]
    public void ChangeProjectProgram_ShouldMoveProjectToNewProgram()
    {
        // Arrange
        var portfolio = _portfolioFaker.PortfolioWithProgramsAndProjects(_dateTimeProvider, 2, 0);

        var program1 = portfolio.Programs.First();
        var program2 = portfolio.Programs.Last();

        var project = portfolio.CreateProject("Test Project", "Description", 1, null, program1.Id);
        project.IsSuccess.Should().BeTrue();

        // Act
        var result = portfolio.ChangeProjectProgram(project.Value.Id, program2.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.Value.ProgramId.Should().Be(program2.Id);
        program1.Projects.Should().NotContain(project.Value);
        program2.Projects.Should().Contain(project.Value);
    }

    [Fact]
    public void ChangeProjectProgram_ShouldRemoveProjectFromProgram()
    {
        // Arrange
        var portfolio = _portfolioFaker.PortfolioWithProgramsAndProjects(_dateTimeProvider, 1, 0);

        var program = portfolio.Programs.First();

        var project = portfolio.CreateProject("Test Project", "Description", 1, null, program.Id);
        project.IsSuccess.Should().BeTrue();

        // Act
        var result = portfolio.ChangeProjectProgram(project.Value.Id, null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.Value.ProgramId.Should().BeNull();
        program.Projects.Should().NotContain(project.Value);
    }

    [Fact]
    public void ChangeProjectProgram_ShouldFail_WhenProjectAlreadyInProgram()
    {
        // Arrange
        var portfolio = _portfolioFaker.PortfolioWithProgramsAndProjects(_dateTimeProvider, 1, 0);

        var program = portfolio.Programs.First();

        var project = portfolio.CreateProject("Test Project", "Description", 1, null, program.Id);
        project.IsSuccess.Should().BeTrue();

        // Act
        var result = portfolio.ChangeProjectProgram(project.Value.Id, program.Id);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("The project is already associated with the specified program.");
    }

    [Fact]
    public void ChangeProjectProgram_ShouldFail_WhenProgramNotInPortfolio()
    {
        // Arrange
        var portfolio1 = _portfolioFaker.PortfolioWithProgramsAndProjects(_dateTimeProvider, 1, 0);

        var program1 = portfolio1.Programs.First();

        var program2 = _programFaker.ActiveProgram(_dateTimeProvider, Guid.NewGuid());

        var projectResult = portfolio1.CreateProject("Test Project", "Description", 1, null, program1.Id);
        projectResult.IsSuccess.Should().BeTrue();

        // Act
        var result = portfolio1.ChangeProjectProgram(projectResult.Value.Id, program2.Id);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("The specified program does not belong to this portfolio.");
    }

    [Fact]
    public void ChangeProjectProgram_ShouldFail_WhenProjectHasNoProgramAndIsRemovedAgain()
    {
        // Arrange
        var portfolio = _portfolioFaker.ActivePortfolio(_dateTimeProvider);
        var project = portfolio.CreateProject("Test Project", "Description", 1, null).Value;

        // Act
        var result = portfolio.ChangeProjectProgram(project.Id, null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("The project is not currently assigned to a program.");
    }


    #endregion Project Management
}
