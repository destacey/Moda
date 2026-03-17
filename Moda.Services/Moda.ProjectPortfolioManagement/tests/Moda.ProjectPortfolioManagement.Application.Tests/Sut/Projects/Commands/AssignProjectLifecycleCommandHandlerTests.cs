using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moda.ProjectPortfolioManagement.Application.Projects.Commands;
using Moda.ProjectPortfolioManagement.Application.Tests.Infrastructure;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.ProjectPortfolioManagement.Domain.Tests.Data;
using Moda.Tests.Shared;
using Moq;
using NodaTime.Extensions;
using NodaTime.Testing;

namespace Moda.ProjectPortfolioManagement.Application.Tests.Sut.Projects.Commands;

public class AssignProjectLifecycleCommandHandlerTests : IDisposable
{
    private readonly FakeProjectPortfolioManagementDbContext _dbContext;
    private readonly AssignProjectLifecycleCommandHandler _handler;
    private readonly Mock<ILogger<AssignProjectLifecycleCommandHandler>> _mockLogger;
    private readonly TestingDateTimeProvider _dateTimeProvider;

    private readonly ProjectFaker _projectFaker;
    private readonly ProjectLifecycleFaker _lifecycleFaker;

    public AssignProjectLifecycleCommandHandlerTests()
    {
        _dbContext = new FakeProjectPortfolioManagementDbContext();
        _mockLogger = new Mock<ILogger<AssignProjectLifecycleCommandHandler>>();
        _dateTimeProvider = new TestingDateTimeProvider(new FakeClock(DateTime.UtcNow.ToInstant()));

        _handler = new AssignProjectLifecycleCommandHandler(_dbContext, _mockLogger.Object);

        _projectFaker = new ProjectFaker();
        _lifecycleFaker = new ProjectLifecycleFaker();
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenProjectDoesNotExist()
    {
        // Arrange
        var command = new AssignProjectLifecycleCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        _dbContext.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenLifecycleDoesNotExist()
    {
        // Arrange
        var project = _projectFaker.AsProposed(_dateTimeProvider);
        _dbContext.AddProject(project);

        var command = new AssignProjectLifecycleCommand(project.Id, Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        _dbContext.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldAssignLifecycle_WhenProjectAndLifecycleExist()
    {
        // Arrange
        var project = _projectFaker.AsProposed(_dateTimeProvider);
        _dbContext.AddProject(project);

        var lifecycle = _lifecycleFaker.AsActiveWithPhases(("Plan", "Planning"), ("Execute", "Execution"), ("Deliver", "Delivery"));
        _dbContext.AddProjectLifecycle(lifecycle);

        var command = new AssignProjectLifecycleCommand(project.Id, lifecycle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.ProjectLifecycleId.Should().Be(lifecycle.Id);
        project.Phases.Count.Should().Be(3);
        _dbContext.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenLifecycleAlreadyAssigned()
    {
        // Arrange
        var project = _projectFaker.AsProposed(_dateTimeProvider);
        var lifecycle = _lifecycleFaker.AsActiveWithPhases(("Plan", "Planning"), ("Execute", "Execution"));
        project.AssignLifecycle(lifecycle);
        _dbContext.AddProject(project);

        var secondLifecycle = _lifecycleFaker.AsActiveWithPhases(("Design", "Design Phase"), ("Build", "Build Phase"));
        _dbContext.AddProjectLifecycle(secondLifecycle);

        var command = new AssignProjectLifecycleCommand(project.Id, secondLifecycle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("already assigned");
        _dbContext.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenLifecycleIsNotActive()
    {
        // Arrange
        var project = _projectFaker.AsProposed(_dateTimeProvider);
        _dbContext.AddProject(project);

        var lifecycle = _lifecycleFaker.AsProposedWithPhases(("Plan", "Planning"), ("Execute", "Execution"));
        _dbContext.AddProjectLifecycle(lifecycle);

        var command = new AssignProjectLifecycleCommand(project.Id, lifecycle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("active");
        _dbContext.SaveChangesCallCount.Should().Be(0);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
