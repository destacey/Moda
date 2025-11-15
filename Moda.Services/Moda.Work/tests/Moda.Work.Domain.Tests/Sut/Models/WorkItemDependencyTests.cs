using Moda.Common.Domain.Enums.Work;
using Moda.Work.Domain.Models;
using NodaTime;

namespace Moda.Work.Domain.Tests.Sut.Models;

public class WorkItemDependencyTests
{
    private static readonly Instant _now = Instant.FromUtc(2025, 1, 1, 0, 0);

    [Theory]
    [InlineData(WorkStatusCategory.Proposed, DependencyState.ToDo)]
    [InlineData(WorkStatusCategory.Active, DependencyState.InProgress)]
    [InlineData(WorkStatusCategory.Done, DependencyState.Done)]
    [InlineData(WorkStatusCategory.Removed, DependencyState.Removed)]
    public void Create_State_MapsStatusCategoriesCorrectly(WorkStatusCategory sourceStatus, DependencyState expectedState)
    {
        // Arrange / Act
        var dep = WorkItemDependency.Create(Guid.NewGuid(), Guid.NewGuid(), sourceStatus, null, null, _now, null, null, null, null, _now);

        // Assert
        Assert.Equal(expectedState, dep.State);
    }

    [Theory]
    [MemberData(nameof(HealthTestCases))]
    public void Create_Health_VariousCases(WorkStatusCategory sourceStatus, int? sourceOffsetDays, int? targetOffsetDays, DependencyPlanningHealth expected)
    {
        // Arrange
        Instant? sourcePlanned = sourceOffsetDays.HasValue ? _now.Plus(Duration.FromDays(sourceOffsetDays.Value)) : null;
        Instant? targetPlanned = targetOffsetDays.HasValue ? _now.Plus(Duration.FromDays(targetOffsetDays.Value)) : null;

        // Act
        var dep = WorkItemDependency.Create(Guid.NewGuid(), Guid.NewGuid(), sourceStatus, sourcePlanned, targetPlanned, _now, null, null, null, null, _now);

        // Assert
        Assert.Equal(expected, dep.Health);
    }

    public static IEnumerable<object[]> HealthTestCases()
    {
        // state Done should always be Healthy (ignores planned dates)
        yield return new object[] { WorkStatusCategory.Done, -10, -5, DependencyPlanningHealth.Healthy };

        // state Done should always be Healthy (ignores planned dates)
        yield return new object[] { WorkStatusCategory.Removed, -10, -5, DependencyPlanningHealth.Unhealthy };

        // both unplanned -> AtRisk
        yield return new object[] { WorkStatusCategory.Active, null!, null!, DependencyPlanningHealth.AtRisk };
        // both planned in the past -> treated as unplanned -> AtRisk
        yield return new object[] { WorkStatusCategory.Active, -2, -1, DependencyPlanningHealth.AtRisk };

        // predecessor before successor -> Healthy
        yield return new object[] { WorkStatusCategory.Active, 1, 2, DependencyPlanningHealth.Healthy };
        // predecessor equal successor -> Healthy
        yield return new object[] { WorkStatusCategory.Active, 5, 5, DependencyPlanningHealth.Healthy };
        // predecessor after successor -> Unhealthy
        yield return new object[] { WorkStatusCategory.Active, 5, 2, DependencyPlanningHealth.Unhealthy };
        // predecessor planned and successor unplanned -> Healthy
        yield return new object[] { WorkStatusCategory.Active, 3, null!, DependencyPlanningHealth.Healthy };
    }

    [Fact]
    public void UpdateSourceDetails_RecomputesStateAndHealth()
    {
        // Arrange: start as Active with no planned dates -> AtRisk
        var dep = WorkItemDependency.Create(Guid.NewGuid(), Guid.NewGuid(), WorkStatusCategory.Active, null, null, _now, null, null, null, null, _now);

        Assert.Equal(DependencyState.InProgress, dep.State);
        Assert.Equal(DependencyPlanningHealth.AtRisk, dep.Health);

        // Act: update source to Done
        dep.UpdateSourceDetails(WorkStatusCategory.Done, null, _now);

        // Assert: state becomes Done and health becomes Healthy
        Assert.Equal(DependencyState.Done, dep.State);
        Assert.Equal(DependencyPlanningHealth.Healthy, dep.Health);
    }

    [Fact]
    public void UpdateTargetPlannedDate_RecomputesHealth()
    {
        // Arrange: predecessor after successor -> Unhealthy
        var sourcePlanned = _now.Plus(Duration.FromDays(5));
        var targetPlanned = _now.Plus(Duration.FromDays(2));
        var dep = WorkItemDependency.Create(Guid.NewGuid(), Guid.NewGuid(), WorkStatusCategory.Active, sourcePlanned, targetPlanned, _now, null, null, null, null, _now);
        Assert.Equal(DependencyPlanningHealth.Unhealthy, dep.Health);

        // Act: move successor after predecessor
        var newTarget = _now.Plus(Duration.FromDays(10));
        dep.UpdateTargetPlannedDate(newTarget, _now);

        // Assert: now healthy
        Assert.Equal(DependencyPlanningHealth.Healthy, dep.Health);
    }

    [Fact]
    public void WorkItemLink_Update_SetsAuditFields()
    {
        // Arrange
        var createdBy = Guid.NewGuid();
        var createdOn = _now;
        var hierarchy = WorkItemHierarchy.Create(Guid.NewGuid(), Guid.NewGuid(), createdOn, createdBy, null, null, null);

        // Act
        var newCreatedBy = Guid.NewGuid();
        var removedBy = Guid.NewGuid();
        hierarchy.Update(newCreatedBy, removedBy, "a comment");

        // Assert
        Assert.Equal(newCreatedBy, hierarchy.CreatedById);
        Assert.Equal(removedBy, hierarchy.RemovedById);
        Assert.Equal("a comment", hierarchy.Comment);
    }

    [Fact]
    public void WorkItemLink_RemoveLink_SetsRemovedProperties()
    {
        // Arrange
        var createdBy = Guid.NewGuid();
        var createdOn = _now;
        var hierarchy = WorkItemHierarchy.Create(Guid.NewGuid(), Guid.NewGuid(), createdOn, createdBy, null, null, null);

        // Act
        var removedOn = _now.Plus(Duration.FromDays(1));
        var removedBy = Guid.NewGuid();
        hierarchy.RemoveLink(removedOn, removedBy);

        // Assert
        Assert.Equal(removedOn, hierarchy.RemovedOn);
        Assert.Equal(removedBy, hierarchy.RemovedById);
    }
}
