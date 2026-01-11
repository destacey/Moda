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

    public ProjectTaskTests()
    {
        _dateTimeProvider = new TestingDateTimeProvider(new FakeClock(DateTime.UtcNow.ToInstant()));
    }

    #region UpdateProjectKey Tests

    [Fact]
    public void UpdateProjectKey_ShouldUpdateTaskKeyAndPreserveNumber()
    {
        // Arrange
        var originalProjectKey = new ProjectKey("ORIG");
        var taskNumber = 123;
        var task = new ProjectTaskFaker().WithData(key: new ProjectTaskKey(originalProjectKey, taskNumber)).Generate();

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
        var task = new ProjectTaskFaker().WithData(key: new ProjectTaskKey(projectKey, taskNumber)).Generate();
        var originalTaskKeyValue = task.Key.Value;

        // Act
        var result = task.UpdateProjectKey(projectKey);

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.Key.Value.Should().Be(originalTaskKeyValue);
    }

    #endregion UpdateProjectKey Tests

    #region ChangeOrder Tests

    [Fact]
    public void ChangeOrder_ShouldSucceed_WhenOrderIsValid()
    {
        // Arrange
        var task = new ProjectTaskFaker().Generate();

        // Act
        var result = task.ChangeOrder(5);

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.Order.Should().Be(5);
    }

    [Fact]
    public void ChangeOrder_ShouldFail_WhenOrderIsZero()
    {
        // Arrange
        var task = new ProjectTaskFaker().Generate();

        // Act
        var result = task.ChangeOrder(0);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Order must be greater than 0.");
    }

    [Fact]
    public void ChangeOrder_ShouldFail_WhenOrderIsNegative()
    {
        // Arrange
        var task = new ProjectTaskFaker().Generate();

        // Act
        var result = task.ChangeOrder(-5);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Order must be greater than 0.");
    }

    [Fact]
    public void ChangeOrder_ShouldAllowUpdatingMultipleTimes()
    {
        // Arrange
        var task = new ProjectTaskFaker().Generate();

        // Act
        var result1 = task.ChangeOrder(5);
        var result2 = task.ChangeOrder(10);
        var result3 = task.ChangeOrder(3);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result3.IsSuccess.Should().BeTrue();
        task.Order.Should().Be(3);
    }

    #endregion ChangeOrder Tests

    #region ChangeParent Tests

    [Fact]
    public void ChangeParent_ShouldSucceed_WhenSettingNewParent()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var parentTaskId = Guid.NewGuid();
        var childTaskId = Guid.NewGuid();

        var parentTask = new ProjectTaskFaker().WithData(id: parentTaskId, projectId: projectId).Generate();
        var childTask = new ProjectTaskFaker().WithData(id: childTaskId, projectId: projectId).Generate();

        // Act
        var result = childTask.ChangeParent(parentTask.Id, 1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        childTask.ParentId.Should().Be(parentTask.Id);
        childTask.Order.Should().Be(1);
    }

    [Fact]
    public void ChangeParent_ShouldSucceed_WhenRemovingParent()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var childTask = new ProjectTaskFaker().WithData(projectId: projectId, parentId: parentId).Generate();

        // Act
        var result = childTask.ChangeParent(null, 1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        childTask.ParentId.Should().BeNull();
        childTask.Order.Should().Be(1);
    }

    [Fact]
    public void ChangeParent_ShouldSucceed_WhenChangingToAnotherParent()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var originalParentId = Guid.NewGuid();
        var newParentId = Guid.NewGuid();
        var childTask = new ProjectTaskFaker().WithData(projectId: projectId, parentId: originalParentId).Generate();

        // Act
        var result = childTask.ChangeParent(newParentId, 3);

        // Assert
        result.IsSuccess.Should().BeTrue();
        childTask.ParentId.Should().Be(newParentId);
        childTask.Order.Should().Be(3);
    }

    [Fact]
    public void ChangeParent_ShouldFail_WhenSettingSelfAsParent()
    {
        // Arrange
        var task = new ProjectTaskFaker().Generate();

        // Act
        var result = task.ChangeParent(task.Id, 1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("A task cannot be its own parent.");
    }

    [Fact]
    public void ChangeParent_ShouldFail_WhenNewParentIsDirectChild()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var parentTaskId = Guid.NewGuid();
        var childTaskId = Guid.NewGuid();

        var parentTask = new ProjectTaskFaker().WithData(id: parentTaskId, projectId: projectId).Generate();
        var childTask = new ProjectTaskFaker().WithData(id: childTaskId, projectId: projectId, parentId: parentTaskId).Generate();

        parentTask.AddChild(childTask);

        // Act
        var result = parentTask.ChangeParent(childTask.Id, 1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("A task cannot be moved under one of its descendants.");
    }

    [Fact]
    public void ChangeParent_ShouldFail_WhenNewParentIsGrandchild()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var grandparentId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var grandparentTask = new ProjectTaskFaker().WithData(id: grandparentId, projectId: projectId).Generate();
        var parentTask = new ProjectTaskFaker().WithData(id: parentId, projectId: projectId, parentId: grandparentId).Generate();
        var childTask = new ProjectTaskFaker().WithData(id: childId, projectId: projectId, parentId: parentId).Generate();

        grandparentTask.AddChild(parentTask);
        parentTask.AddChild(childTask);

        // Act
        var result = grandparentTask.ChangeParent(childTask.Id, 1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("A task cannot be moved under one of its descendants.");
    }

    [Fact]
    public void ChangeParent_ShouldFail_WhenNewParentIsDeepDescendant()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var rootId = Guid.NewGuid();
        var level1Id = Guid.NewGuid();
        var level2Id = Guid.NewGuid();
        var level3Id = Guid.NewGuid();

        var rootTask = new ProjectTaskFaker().WithData(id: rootId, projectId: projectId).Generate();
        var level1Task = new ProjectTaskFaker().WithData(id: level1Id, projectId: projectId, parentId: rootId).Generate();
        var level2Task = new ProjectTaskFaker().WithData(id: level2Id, projectId: projectId, parentId: level1Id).Generate();
        var level3Task = new ProjectTaskFaker().WithData(id: level3Id, projectId: projectId, parentId: level2Id).Generate();

        rootTask.AddChild(level1Task);
        level1Task.AddChild(level2Task);
        level2Task.AddChild(level3Task);

        // Act
        var result = rootTask.ChangeParent(level3Task.Id, 1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("A task cannot be moved under one of its descendants.");
    }

    [Fact]
    public void ChangeParent_ShouldSucceed_WhenMovingToSibling()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var sibling1Id = Guid.NewGuid();
        var sibling2Id = Guid.NewGuid();

        var parentTask = new ProjectTaskFaker().WithData(id: parentId, projectId: projectId).Generate();
        var sibling1 = new ProjectTaskFaker().WithData(id: sibling1Id, projectId: projectId, parentId: parentId, order: 1).Generate();
        var sibling2 = new ProjectTaskFaker().WithData(id: sibling2Id, projectId: projectId, parentId: parentId, order: 2).Generate();

        parentTask.AddChild(sibling1);
        parentTask.AddChild(sibling2);

        // Act
        var result = sibling1.ChangeParent(sibling2.Id, 1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        sibling1.ParentId.Should().Be(sibling2.Id);
        sibling1.Order.Should().Be(1);
    }

    [Fact]
    public void ChangeParent_ShouldSucceed_WhenMovingToUncle()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var grandparentId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var uncleId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var grandparent = new ProjectTaskFaker().WithData(id: grandparentId, projectId: projectId).Generate();
        var parent = new ProjectTaskFaker().WithData(id: parentId, projectId: projectId, parentId: grandparentId).Generate();
        var uncle = new ProjectTaskFaker().WithData(id: uncleId, projectId: projectId, parentId: grandparentId).Generate();
        var child = new ProjectTaskFaker().WithData(id: childId, projectId: projectId, parentId: parentId).Generate();

        grandparent.AddChild(parent);
        grandparent.AddChild(uncle);
        parent.AddChild(child);

        // Act
        var result = child.ChangeParent(uncle.Id, 1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        child.ParentId.Should().Be(uncle.Id);
    }

    [Fact]
    public void ChangeParent_ShouldFail_WhenOrderIsZero()
    {
        // Arrange
        var task = new ProjectTaskFaker().Generate();
        var newParentId = Guid.NewGuid();

        // Act
        var result = task.ChangeParent(newParentId, 0);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Order must be greater than 0.");
    }

    [Fact]
    public void ChangeParent_ShouldFail_WhenOrderIsNegative()
    {
        // Arrange
        var task = new ProjectTaskFaker().Generate();
        var newParentId = Guid.NewGuid();

        // Act
        var result = task.ChangeParent(newParentId, -5);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Order must be greater than 0.");
    }

    [Fact]
    public void ChangeParent_ShouldUpdateOrder_WhenChangingParent()
    {
        // Arrange
        var task = new ProjectTaskFaker().WithData(order: 5).Generate();
        var newParentId = Guid.NewGuid();

        // Act
        var result = task.ChangeParent(newParentId, 10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.ParentId.Should().Be(newParentId);
        task.Order.Should().Be(10);
    }

    [Fact]
    public void ChangeParent_ShouldUpdateOrder_WhenRemovingParent()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var task = new ProjectTaskFaker().WithData(parentId: parentId, order: 3).Generate();

        // Act
        var result = task.ChangeParent(null, 7);

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.ParentId.Should().BeNull();
        task.Order.Should().Be(7);
    }

    [Fact]
    public void ChangeParent_ShouldSetParentId_EvenWhenOrderValidationFails()
    {
        // Arrange
        var originalParentId = Guid.NewGuid();
        var newParentId = Guid.NewGuid();
        var task = new ProjectTaskFaker().WithData(parentId: originalParentId, order: 5).Generate();

        // Act
        var result = task.ChangeParent(newParentId, 0);

        // Assert
        result.IsFailure.Should().BeTrue();
        // Note: In current implementation, ParentId is set before order validation
        // This test documents current behavior - ParentId IS changed even when order fails
        task.ParentId.Should().Be(newParentId);
    }

    #endregion ChangeParent Tests
}
