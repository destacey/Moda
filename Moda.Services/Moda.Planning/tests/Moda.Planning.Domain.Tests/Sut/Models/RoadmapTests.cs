﻿using Moda.Common.Domain.Enums;
using Moda.Common.Models;
using Moda.Planning.Domain.Enums;
using Moda.Planning.Domain.Models.Roadmaps;
using Moda.Planning.Domain.Tests.Data;
using Moda.Planning.Domain.Tests.Models;
using Moda.Tests.Shared;
using NodaTime.Extensions;
using NodaTime.Testing;

namespace Moda.Planning.Domain.Tests.Sut.Models;
public class RoadmapTests
{
    private readonly TestingDateTimeProvider _dateTimeProvider;
    private readonly RoadmapFaker _faker;
    private readonly RoadmapActivityFaker _activityFaker;
    private readonly RoadmapMilestoneFaker _milestoneFaker;
    private readonly RoadmapTimeboxFaker _timeboxFaker;

    public RoadmapTests()
    {
        _dateTimeProvider = new(new FakeClock(DateTime.UtcNow.ToInstant()));
        _faker = new RoadmapFaker(_dateTimeProvider.Today);
        _activityFaker = new RoadmapActivityFaker(localDate: _dateTimeProvider.Today);
        _milestoneFaker = new RoadmapMilestoneFaker(localDate: _dateTimeProvider.Today);
        _timeboxFaker = new RoadmapTimeboxFaker(localDate: _dateTimeProvider.Today);
    }

    [Fact]
    public void Create_ValidParameters_ShouldReturnSuccess()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();

        // Act
        var result = Roadmap.Create(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(fakeRoadmap.Name);
        result.Value.Description.Should().Be(fakeRoadmap.Description);
        result.Value.DateRange.Should().Be(fakeRoadmap.DateRange);
        result.Value.Visibility.Should().Be(fakeRoadmap.Visibility);
        result.Value.RoadmapManagers.Should().HaveCount(1);
        result.Value.RoadmapManagers.First().ManagerId.Should().Be(managerId);
    }

    [Fact]
    public void Create_NoManagers_ShouldReturnFailure()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managers = Array.Empty<Guid>();

