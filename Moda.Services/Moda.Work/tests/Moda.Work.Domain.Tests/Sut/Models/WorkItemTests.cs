using Moda.Common.Domain.Enums.Work;
using Moda.Tests.Shared;
using Moda.Work.Domain.Interfaces;
using Moda.Work.Domain.Tests.Data;

namespace Moda.Work.Domain.Tests.Sut.Models;
public class WorkItemTests
{
    private readonly TestingDateTimeProvider _dateTimeProvider;
    private readonly WorkItemFaker _workItemFaker;
    private readonly WorkTypeFaker _workTypeFaker;

    public WorkItemTests()
    {
        _dateTimeProvider = new(new DateTime(2024,04,01, 11,0,0));
        _workItemFaker = new WorkItemFaker(); 
        _workTypeFaker = new WorkTypeFaker();
    }

    #region UpdateParent

    [Fact]
    public void UpdateParent_WithNoParent_SetNullParent_ShouldSucceed()
    {
        // Arrange
        var workItem = _workItemFaker.Generate();
        IWorkItemParentInfo? parentInfo = null;

        // Act
        var result = workItem.UpdateParent(parentInfo, workItem.Type);

        // Assert
        Assert.True(result.IsSuccess);
        workItem.ParentId.Should().BeNull();
    }

    [Fact]
    public void UpdateParent_WithParent_SetNullParent_ShouldSucceed()
    {
        // Arrange
        var workItem = _workItemFaker.WithData(parentId: Guid.NewGuid()).Generate();
        IWorkItemParentInfo? parentInfo = null;

        // Act
        var result = workItem.UpdateParent(parentInfo, workItem.Type);

        // Assert
        Assert.True(result.IsSuccess);
        workItem.ParentId.Should().BeNull();
    }

    [Fact]
    public void UpdateParent_StoryWithNoParent_SetEpicParent_ShouldSucceed()
    {
        // Arrange
        var storyWorkType = _workTypeFaker.AsStory().Generate();
        var epicWorkType = _workTypeFaker.AsEpic().Generate();
        var workItem = _workItemFaker.WithData(type: storyWorkType).Generate();
        var parentInfo = new MockParentInfo(Guid.NewGuid(), 123456, epicWorkType.Level!.Tier, epicWorkType.Level.Order);

        // Act
        var result = workItem.UpdateParent(parentInfo, workItem.Type);

        // Assert
        Assert.True(result.IsSuccess);
        workItem.ParentId.Should().Be(parentInfo.Id);
    }

    [Fact]
    public void UpdateParent_FeatureWithNoParent_SetEpicParent_ShouldSucceed()
    {
        // Arrange
        var featureWorkType = _workTypeFaker.AsFeature().Generate();
        var epicWorkType = _workTypeFaker.AsEpic().Generate();
        var workItem = _workItemFaker.WithData(type: featureWorkType).Generate();
        var parentInfo = new MockParentInfo(Guid.NewGuid(), 123456, epicWorkType.Level!.Tier, epicWorkType.Level.Order);

        // Act
        var result = workItem.UpdateParent(parentInfo, workItem.Type);

        // Assert
        Assert.True(result.IsSuccess);
        workItem.ParentId.Should().Be(parentInfo.Id);
    }

    [Fact]
    public void UpdateParent_StoryWithNoParent_SetStoryParent_ShouldFail()
    {
        // Arrange
        var storyWorkType = _workTypeFaker.AsStory().Generate();
        var workItem = _workItemFaker.WithData(type: storyWorkType).Generate();
        var parentInfo = new MockParentInfo(Guid.NewGuid(), 123456, storyWorkType.Level!.Tier, storyWorkType.Level.Order);

        // Act
        var result = workItem.UpdateParent(parentInfo, workItem.Type);

        // Assert
        Assert.True(result.IsFailure);
        workItem.ParentId.Should().BeNull();
        result.Error.Should().Be("Only portfolio tier work items can be parents.");
    }

    [Fact]
    public void UpdateParent_StoryWithNoParent_SetOtherParent_ShouldFail()
    {
        // Arrange
        var storyWorkType = _workTypeFaker.AsStory().Generate();
        var otherWorkType = _workTypeFaker.AsOther().Generate();
        var workItem = _workItemFaker.WithData(type: storyWorkType).Generate();
        var parentInfo = new MockParentInfo(Guid.NewGuid(), 123456, otherWorkType.Level!.Tier, otherWorkType.Level.Order);

        // Act
        var result = workItem.UpdateParent(parentInfo, workItem.Type);

        // Assert
        Assert.True(result.IsFailure);
        workItem.ParentId.Should().BeNull();
        result.Error.Should().Be("Only portfolio tier work items can be parents.");
    }

