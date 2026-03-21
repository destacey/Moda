using FluentAssertions;
using Moda.Common.Application.Interfaces;
using Moda.Common.Domain.Tests.Data;
using Moda.ProjectPortfolioManagement.Application.Projects.Queries;
using Moda.ProjectPortfolioManagement.Application.Tests.Infrastructure;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.ProjectPortfolioManagement.Domain.Models;
using Moda.ProjectPortfolioManagement.Domain.Tests.Data;
using Moda.Tests.Shared.Extensions;
using Moq;
using TaskStatus = Moda.ProjectPortfolioManagement.Domain.Enums.TaskStatus;

namespace Moda.ProjectPortfolioManagement.Application.Tests.Sut.Projects.Queries;

public class GetMyProjectsSummaryQueryHandlerTests : IDisposable
{
    private readonly FakeProjectPortfolioManagementDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly GetMyProjectsSummaryQueryHandler _handler;
    private readonly ProjectFaker _projectFaker;
    private readonly EmployeeFaker _employeeFaker;
    private readonly Guid _employeeId = Guid.NewGuid();

    public GetMyProjectsSummaryQueryHandlerTests()
    {
        _dbContext = new FakeProjectPortfolioManagementDbContext();
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(u => u.GetEmployeeId()).Returns(_employeeId);
        _handler = new GetMyProjectsSummaryQueryHandler(_dbContext, _currentUserMock.Object);
        _projectFaker = new ProjectFaker();
        _employeeFaker = new EmployeeFaker();
    }

