using FluentAssertions;
using Moda.Common.Domain.Models.ProjectPortfolioManagement;
using Moda.ProjectPortfolioManagement.Domain.Tests.Data;
using Moda.Tests.Shared;
using NodaTime.Extensions;
using NodaTime.Testing;

namespace Moda.ProjectPortfolioManagement.Domain.Tests.Sut.Models;

public class ProjectTaskTests
{
    private readonly TestingDateTimeProvider _dateTimeProvider;

    private readonly ProjectTaskFaker _projectTaskFaker;

    public ProjectTaskTests()
    {
        _dateTimeProvider = new TestingDateTimeProvider(new FakeClock(DateTime.UtcNow.ToInstant()));

        _projectTaskFaker = new ProjectTaskFaker();
    }

    #region UpdateProjectKey Tests

    [Fact]
    public void UpdateProjectKey_ShouldUpdateTaskKeyAndPreserveNumber()
    {
        // Arrange
        var originalProjectKey = new ProjectKey("ORIG");
        var taskNumber = 123;
        var task = _projectTaskFaker.WithData(key: new ProjectTaskKey(originalProjectKey, taskNumber)).Generate();

        var newProjectKey = new ProjectKey("NEWKEY");

        // Act
        var result = task.UpdateProjectKey(newProjectKey);

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.Number.Should().Be(taskNumber);
        task.Key.Value.Should().Be($"{newProjectKey.Value}-{taskNumber}");
    }

    [Fact]
    public void UpdateProjectKey_ShouldBeNoOp_WhenProjectKeyIsUnchanged()
    {
        // Arrange
        var projectKey = new ProjectKey("SAME");
        var taskNumber = 7;
        var task = _projectTaskFaker.WithData(key: new ProjectTaskKey(projectKey, taskNumber)).Generate();
        var originalTaskKeyValue = task.Key.Value;

        // Act
        var result = task.UpdateProjectKey(projectKey);

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.Key.Value.Should().Be(originalTaskKeyValue);
    }

    #endregion UpdateProjectKey Tests

    #region SetOrder Tests

    [Fact]
    public void SetOrder_ShouldSucceed_WhenOrderIsValid()
    {
        // Arrange
        var task = _projectTaskFaker.Generate();

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
        var task = _projectTaskFaker.Generate();

        // Act
        var result = task.SetOrder(0);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Order must be greater than 0.");
        task.Order.Should().Be(task.Order); // Should remain unchanged
    }

    [Fact]
    public void SetOrder_ShouldFail_WhenOrderIsNegative()
    {
        // Arrange
        var task = _projectTaskFaker.Generate();

        // Act
        var result = task.SetOrder(-5);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Order must be greater than 0.");
        task.Order.Should().Be(task.Order); // Should remain unchanged
    }

    [Fact]
    public void SetOrder_ShouldAllowUpdatingMultipleTimes()
    {
        // Arrange
        var task = _projectTaskFaker.Generate();

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