    [Fact]
    public void UpdateParent_FeatureWithNoParent_SetOtherParent_ShouldFail()
    {
        // Arrange
        var featureWorkType = _workTypeFaker.AsFeature().Generate();
        var otherWorkType = _workTypeFaker.AsOther().Generate();
        var workItem = _workItemFaker.WithData(type: featureWorkType).Generate();
        var parentInfo = new MockParentInfo(Guid.NewGuid(), 123456, otherWorkType.Level!.Tier, otherWorkType.Level.Order);

        // Act
        var result = workItem.UpdateParent(parentInfo, workItem.Type);

        // Assert
        Assert.True(result.IsFailure);
        workItem.ParentId.Should().BeNull();
        result.Error.Should().Be("Only portfolio tier work items can be parents.");
    }

    [Fact]
    public void UpdateParent_FeatureWithNoParent_SetStoryParent_ShouldFail()
    {
        // Arrange
        var featureWorkType = _workTypeFaker.AsFeature().Generate();
        var storyWorkType = _workTypeFaker.AsStory().Generate();
        var workItem = _workItemFaker.WithData(type: featureWorkType).Generate();
        var parentInfo = new MockParentInfo(Guid.NewGuid(), 123456, storyWorkType.Level!.Tier, storyWorkType.Level.Order);

        // Act
        var result = workItem.UpdateParent(parentInfo, workItem.Type);

        // Assert
        Assert.True(result.IsFailure);
        workItem.ParentId.Should().BeNull();
        result.Error.Should().Be("Only portfolio tier work items can be parents.");
    }

    [Fact]
    public void UpdateParent_FeatureWithNoParent_SetFeatureParent_ShouldFail()
    {
        // Arrange
        var featureWorkType = _workTypeFaker.AsFeature().Generate();
        var workItem = _workItemFaker.WithData(type: featureWorkType).Generate();
        var parentInfo = new MockParentInfo(Guid.NewGuid(), 123456, featureWorkType.Level!.Tier, featureWorkType.Level.Order);

        // Act
        var result = workItem.UpdateParent(parentInfo, workItem.Type);

        // Assert
        Assert.True(result.IsFailure);
        workItem.ParentId.Should().BeNull();
        result.Error.Should().Be("The parent must be a higher level than the work item.");
    }

    [Fact]
    public void UpdateParent_StoryWithParent_SetEpicParent_ShouldSucceed()
    {
        // Arrange
        var storyWorkType = _workTypeFaker.AsStory().Generate();
        var epicWorkType = _workTypeFaker.AsEpic().Generate();
        var workItem = _workItemFaker.WithData(type: storyWorkType, parentId: Guid.NewGuid()).Generate();
        var parentInfo = new MockParentInfo(Guid.NewGuid(), 123456, epicWorkType.Level!.Tier, epicWorkType.Level.Order);

        // Act
        var result = workItem.UpdateParent(parentInfo, workItem.Type);

        // Assert
        Assert.True(result.IsSuccess);
        workItem.ParentId.Should().Be(parentInfo.Id);
    }

    [Fact]
    public void UpdateParent_FeatureWithParent_SetEpicParent_ShouldSucceed()
    {
        // Arrange
        var featureWorkType = _workTypeFaker.AsFeature().Generate();
        var epicWorkType = _workTypeFaker.AsEpic().Generate();
        var workItem = _workItemFaker.WithData(type: featureWorkType, parentId: Guid.NewGuid()).Generate();
        var parentInfo = new MockParentInfo(Guid.NewGuid(), 123456, epicWorkType.Level!.Tier, epicWorkType.Level.Order);

        // Act
        var result = workItem.UpdateParent(parentInfo, workItem.Type);

        // Assert
        Assert.True(result.IsSuccess);
        workItem.ParentId.Should().Be(parentInfo.Id);
    }

    [Fact]
    public void UpdateParent_StoryWithParent_SetStoryParent_ShouldFail()
    {
        // Arrange
        var storyWorkType = _workTypeFaker.AsStory().Generate();
        var expectedParentId = Guid.NewGuid();
        var workItem = _workItemFaker.WithData(type: storyWorkType, parentId: expectedParentId).Generate();
        var parentInfo = new MockParentInfo(Guid.NewGuid(), 123456, storyWorkType.Level!.Tier, storyWorkType.Level.Order);

        // Act
        var result = workItem.UpdateParent(parentInfo, workItem.Type);

        // Assert
        Assert.True(result.IsFailure);
        workItem.ParentId.Should().Be(expectedParentId);
        result.Error.Should().Be("Only portfolio tier work items can be parents.");
    }