        // Act
        var result = Roadmap.Create(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, managers);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Required input roadmapManagerIds was empty. (Parameter 'roadmapManagerIds')");
    }

    [Fact]
    public void Update_ValidParameters_ShouldReturnSuccess()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.Create(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;
        
        var newName = "Updated Name";
        var newDescription = "Updated Description";
        var newDateRange = new LocalDateRange(_dateTimeProvider.Today.PlusDays(1), _dateTimeProvider.Today.PlusDays(11));
        var newVisibility = Visibility.Private;

        // Act
        var result = roadmap.Update(newName, newDescription, newDateRange,[managerId], newVisibility, managerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        roadmap.Name.Should().Be(newName);
        roadmap.Description.Should().Be(newDescription);
        roadmap.DateRange.Should().Be(newDateRange);
        roadmap.Visibility.Should().Be(newVisibility);
    }

    [Fact]
    public void Update_InvalidManagerId_ShouldReturnFailure()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.Create(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;

        // Act
        var result = roadmap.Update("Updated Name", "Updated Description", new LocalDateRange(_dateTimeProvider.Today.PlusDays(1), _dateTimeProvider.Today.PlusDays(11)), [managerId], Visibility.Private, Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("User is not a roadmap manager of this roadmap.");
    }

    #region Add/Remove Manager Tests

    [Fact]
    public void AddManager_ValidManagerId_ShouldReturnSuccess()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.Create(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;

        var initialManagers = new Guid[] { managerId };
        var managerId2 = Guid.NewGuid();
        var expectedManagers = initialManagers.Append(managerId2);

        // Act
        var result = roadmap.AddManager(managerId2, managerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        roadmap.RoadmapManagers.Should().HaveCount(2);
        roadmap.RoadmapManagers.Select(x => x.ManagerId).Should().BeEquivalentTo(expectedManagers);
    }

    [Fact]
    public void AddManager_DuplicateManagerId_ShouldReturnFailure()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.Create(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;

        // Act
        var result = roadmap.AddManager(managerId, managerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Roadmap manager already exists on this roadmap.");
        roadmap.RoadmapManagers.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveManager_ValidManagerId_ShouldReturnSuccess()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var managerId2 = Guid.NewGuid();
        var roadmap = Roadmap.Create(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId, managerId2]).Value;

        // Act
        var result = roadmap.RemoveManager(managerId2, managerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        roadmap.RoadmapManagers.Should().HaveCount(1);
        roadmap.RoadmapManagers.First().ManagerId.Should().Be(managerId);
    }

    [Fact]
    public void RemoveManager_InvalidManagerId_ShouldReturnFailure()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.Create(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;

        // Act
        var result = roadmap.RemoveManager(Guid.NewGuid(), managerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Roadmap manager does not exist on this roadmap.");
    }

    [Fact]
    public void RemoveManager_LastManager_ShouldReturnFailure()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.Create(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;

        // Act
        var result = roadmap.RemoveManager(managerId, managerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Roadmap must have at least one roadmap manager.");
    }

    #endregion Add/Remove Manager Tests

    #region Create Item Tests

    [Fact]
    public void CreateActivity_WhenUserIsNotManager_ShouldReturnFailure()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.Create(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;

        var nonManagerId = Guid.NewGuid();
        var upsertActivity = new TestUpsertRoadmapActivity(_activityFaker.Generate());

        // Act
        var result = roadmap.CreateActivity(upsertActivity, nonManagerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("User is not a roadmap manager of this roadmap.");
    }

    [Fact]
    public void CreateActivity_AsRootItem_ShouldReturnSuccess()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.Create(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;
        var upsertActivity = new TestUpsertRoadmapActivity(_activityFaker.Generate());

        // Act
        var result = roadmap.CreateActivity(upsertActivity, managerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(upsertActivity.Name);
        result.Value.Type.Should().Be(RoadmapItemType.Activity);
        result.Value.ParentId.Should().BeNull();
        result.Value.Children.Should().BeEmpty();
        roadmap.Items.Should().Contain(result.Value);
    }

    [Fact]
    public void CreateActivity_WithInvalidParent_ShouldReturnFailure()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.Create(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;
        var activity = _activityFaker.WithData(parentId: Guid.NewGuid()).Generate();
        var upsertActivity = new TestUpsertRoadmapActivity(activity);

        // Act
        var result = roadmap.CreateActivity(upsertActivity, managerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Parent Roadmap Activity does not exist on this roadmap.");
    }

    [Fact]
    public void CreateActivity_AsChildItem_ShouldReturnSuccess()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.Create(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;

        // Create parent activity
        var parentActivity = _activityFaker.Generate();
        var upsertParentActivity = new TestUpsertRoadmapActivity(parentActivity);
        var parentResult = roadmap.CreateActivity(upsertParentActivity, managerId);
        parentResult.IsSuccess.Should().BeTrue();

        // Create child activity
        var childActivity = _activityFaker
            .WithData(parentId: parentResult.Value.Id)
            .Generate();
        var upsertChildActivity = new TestUpsertRoadmapActivity(childActivity);

        // Act
        var result = roadmap.CreateActivity(upsertChildActivity, managerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ParentId.Should().Be(parentResult.Value.Id);
        roadmap.Items.Should().Contain(result.Value);
    }

    [Fact]
    public void CreateMilestone_WhenUserIsNotManager_ShouldReturnFailure()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.Create(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;

        var nonManagerId = Guid.NewGuid();
        var upsertMilestone = new TestUpsertRoadmapMilestone(_milestoneFaker.Generate());

        // Act
        var result = roadmap.CreateMilestone(upsertMilestone, nonManagerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("User is not a roadmap manager of this roadmap.");
    }

    [Fact]
    public void CreateMilestone_AsRootItem_ShouldReturnSuccess()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.Create(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;
        var upsertMilestone = new TestUpsertRoadmapMilestone(_milestoneFaker.Generate());

        // Act
        var result = roadmap.CreateMilestone(upsertMilestone, managerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(upsertMilestone.Name);
        result.Value.Type.Should().Be(RoadmapItemType.Milestone);
        result.Value.ParentId.Should().BeNull();
        roadmap.Items.Should().Contain(result.Value);
    }


    [Fact]
    public void CreateTimebox_WhenUserIsNotManager_ShouldReturnFailure()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.Create(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;

        var nonManagerId = Guid.NewGuid();
        var upsertTimebox = new TestUpsertRoadmapTimebox(_timeboxFaker.Generate());

        // Act
        var result = roadmap.CreateTimebox(upsertTimebox, nonManagerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("User is not a roadmap manager of this roadmap.");
    }

    [Fact]
    public void CreateTimebox_AsRootItem_ShouldReturnSuccess()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.Create(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;
        var upsertMilestone = new TestUpsertRoadmapTimebox(_timeboxFaker.Generate());

        // Act
        var result = roadmap.CreateTimebox(upsertMilestone, managerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(upsertMilestone.Name);
        result.Value.Type.Should().Be(RoadmapItemType.Timebox);
        result.Value.ParentId.Should().BeNull();
        roadmap.Items.Should().Contain(result.Value);
    }

    #endregion Create Item Tests


    #region Update Item Tests

    // ACTIVITY

    [Fact]
    public void UpdateActivity_WhenUserIsNotManager_ShouldReturnFailure()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.Create(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;

        var createResult = roadmap.CreateActivity(new TestUpsertRoadmapActivity(_activityFaker.Generate()), managerId);
        createResult.IsSuccess.Should().BeTrue();

        var nonManagerId = Guid.NewGuid();
        var updateActivity = new TestUpsertRoadmapActivity(_activityFaker.Generate());

        // Act
        var result = roadmap.UpdateActivity(createResult.Value.Id, updateActivity, nonManagerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("User is not a roadmap manager of this roadmap.");
    }

    [Fact]
    public void UpdateActivity_WhenActivityDoesNotExist_ShouldReturnFailure()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.Create(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;

        var updateActivity = new TestUpsertRoadmapActivity(_activityFaker.Generate());

        // Act
        var result = roadmap.UpdateActivity(Guid.NewGuid(), updateActivity, managerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Roadmap Activity does not exist on this roadmap.");
    }

    [Fact]
    public void UpdateActivity_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.Create(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;

        var createResult = roadmap.CreateActivity(new TestUpsertRoadmapActivity(_activityFaker.Generate()), managerId);
        createResult.IsSuccess.Should().BeTrue();

        var updateActivity = new TestUpsertRoadmapActivity(_activityFaker
            .WithData(
                name: "Updated Activity",
                description: "Updated Description",
                dateRange: new LocalDateRange(_dateTimeProvider.Today.PlusDays(1), _dateTimeProvider.Today.PlusDays(5))
            )
            .Generate());

        // Act
        var result = roadmap.UpdateActivity(createResult.Value.Id, updateActivity, managerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updatedActivity = roadmap.Items.OfType<RoadmapActivity>().First();
        updatedActivity.Name.Should().Be("Updated Activity");
        updatedActivity.Description.Should().Be("Updated Description");
        updatedActivity.DateRange.Start.Should().Be(_dateTimeProvider.Today.PlusDays(1));
        updatedActivity.DateRange.End.Should().Be(_dateTimeProvider.Today.PlusDays(5));
    }

    [Fact]
    public void UpdateActivity_ChangeParentFromRootToChild_ShouldReturnSuccess()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.Create(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;
        roadmap.SetPrivate(x => x.Id, Guid.NewGuid());

        // Create parent activity
        var createParentResult = roadmap.CreateActivity(new TestUpsertRoadmapActivity(_activityFaker.Generate()), managerId);
        createParentResult.IsSuccess.Should().BeTrue();
        createParentResult.Value.SetPrivate(x => x.Id, Guid.NewGuid());

        // Create child activity
        var createChildResult = roadmap.CreateActivity(new TestUpsertRoadmapActivity(_activityFaker.Generate()), managerId);
        createChildResult.IsSuccess.Should().BeTrue();

        // Create update request with new parent
        var updateActivity = new TestUpsertRoadmapActivity(createChildResult.Value);
        updateActivity.ParentId = createParentResult.Value.Id;

        // Act
        var result = roadmap.UpdateActivity(createChildResult.Value.Id, updateActivity, managerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updatedActivity = roadmap.Items.OfType<RoadmapActivity>().First(x => x.Id == createChildResult.Value.Id);
        updatedActivity.ParentId.Should().Be(createParentResult.Value.Id);
        createParentResult.Value.Children.Count.Should().Be(1);
        createParentResult.Value.Children.Should().Contain(updatedActivity);

        var rootActivities = roadmap.Items.OfType<RoadmapActivity>().Where(x => x.ParentId == null).ToList();
        rootActivities.Should().HaveCount(1);
    }

    [Fact]
    public void UpdateActivity_ChangeParentFromChildToRoot_ShouldReturnSuccessAndUpdateOrder()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.Create(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;
        roadmap.SetPrivate(x => x.Id, Guid.NewGuid());

        // Create parent activity
        var parentActivity = _activityFaker.Generate();
        var createParentResult = roadmap.CreateActivity(new TestUpsertRoadmapActivity(parentActivity), managerId);
        createParentResult.IsSuccess.Should().BeTrue();
        createParentResult.Value.SetPrivate(x => x.Id, Guid.NewGuid());

        // Create child activity
        var childActivity = _activityFaker
            .WithData(parentId: createParentResult.Value.Id)
            .Generate();
        var createChildResult = roadmap.CreateActivity(new TestUpsertRoadmapActivity(childActivity), managerId);
        createChildResult.IsSuccess.Should().BeTrue();
        createChildResult.Value.SetPrivate(x => x.Id, Guid.NewGuid());
        createChildResult.Value.SetPrivate(x => x.Parent, createParentResult.Value);

        // Create update request to make it a root activity
        var updateActivity = new TestUpsertRoadmapActivity(createChildResult.Value)
        {
            ParentId = null
        };

        // Act
        var result = roadmap.UpdateActivity(createChildResult.Value.Id, updateActivity, managerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updatedActivity = roadmap.Items.OfType<RoadmapActivity>().First(x => x.Id == createChildResult.Value.Id);
        updatedActivity.ParentId.Should().BeNull();

        var rootActivities = roadmap.Items.OfType<RoadmapActivity>().Where(x => x.ParentId == null).ToList();
        rootActivities.Should().HaveCount(2);
        rootActivities.Should().BeInAscendingOrder(x => x.Order);

        var originalParentActivity = rootActivities.First(x => x.Id == createParentResult.Value.Id);
        originalParentActivity.Children.Should().BeEmpty();
    }

    // MILESTONE

    [Fact]
    public void UpdateMilestone_WhenUserIsNotManager_ShouldReturnFailure()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.Create(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;

        var createResult = roadmap.CreateMilestone(new TestUpsertRoadmapMilestone(_milestoneFaker.Generate()), managerId);
        createResult.IsSuccess.Should().BeTrue();

        var nonManagerId = Guid.NewGuid();
        var updateMilestone = new TestUpsertRoadmapMilestone(_milestoneFaker.Generate());

        // Act
        var result = roadmap.UpdateMilestone(createResult.Value.Id, updateMilestone, nonManagerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("User is not a roadmap manager of this roadmap.");
    }

    [Fact]
    public void UpdateMilestone_WhenMilestoneDoesNotExist_ShouldReturnFailure()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.Create(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;

        var updateMilestone = new TestUpsertRoadmapMilestone(_milestoneFaker.Generate());

        // Act
        var result = roadmap.UpdateMilestone(Guid.NewGuid(), updateMilestone, managerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Roadmap Milestone does not exist on this roadmap.");
    }

    [Fact]
    public void UpdateMilestone_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.Create(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;
        roadmap.SetPrivate(x => x.Id, Guid.NewGuid());

        var milestone = _milestoneFaker.Generate();
        var createResult = roadmap.CreateMilestone(new TestUpsertRoadmapMilestone(milestone), managerId);
        createResult.IsSuccess.Should().BeTrue();
        createResult.Value.SetPrivate(x => x.Id, Guid.NewGuid());

        var updateMilestone = new TestUpsertRoadmapMilestone(_milestoneFaker
            .WithData(
                name: "Updated Milestone",
                description: "Updated Description",
                date: _dateTimeProvider.Today.PlusDays(1)
            )
            .Generate());

        // Act
        var result = roadmap.UpdateMilestone(createResult.Value.Id, updateMilestone, managerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updatedMilestone = roadmap.Items.OfType<RoadmapMilestone>().First(i => i.Id == createResult.Value.Id);
        updatedMilestone.Name.Should().Be("Updated Milestone");
        updatedMilestone.Description.Should().Be("Updated Description");
        updatedMilestone.Date.Should().Be(_dateTimeProvider.Today.PlusDays(1));
    }

    [Fact]
    public void UpdateMilestone_ChangeParentFromRootToChild_ShouldReturnSuccess()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.Create(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;
        roadmap.SetPrivate(x => x.Id, Guid.NewGuid());

        // Create parent activity
        var createParentResult = roadmap.CreateActivity(new TestUpsertRoadmapActivity(_activityFaker.Generate()), managerId);
        createParentResult.IsSuccess.Should().BeTrue();
        createParentResult.Value.SetPrivate(x => x.Id, Guid.NewGuid());

        // Create milestone
        var createMilestoneResult = roadmap.CreateMilestone(new TestUpsertRoadmapMilestone(_milestoneFaker.Generate()), managerId);
        createMilestoneResult.IsSuccess.Should().BeTrue();

        // Create update request with new parent
        var updateMilestone = new TestUpsertRoadmapMilestone(createMilestoneResult.Value)
        {
            ParentId = createParentResult.Value.Id
        };

        // Act
        var result = roadmap.UpdateMilestone(createMilestoneResult.Value.Id, updateMilestone, managerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updatedMilestone = roadmap.Items.OfType<RoadmapMilestone>().First(x => x.Id == createMilestoneResult.Value.Id);
        updatedMilestone.ParentId.Should().Be(createParentResult.Value.Id);
        createParentResult.Value.Children.Count.Should().Be(1);
        createParentResult.Value.Children.Should().Contain(updatedMilestone);
    }

    // TIMEBOX

    [Fact]
    public void UpdateTimebox_WhenUserIsNotManager_ShouldReturnFailure()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.Create(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;

        var createResult = roadmap.CreateTimebox(new TestUpsertRoadmapTimebox(_timeboxFaker.Generate()), managerId);
        createResult.IsSuccess.Should().BeTrue();

        var nonManagerId = Guid.NewGuid();
        var updateTimebox = new TestUpsertRoadmapTimebox(_timeboxFaker.Generate());

        // Act
        var result = roadmap.UpdateTimebox(createResult.Value.Id, updateTimebox, nonManagerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("User is not a roadmap manager of this roadmap.");
    }

    [Fact]
    public void UpdateTimebox_WhenMilestoneDoesNotExist_ShouldReturnFailure()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.Create(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;

        var updateTimebox = new TestUpsertRoadmapTimebox(_timeboxFaker.Generate());

        // Act
        var result = roadmap.UpdateTimebox(Guid.NewGuid(), updateTimebox, managerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Roadmap Timebox does not exist on this roadmap.");
    }

    [Fact]
    public void UpdateTimebox_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.Create(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;
        roadmap.SetPrivate(x => x.Id, Guid.NewGuid());

        var createResult = roadmap.CreateTimebox(new TestUpsertRoadmapTimebox(_timeboxFaker.Generate()), managerId);
        createResult.IsSuccess.Should().BeTrue();
        createResult.Value.SetPrivate(x => x.Id, Guid.NewGuid());

        var updateTimebox = new TestUpsertRoadmapTimebox(_timeboxFaker
            .WithData(
                name: "Updated Timebox",
                description: "Updated Description",
                dateRange: new LocalDateRange(_dateTimeProvider.Today.PlusDays(1), _dateTimeProvider.Today.PlusDays(5))
            )
            .Generate());

        // Act
        var result = roadmap.UpdateTimebox(createResult.Value.Id, updateTimebox, managerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updatedTimebox = roadmap.Items.OfType<RoadmapTimebox>().First(i => i.Id == createResult.Value.Id);
        updatedTimebox.Name.Should().Be("Updated Timebox");
        updatedTimebox.Description.Should().Be("Updated Description");
        updatedTimebox.ParentId.Should().BeNull();
        updatedTimebox.DateRange.Start.Should().Be(_dateTimeProvider.Today.PlusDays(1));
        updatedTimebox.DateRange.End.Should().Be(_dateTimeProvider.Today.PlusDays(5));
    }


    [Fact]
    public void UpdateTimebox_ChangeParentFromRootToChild_ShouldReturnSuccess()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.Create(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;
        roadmap.SetPrivate(x => x.Id, Guid.NewGuid());

        // Create parent activity
        var createParentResult = roadmap.CreateActivity(new TestUpsertRoadmapActivity(_activityFaker.Generate()), managerId);
        createParentResult.IsSuccess.Should().BeTrue();
        createParentResult.Value.SetPrivate(x => x.Id, Guid.NewGuid());

        // Create milestone
        var createTimeboxResult = roadmap.CreateTimebox(new TestUpsertRoadmapTimebox(_timeboxFaker.Generate()), managerId);
        createTimeboxResult.IsSuccess.Should().BeTrue();

        // Create update request with new parent
        var updateTimebox = new TestUpsertRoadmapTimebox(createTimeboxResult.Value)
        {
            ParentId = createParentResult.Value.Id
        };

        // Act
        var result = roadmap.UpdateTimebox(createTimeboxResult.Value.Id, updateTimebox, managerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updatedTimebox = roadmap.Items.OfType<RoadmapTimebox>().First(x => x.Id == createTimeboxResult.Value.Id);
        updatedTimebox.ParentId.Should().Be(createParentResult.Value.Id);
        createParentResult.Value.Children.Count.Should().Be(1);
        createParentResult.Value.Children.Should().Contain(updatedTimebox);
    }



    #endregion Update Item Tests

    //[Fact]
    //public void SetChildrenOrder_ForAll_WhenValidChildrenProvided_ShouldReturnSuccess()
    //{
    //    // Arrange
    //    var roadmap = _faker.WithChildren(3);
    //    var managerId = roadmap.RoadmapManagers.First().ManagerId;

    //    var children = roadmap.Children.OrderBy(c => c.Order).ToList();

    //    var child1 = children[0];
    //    var child2 = children[1];
    //    var child3 = children[2];

    //    var childLinks = new Dictionary<Guid, int>
    //    {
    //        { child1.Id, 2 },
    //        { child2.Id, 17 }, // setting the higher order than the count of child links should still set it to the last
    //        { child3.Id, 1 }
    //    };

    //    // Act
    //    var result = roadmap.SetChildrenOrder(childLinks, managerId);

    //    // Assert
    //    result.IsSuccess.Should().BeTrue();
    //    roadmap.Children.First(x => x.Id == child1.Id).Order.Should().Be(2);
    //    roadmap.Children.First(x => x.Id == child2.Id).Order.Should().Be(3);
    //    roadmap.Children.First(x => x.Id == child3.Id).Order.Should().Be(1);
    //}

    //[Fact]
    //public void SetChildLinksOrder_ForAll_WhenUserIsNotManager_ShouldReturnFailure()
    //{
    //    // Arrange
    //    var roadmap = _faker.WithChildren(3);
    //    var childLinks = new Dictionary<Guid, int>();
    //    var nonManagerId = Guid.NewGuid();

    //    // Act
    //    var result = roadmap.SetChildrenOrder(childLinks, nonManagerId);

    //    // Assert
    //    result.IsFailure.Should().BeTrue();
    //    result.Error.Should().Be("User is not a roadmap manager of this roadmap.");
    //}

    //[Fact]
    //public void SetChildLinksOrder_ForAll_WhenChildLinksCountMismatch_ShouldReturnFailure()
    //{
    //    // Arrange
    //    var roadmap = _faker.WithChildren(2);
    //    var managerId = roadmap.RoadmapManagers.First().ManagerId;
    //    var childLinks = new Dictionary<Guid, int> { { Guid.NewGuid(), 1 } };

    //    // Act
    //    var result = roadmap.SetChildrenOrder(childLinks, managerId);

    //    // Assert
    //    result.IsFailure.Should().BeTrue();
    //    result.Error.Should().Be("Not all child roadmaps provided were found.");
    //}

    //[Fact]
    //public void SetChildLinksOrder_ForAll_WhenChildLinkNotFound_ShouldReturnFailure()
    //{
    //    // Arrange
    //    var roadmap = _faker.WithChildren(1);
    //    var managerId = roadmap.RoadmapManagers.First().ManagerId;
    //    var childLinks = new Dictionary<Guid, int> { { Guid.NewGuid(), 1 } };

    //    // Act
    //    var result = roadmap.SetChildrenOrder(childLinks, managerId);

    //    // Assert
    //    result.IsFailure.Should().BeTrue();
    //    result.Error.Should().Be("Not all child roadmaps provided were found.");
    //}

    //[Fact]
    //public void SetChildLinksOrder_ForOne_WhenMovingDown_ShouldReturnSuccess()
    //{
    //    // Arrange
    //    var roadmap = _faker.WithChildren(5);
    //    var managerId = roadmap.RoadmapManagers.First().ManagerId;

    //    var children = roadmap.Children.OrderBy(c => c.Order).ToList();

    //    var child1 = children[0];
    //    var child2 = children[1];
    //    var child3 = children[2];
    //    var child4 = children[3];
    //    var child5 = children[4];

    //    // Act
    //    var result = roadmap.SetChildrenOrder(child2.Id, 4, managerId);

    //    // Assert
    //    result.IsSuccess.Should().BeTrue();
    //    roadmap.Children.First(x => x.Id == child1.Id).Order.Should().Be(1);
    //    roadmap.Children.First(x => x.Id == child2.Id).Order.Should().Be(4);
    //    roadmap.Children.First(x => x.Id == child3.Id).Order.Should().Be(2);
    //    roadmap.Children.First(x => x.Id == child4.Id).Order.Should().Be(3);
    //    roadmap.Children.First(x => x.Id == child5.Id).Order.Should().Be(5);
    //}

    //[Fact]
    //public void SetChildLinksOrder_ForOne_WhenMovingUp_ShouldReturnSuccess()
    //{
    //    // Arrange
    //    var roadmap = _faker.WithChildren(5);
    //    var managerId = roadmap.RoadmapManagers.First().ManagerId;

    //    var children = roadmap.Children.OrderBy(c => c.Order).ToList();

    //    var child1 = children[0];
    //    var child2 = children[1];
    //    var child3 = children[2];
    //    var child4 = children[3];
    //    var child5 = children[4];

    //    // Act
    //    var result = roadmap.SetChildrenOrder(child4.Id, 2, managerId);

    //    // Assert
    //    result.IsSuccess.Should().BeTrue();
    //    roadmap.Children.First(x => x.Id == child1.Id).Order.Should().Be(1);
    //    roadmap.Children.First(x => x.Id == child2.Id).Order.Should().Be(3);
    //    roadmap.Children.First(x => x.Id == child3.Id).Order.Should().Be(4);
    //    roadmap.Children.First(x => x.Id == child4.Id).Order.Should().Be(2);
    //    roadmap.Children.First(x => x.Id == child5.Id).Order.Should().Be(5);
    //}

    //[Fact]
    //public void SetChildLinksOrder_ForOne_WhenUserIsNotManager_ShouldReturnFailure()
    //{
    //    // Arrange
    //    var roadmap = _faker.WithChildren(3);
    //    var children = new Dictionary<Guid, int>();
    //    var nonManagerId = Guid.NewGuid();

    //    // Act
    //    var result = roadmap.SetChildrenOrder(children, nonManagerId);

    //    // Assert
    //    result.IsFailure.Should().BeTrue();
    //    result.Error.Should().Be("User is not a roadmap manager of this roadmap.");
    //}

    //[Fact]
    //public void SetChildLinksOrder_ForOne_WhenChildLinkNotFound_ShouldReturnFailure()
    //{
    //    // Arrange
    //    var roadmap = _faker.WithChildren(3);
    //    var managerId = roadmap.RoadmapManagers.First().ManagerId;

    //    // Act
    //    var result = roadmap.SetChildrenOrder(Guid.NewGuid(), 1, managerId);

    //    // Assert
    //    result.IsFailure.Should().BeTrue();
    //    result.Error.Should().Be("Child roadmap does not exist on this roadmap.");
    //}
}