    private static HashSet<RoleAssignment<TaskRole>> CreateTaskAssigneeRoles(Guid taskId, Guid employeeId)
    {
        var employee = new EmployeeFaker().Generate();
        employee.SetPrivateField("_id", employeeId);
        var role = new RoleAssignmentFaker<TaskRole>()
            .WithObjectId(taskId)
            .WithRole(TaskRole.Assignee)
            .WithEmployeeId(employeeId)
            .Generate();
        role.Employee = employee;
        return [role];
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptySummary_WhenNoEmployeeId()
    {
        // Arrange
        _currentUserMock.Setup(u => u.GetEmployeeId()).Returns((Guid?)null);
        var query = new GetMyProjectsSummaryQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldReturnZeroCounts_WhenNoProjects()
    {
        // Arrange
        var query = new GetMyProjectsSummaryQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.TotalCount.Should().Be(0);
        result.SponsorCount.Should().Be(0);
        result.OwnerCount.Should().Be(0);
        result.ManagerCount.Should().Be(0);
        result.MemberCount.Should().Be(0);
        result.AssigneeCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldCountSponsorRole()
    {
        // Arrange
        var project = _projectFaker.WithData(
            roles: new Dictionary<ProjectRole, HashSet<Guid>>
            {
                { ProjectRole.Sponsor, [_employeeId] },
            }
        ).Generate();
        _dbContext.AddProject(project);

        var query = new GetMyProjectsSummaryQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result!.SponsorCount.Should().Be(1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldCountOwnerRole()
    {
        // Arrange
        var project = _projectFaker.WithData(
            roles: new Dictionary<ProjectRole, HashSet<Guid>>
            {
                { ProjectRole.Owner, [_employeeId] },
            }
        ).Generate();
        _dbContext.AddProject(project);

        var query = new GetMyProjectsSummaryQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result!.OwnerCount.Should().Be(1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldCountManagerRole()
    {
        // Arrange
        var project = _projectFaker.WithData(
            roles: new Dictionary<ProjectRole, HashSet<Guid>>
            {
                { ProjectRole.Manager, [_employeeId] },
            }
        ).Generate();
        _dbContext.AddProject(project);

        var query = new GetMyProjectsSummaryQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result!.ManagerCount.Should().Be(1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldCountMemberRole()
    {
        // Arrange
        var project = _projectFaker.WithData(
            roles: new Dictionary<ProjectRole, HashSet<Guid>>
            {
                { ProjectRole.Member, [_employeeId] },
            }
        ).Generate();
        _dbContext.AddProject(project);

        var query = new GetMyProjectsSummaryQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result!.MemberCount.Should().Be(1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldCountTaskAssignee()
    {
        // Arrange
        var project = _projectFaker.Generate();

        var task = new ProjectTaskFaker().WithData(
            projectId: project.Id,
            status: TaskStatus.InProgress
        ).Generate();
        task.SetPrivateField("_roles", CreateTaskAssigneeRoles(task.Id, _employeeId));
        project.SetPrivateField("_tasks", new List<ProjectTask> { task });

        _dbContext.AddProject(project);

        var query = new GetMyProjectsSummaryQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result!.AssigneeCount.Should().Be(1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldCountMultipleRolesOnSameProject()
    {
        // Arrange — user is both Sponsor and Manager on same project
        var project = _projectFaker.WithData(
            roles: new Dictionary<ProjectRole, HashSet<Guid>>
            {
                { ProjectRole.Sponsor, [_employeeId] },
                { ProjectRole.Manager, [_employeeId] },
            }
        ).Generate();
        _dbContext.AddProject(project);

        var query = new GetMyProjectsSummaryQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result!.SponsorCount.Should().Be(1);
        result.ManagerCount.Should().Be(1);
        result.TotalCount.Should().Be(1); // same project counted once
    }

    [Fact]
    public async Task Handle_ShouldCountAcrossMultipleProjects()
    {
        // Arrange
        var project1 = _projectFaker.WithData(
            roles: new Dictionary<ProjectRole, HashSet<Guid>>
            {
                { ProjectRole.Sponsor, [_employeeId] },
            }
        ).Generate();

        var project2 = _projectFaker.WithData(
            roles: new Dictionary<ProjectRole, HashSet<Guid>>
            {
                { ProjectRole.Manager, [_employeeId] },
            }
        ).Generate();

        _dbContext.AddProjects([project1, project2]);

        var query = new GetMyProjectsSummaryQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result!.SponsorCount.Should().Be(1);
        result.ManagerCount.Should().Be(1);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldNotCountOtherUsersProjects()
    {
        // Arrange
        var otherId = Guid.NewGuid();
        var project = _projectFaker.WithData(
            roles: new Dictionary<ProjectRole, HashSet<Guid>>
            {
                { ProjectRole.Sponsor, [otherId] },
            }
        ).Generate();
        _dbContext.AddProject(project);

        var query = new GetMyProjectsSummaryQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result!.TotalCount.Should().Be(0);
        result.SponsorCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldFilterByStatus()
    {
        // Arrange
        var activeProject = _projectFaker.WithData(
            status: ProjectStatus.Active,
            roles: new Dictionary<ProjectRole, HashSet<Guid>>
            {
                { ProjectRole.Owner, [_employeeId] },
            }
        ).Generate();

        var proposedProject = _projectFaker.WithData(
            status: ProjectStatus.Proposed,
            roles: new Dictionary<ProjectRole, HashSet<Guid>>
            {
                { ProjectRole.Owner, [_employeeId] },
            }
        ).Generate();

        _dbContext.AddProjects([activeProject, proposedProject]);

        var query = new GetMyProjectsSummaryQuery(StatusFilter: [ProjectStatus.Active]);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result!.TotalCount.Should().Be(1);
        result.OwnerCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllStatuses_WhenNoStatusFilter()
    {
        // Arrange
        var activeProject = _projectFaker.WithData(
            status: ProjectStatus.Active,
            roles: new Dictionary<ProjectRole, HashSet<Guid>>
            {
                { ProjectRole.Owner, [_employeeId] },
            }
        ).Generate();

        var proposedProject = _projectFaker.WithData(
            status: ProjectStatus.Proposed,
            roles: new Dictionary<ProjectRole, HashSet<Guid>>
            {
                { ProjectRole.Manager, [_employeeId] },
            }
        ).Generate();

        _dbContext.AddProjects([activeProject, proposedProject]);

        var query = new GetMyProjectsSummaryQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result!.TotalCount.Should().Be(2);
        result.OwnerCount.Should().Be(1);
        result.ManagerCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldCountAssigneeWhoAlsoHasProjectRole()
    {
        // Arrange — user is a Manager AND has task assignments on same project
        var project = _projectFaker.WithData(
            roles: new Dictionary<ProjectRole, HashSet<Guid>>
            {
                { ProjectRole.Manager, [_employeeId] },
            }
        ).Generate();

        var task = new ProjectTaskFaker().WithData(
            projectId: project.Id,
            status: TaskStatus.InProgress
        ).Generate();
        task.SetPrivateField("_roles", CreateTaskAssigneeRoles(task.Id, _employeeId));
        project.SetPrivateField("_tasks", new List<ProjectTask> { task });

        _dbContext.AddProject(project);

        var query = new GetMyProjectsSummaryQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result!.ManagerCount.Should().Be(1);
        result.AssigneeCount.Should().Be(1); // counted as assignee too
        result.TotalCount.Should().Be(1); // but total is still 1 project
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