    [Fact]
    public void UpdateParent_StoryWithParent_SetOtherParent_ShouldFail()
    {
        // Arrange
        var storyWorkType = _workTypeFaker.AsStory().Generate();
        var otherWorkType = _workTypeFaker.AsOther().Generate();
        var expectedParentId = Guid.NewGuid();
        var workItem = _workItemFaker.WithData(type: storyWorkType, parentId: expectedParentId).Generate();
        var parentInfo = new MockParentInfo(Guid.NewGuid(), 123456, otherWorkType.Level!.Tier, otherWorkType.Level.Order);

        // Act
        var result = workItem.UpdateParent(parentInfo, workItem.Type);

        // Assert
        Assert.True(result.IsFailure);
        workItem.ParentId.Should().Be(expectedParentId);
        result.Error.Should().Be("Only portfolio tier work items can be parents.");
    }

    [Fact]
    public void UpdateParent_FeatureWithParent_SetOtherParent_ShouldFail()
    {
        // Arrange
        var featureWorkType = _workTypeFaker.AsFeature().Generate();
        var otherWorkType = _workTypeFaker.AsOther().Generate();
        var expectedParentId = Guid.NewGuid();
        var workItem = _workItemFaker.WithData(type: featureWorkType, parentId: expectedParentId).Generate();
        var parentInfo = new MockParentInfo(Guid.NewGuid(), 123456, otherWorkType.Level!.Tier, otherWorkType.Level.Order);

        // Act
        var result = workItem.UpdateParent(parentInfo, workItem.Type);

        // Assert
        Assert.True(result.IsFailure);
        workItem.ParentId.Should().Be(expectedParentId);
        result.Error.Should().Be("Only portfolio tier work items can be parents.");
    }

    [Fact]
    public void UpdateParent_FeatureWithParent_SetStoryParent_ShouldFail()
    {
        // Arrange
        var featureWorkType = _workTypeFaker.AsFeature().Generate();
        var storyWorkType = _workTypeFaker.AsStory().Generate();
        var expectedParentId = Guid.NewGuid();
        var workItem = _workItemFaker.WithData(type: featureWorkType, parentId: expectedParentId).Generate();
        var parentInfo = new MockParentInfo(Guid.NewGuid(), 123456, storyWorkType.Level!.Tier, storyWorkType.Level.Order);

        // Act
        var result = workItem.UpdateParent(parentInfo, workItem.Type);

        // Assert
        Assert.True(result.IsFailure);
        workItem.ParentId.Should().Be(expectedParentId);
        result.Error.Should().Be("Only portfolio tier work items can be parents.");
    }

    [Fact]
    public void UpdateParent_FeatureWithParent_SetFeatureParent_ShouldFail()
    {
        // Arrange
        var featureWorkType = _workTypeFaker.AsFeature().Generate();
        var expectedParentId = Guid.NewGuid();
        var workItem = _workItemFaker.WithData(type: featureWorkType, parentId: expectedParentId).Generate();
        var parentInfo = new MockParentInfo(Guid.NewGuid(), 123456, featureWorkType.Level!.Tier, featureWorkType.Level.Order);

        // Act
        var result = workItem.UpdateParent(parentInfo, workItem.Type);

        // Assert
        Assert.True(result.IsFailure);
        workItem.ParentId.Should().Be(expectedParentId);
        result.Error.Should().Be("The parent must be a higher level than the work item.");
    }

    [Fact]
    public void UpdateParent_FeatureWithParentAndWithoutLevel_SetFeatureParent_ShouldFail()
    {
        // Arrange
        var workItemType = _workTypeFaker.AsFeature().Generate();
        var parentType = _workTypeFaker.AsFeature().Generate();

        workItemType.Level = null;

        var expectedParentId = Guid.NewGuid();
        var workItem = _workItemFaker.WithData(type: workItemType, parentId: expectedParentId).Generate();
        var parentInfo = new MockParentInfo(Guid.NewGuid(), 123456, parentType.Level!.Tier, parentType.Level.Order);

        // Act
        var result = workItem.UpdateParent(parentInfo, workItem.Type);

        // Assert
        Assert.True(result.IsFailure);
        workItem.ParentId.Should().Be(expectedParentId);
        result.Error.Should().Be("Unable to set the work item parent without the type and level.");
    }

    #endregion UpdateParent

    public sealed record MockParentInfo(Guid Id, int? ExternalId, WorkTypeTier Tier, int LevelOrder) : IWorkItemParentInfo;
    public sealed record MockBadParentInfo : IWorkItemParentInfo
    {
        public Guid Id { get; set; }

        public int? ExternalId { get; set; }

        public WorkTypeTier Tier { get; set; }

        public int LevelOrder { get; set; }
    }
}
