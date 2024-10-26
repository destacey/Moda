using Moda.Common.Models;
using Moda.Planning.Domain.Interfaces.Roadmaps;
using Moda.Planning.Domain.Models.Roadmaps;
using Moda.Planning.Domain.Tests.Data;
using Moda.Tests.Shared;
using NodaTime.Extensions;
using NodaTime.Testing;

namespace Moda.Planning.Domain.Tests.Sut.Models.Roadmaps;

public class BaseRoadmapItemTests
{
    private readonly TestingDateTimeProvider _dateTimeProvider;
    private readonly RoadmapActivityFaker _faker;

    public BaseRoadmapItemTests()
    {
        _dateTimeProvider = new(new FakeClock(DateTime.UtcNow.ToInstant()));
        _faker = new RoadmapActivityFaker(Guid.NewGuid(), _dateTimeProvider.Today);
    }

    [Fact]
    public void ChangeParent_WhenNoCurrentParent_AndNewParentIsValid_ShouldSucceed()
    {
        // Arrange
        var activity = _faker.Generate();
        var parentActivity = _faker.Generate();

        // Act
        var result = activity.ChangeParent(parentActivity);

        // Assert
        result.IsSuccess.Should().BeTrue();
        activity.ParentId.Should().Be(parentActivity.Id);
        activity.Parent.Should().Be(parentActivity);
        activity.Order.Should().Be(1);
        parentActivity.Children.Should().Contain(activity);
    }

    [Fact]
    public void ChangeParent_WhenCurrentParentExists_AndNewParentIsValid_ShouldSucceed()
    {
        // Arrange
        var originalParentActivity = _faker.WithChildren(2);
        var newParentActivity = _faker.WithChildren(2);

        var activity = originalParentActivity.Children[0] as RoadmapActivity;
        var activityFromPreviousParent = originalParentActivity.Children[1] as RoadmapActivity;

        // Act
        var result = activity!.ChangeParent(newParentActivity);

        // Assert
        result.IsSuccess.Should().BeTrue();
        activity.ParentId.Should().Be(newParentActivity.Id);
        activity.Parent.Should().Be(newParentActivity);
        activity.Order.Should().Be(3);
        newParentActivity.Children.Should().Contain(activity);
        originalParentActivity.Children.Should().NotContain(activity);
        activityFromPreviousParent!.Order.Should().Be(1);
    }

    [Fact]
    public void ChangeParent_WhenNewParentIsSameAsCurrentParent_ShouldReturnFailure()
    {
        // Arrange
        var parentActivity = _faker.Generate();

        var upsertActivity = new TestUpsertRoadmapActivity(_faker.Generate());

        var activity = parentActivity.CreateChildActivity(upsertActivity);

        // Act
        var result = activity.ChangeParent(parentActivity);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Unable to change the parent because the new parent is the same as the current parent.");
    }


    [Fact]
    public void ChangeParent_WhenNewParentIsSelf_ShouldReturnFailure()
    {
        // Arrange
        var activity = _faker.Generate();

        // Act
        var result = activity.ChangeParent(activity);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Unable to make the Roadmap Item a child of itself.");
    }

    //[Fact]
    //public void ChangeParent_ShouldReturnFailure_WhenCurrentParentDataNotLoaded()
    //{
    //    // Arrange
    //    var roadmapId = Guid.NewGuid();
    //    var parentId = Guid.NewGuid();
    //    var roadmapItem = new TestRoadmapItem(roadmapId, "Test Item", RoadmapItemType.Activity)
    //    {
    //        ParentId = parentId
    //    };
    //    var newParent = new Mock<RoadmapActivity>();

    //    // Act
    //    var result = roadmapItem.ChangeParent(newParent.Object);

    //    // Assert
    //    Assert.True(result.IsFailure);
    //    Assert.Equal("Unable to change the parent because the current parent data has not been loaded.", result.Error);
    //}

    private record TestUpsertRoadmapActivity : IUpsertRoadmapActivity
    {

        public TestUpsertRoadmapActivity(RoadmapActivity activity)
        {
            ParentId = activity.ParentId;
            Name = activity.Name;
            Description = activity.Description;
            DateRange = activity.DateRange;
            Color = activity.Color;
        }

        public Guid? ParentId { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public LocalDateRange DateRange { get; set; } = default!;
        public string? Color { get; set; }
    }
}