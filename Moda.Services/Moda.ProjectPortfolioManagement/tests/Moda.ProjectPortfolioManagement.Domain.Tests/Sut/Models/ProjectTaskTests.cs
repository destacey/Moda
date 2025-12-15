using FluentAssertions;
using Moda.Common.Domain.Models.ProjectPortfolioManagement;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.ProjectPortfolioManagement.Domain.Models;
using Moda.Tests.Shared;
using NodaTime.Extensions;
using NodaTime.Testing;

namespace Moda.ProjectPortfolioManagement.Domain.Tests.Sut.Models;

public class ProjectTaskTests
{
    private readonly TestingDateTimeProvider _dateTimeProvider;

    public ProjectTaskTests()
    {
        _dateTimeProvider = new TestingDateTimeProvider(new FakeClock(DateTime.UtcNow.ToInstant()));
    }

    #region SetOrder Tests

    [Fact]
    public void SetOrder_ShouldSucceed_WhenOrderIsValid()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var projectKey = new ProjectKey("TEST");
        var task = ProjectTask.Create(
            projectId: projectId,
            projectKey: projectKey,
            taskNumber: 1,
            name: "Test Task",
            description: "Description",
            type: ProjectTaskType.Task,
            priority: null,
            order: 1,
            parentId: null,
            teamId: null,
            plannedDateRange: null,
            plannedDate: null,
            estimatedEffortHours: null,
            assignments: null);

        // Act
        var result = task.SetOrder(5);

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.Order.Should().Be(5);
    }

    [Fact]
    public void SetOrder_ShouldFail_WhenOrderIsZero()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var projectKey = new ProjectKey("TEST");
        var task = ProjectTask.Create(
            projectId: projectId,
            projectKey: projectKey,
            taskNumber: 1,
            name: "Test Task",
            description: "Description",
            type: ProjectTaskType.Task,
            priority: null,
            order: 1,
            parentId: null,
            teamId: null,
            plannedDateRange: null,
            plannedDate: null,
            estimatedEffortHours: null,
            assignments: null);

        // Act
        var result = task.SetOrder(0);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Order must be greater than 0.");
        task.Order.Should().Be(1); // Should remain unchanged
    }

    [Fact]
    public void SetOrder_ShouldFail_WhenOrderIsNegative()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var projectKey = new ProjectKey("TEST");
        var task = ProjectTask.Create(
            projectId: projectId,
            projectKey: projectKey,
            taskNumber: 1,
            name: "Test Task",
            description: "Description",
            type: ProjectTaskType.Task,
            priority: null,
            order: 1,
            parentId: null,
            teamId: null,
            plannedDateRange: null,
            plannedDate: null,
            estimatedEffortHours: null,
            assignments: null);

        // Act
        var result = task.SetOrder(-5);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Order must be greater than 0.");
        task.Order.Should().Be(1); // Should remain unchanged
    }

    [Fact]
    public void SetOrder_ShouldAllowUpdatingMultipleTimes()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var projectKey = new ProjectKey("TEST");
        var task = ProjectTask.Create(
            projectId: projectId,
            projectKey: projectKey,
            taskNumber: 1,
            name: "Test Task",
            description: "Description",
            type: ProjectTaskType.Task,
            priority: null,
            order: 1,
            parentId: null,
            teamId: null,
            plannedDateRange: null,
            plannedDate: null,
            estimatedEffortHours: null,
            assignments: null);

        // Act
        var result1 = task.SetOrder(5);
        var result2 = task.SetOrder(10);
        var result3 = task.SetOrder(3);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result3.IsSuccess.Should().BeTrue();
        task.Order.Should().Be(3); // Should have the last value
    }

    #endregion SetOrder Tests
}
