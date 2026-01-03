using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moda.ProjectPortfolioManagement.Application.ProjectTasks.Commands;
using Moda.ProjectPortfolioManagement.Application.Tests.Infrastructure;
using Moda.ProjectPortfolioManagement.Domain.Tests.Data;
using Moda.Tests.Shared;
using Moq;
using NodaTime.Extensions;
using NodaTime.Testing;

namespace Moda.ProjectPortfolioManagement.Application.Tests.Sut.ProjectTasks.Commands;

public class UpdateProjectTaskOrderCommandHandlerTests : IDisposable
{
    private readonly FakeProjectPortfolioManagementDbContext _dbContext;
    private readonly UpdateProjectTaskOrderCommandHandler _handler;
    private readonly TestingDateTimeProvider _dateTimeProvider;
    private readonly Mock<ILogger<UpdateProjectTaskOrderCommandHandler>> _mockLogger;
    private readonly ProjectTaskFaker _taskFaker;

    public UpdateProjectTaskOrderCommandHandlerTests()
    {
        _dbContext = new FakeProjectPortfolioManagementDbContext();
        _dateTimeProvider = new TestingDateTimeProvider(new FakeClock(DateTime.UtcNow.ToInstant()));
        _mockLogger = new Mock<ILogger<UpdateProjectTaskOrderCommandHandler>>();

        _handler = new UpdateProjectTaskOrderCommandHandler(
            _dbContext,
            _mockLogger.Object);

        _taskFaker = new ProjectTaskFaker();
    }

    [Fact]
    public async Task Handle_ShouldUpdateTaskOrder_WhenTaskExists()
    {
        // Arrange
        var task = _taskFaker.WithData(order: 1).Generate();

        _dbContext.AddProjectTask(task);

        var command = new UpdateProjectTaskOrderCommand(task.Id, 5);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.Order.Should().Be(5);
        _dbContext.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenTaskDoesNotExist()
    {
        // Arrange
        var nonExistentTaskId = Guid.NewGuid();
        var command = new UpdateProjectTaskOrderCommand(nonExistentTaskId, 5);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        _dbContext.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenOrderIsZero()
    {
        // Arrange
        var task = _taskFaker.WithData(order: 1).Generate();

        _dbContext.AddProjectTask(task);

        var command = new UpdateProjectTaskOrderCommand(task.Id, 0);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Order must be greater than 0.");
        task.Order.Should().Be(1); // Should remain unchanged
        _dbContext.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenOrderIsNegative()
    {
        // Arrange
        var task = _taskFaker.WithData(order: 1).Generate();

        _dbContext.AddProjectTask(task);

        var command = new UpdateProjectTaskOrderCommand(task.Id, -1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Order must be greater than 0.");
        task.Order.Should().Be(1); // Should remain unchanged
        _dbContext.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldAllowReorderingMultipleTasks()
    {
        // Arrange
        var task1 = _taskFaker.WithData(name: "Task 1", order: 1).Generate();
        var task2 = _taskFaker.WithData(name: "Task 2", order: 2).Generate();
        var task3 = _taskFaker.WithData(name: "Task 3", order: 3).Generate();

        _dbContext.AddProjectTasks([task1, task2, task3]);

        // Act - Reorder task3 to position 1
        var result1 = await _handler.Handle(new UpdateProjectTaskOrderCommand(task3.Id, 1), CancellationToken.None);
        // Reorder task1 to position 2
        var result2 = await _handler.Handle(new UpdateProjectTaskOrderCommand(task1.Id, 2), CancellationToken.None);
        // Reorder task2 to position 3
        var result3 = await _handler.Handle(new UpdateProjectTaskOrderCommand(task2.Id, 3), CancellationToken.None);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result3.IsSuccess.Should().BeTrue();

        task3.Order.Should().Be(1);
        task1.Order.Should().Be(2);
        task2.Order.Should().Be(3);

        _dbContext.SaveChangesCallCount.Should().Be(3);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
