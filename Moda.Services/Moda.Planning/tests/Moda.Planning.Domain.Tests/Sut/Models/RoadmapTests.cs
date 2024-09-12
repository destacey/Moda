﻿using Moda.Common.Domain.Enums;
using Moda.Common.Models;
using Moda.Planning.Domain.Models;
using Moda.Planning.Domain.Tests.Data;
using Moda.Tests.Shared;
using NodaTime.Extensions;
using NodaTime.Testing;

namespace Moda.Planning.Domain.Tests.Sut.Models;
public class RoadmapTests
{
    private readonly TestingDateTimeProvider _dateTimeProvider;
    private readonly RoadmapFaker _faker;

    public RoadmapTests()
    {
        _dateTimeProvider = new(new FakeClock(DateTime.UtcNow.ToInstant()));
        _faker = new RoadmapFaker(_dateTimeProvider.Today);
    }

    [Fact]
    public void Create_ValidParameters_ShouldReturnSuccess()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();

        // Act
        var result = Roadmap.CreateRoot(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(fakeRoadmap.Name);
        result.Value.Description.Should().Be(fakeRoadmap.Description);
        result.Value.DateRange.Should().Be(fakeRoadmap.DateRange);
        result.Value.Visibility.Should().Be(fakeRoadmap.Visibility);
        result.Value.Managers.Should().HaveCount(1);
        result.Value.Managers.First().ManagerId.Should().Be(managerId);
        result.Value.ParentId.Should().BeNull();
        result.Value.Order.Should().BeNull();
    }

    [Fact]
    public void Create_NoManagers_ShouldReturnFailure()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managers = Array.Empty<Guid>();

        // Act
        var result = Roadmap.CreateRoot(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, managers);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Required input managers was empty. (Parameter 'managers')");
    }

    [Fact]
    public void Update_ValidParameters_ShouldReturnSuccess()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.CreateRoot(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;
        
        var newName = "Updated Name";
        var newDescription = "Updated Description";
        var newDateRange = new LocalDateRange(_dateTimeProvider.Today.PlusDays(1), _dateTimeProvider.Today.PlusDays(11));
        var newVisibility = Visibility.Private;

        // Act
        var result = roadmap.Update(newName, newDescription, newDateRange, newVisibility, managerId);

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
        var roadmap = Roadmap.CreateRoot(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;

        // Act
        var result = roadmap.Update("Updated Name", "Updated Description", new LocalDateRange(_dateTimeProvider.Today.PlusDays(1), _dateTimeProvider.Today.PlusDays(11)), Visibility.Private, Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("User is not a manager of this roadmap.");
    }

    [Fact]
    public void AddManager_ValidManagerId_ShouldReturnSuccess()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.CreateRoot(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;

        var initialManagers = new Guid[] { managerId };
        var managerId2 = Guid.NewGuid();
        var expectedManagers = initialManagers.Append(managerId2);

        // Act
        var result = roadmap.AddManager(managerId2, managerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        roadmap.Managers.Should().HaveCount(2);
        roadmap.Managers.Select(x => x.ManagerId).Should().BeEquivalentTo(expectedManagers);
    }

    [Fact]
    public void AddManager_DuplicateManagerId_ShouldReturnFailure()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.CreateRoot(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;

        // Act
        var result = roadmap.AddManager(managerId, managerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Roadmap manager already exists on this roadmap.");
        roadmap.Managers.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveManager_ValidManagerId_ShouldReturnSuccess()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var managerId2 = Guid.NewGuid();
        var roadmap = Roadmap.CreateRoot(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId, managerId2]).Value;

        // Act
        var result = roadmap.RemoveManager(managerId2, managerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        roadmap.Managers.Should().HaveCount(1);
        roadmap.Managers.First().ManagerId.Should().Be(managerId);
    }

    [Fact]
    public void RemoveManager_InvalidManagerId_ShouldReturnFailure()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.CreateRoot(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;

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
        var roadmap = Roadmap.CreateRoot(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;

        // Act
        var result = roadmap.RemoveManager(managerId, managerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Roadmap must have at least one manager.");
    }

    [Fact]
    public void CreateChild_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.CreateRoot(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;

        var fakeChildRoadmap = _faker.Generate();

        // Act
        var result = roadmap.CreateChild(fakeChildRoadmap.Name, fakeChildRoadmap.Description, fakeChildRoadmap.DateRange, fakeChildRoadmap.Visibility, [managerId], managerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        roadmap.Children.Should().Contain(child => child.Name == fakeChildRoadmap.Name);
        roadmap.Children.Count.Should().Be(1);
        roadmap.Children.First().Order.Should().Be(1);
    }

    [Fact]
    public void CreateChild_WithMultiple_ShouldReturnCorrectOrder()
    {
        // Arrange
        var roadmap = _faker.WithChildren(2);
        var managerId = roadmap.Managers.First().ManagerId;
        var fakeChildRoadmap = _faker.Generate();

        // Act
        var result3 = roadmap.CreateChild(fakeChildRoadmap.Name, fakeChildRoadmap.Description, fakeChildRoadmap.DateRange, fakeChildRoadmap.Visibility, [managerId], managerId);

        // Assert
        result3.IsSuccess.Should().BeTrue();
        roadmap.Children.First(x => x.Name == fakeChildRoadmap.Name).Order.Should().Be(3);
    }

    [Fact]
    public void RemoveChildLink_WhenValid_ShouldReturnSuccess()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.CreateRoot(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;

        var fakeChildRoadmap = _faker.Generate();
        var child = roadmap.CreateChild(fakeChildRoadmap.Name, fakeChildRoadmap.Description, fakeChildRoadmap.DateRange, fakeChildRoadmap.Visibility, [managerId], managerId).Value;

        // Act
        var result = roadmap.RemoveChild(child.Id, managerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        roadmap.Children.Count.Should().Be(0);
    }

    [Fact]
    public void RemoveChildLink_WhenChildLinkDoesNotExist_ShouldReturnFailure()
    {
        // Arrange
        var fakeRoadmap = _faker.Generate();
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.CreateRoot(fakeRoadmap.Name, fakeRoadmap.Description, fakeRoadmap.DateRange, fakeRoadmap.Visibility, [managerId]).Value;

        var fakeChildRoadmap = _faker.Generate();
        var child = roadmap.CreateChild(fakeChildRoadmap.Name, fakeChildRoadmap.Description, fakeChildRoadmap.DateRange, fakeChildRoadmap.Visibility, [managerId], managerId).Value;

        // Act
        var result = roadmap.RemoveChild(Guid.NewGuid(), managerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Child Roadmap does not exist on this roadmap.");
        roadmap.Children.Count.Should().Be(1);
    }

    [Fact]
    public void SetChildrenOrder_ForAll_WhenValidChildrenProvided_ShouldReturnSuccess()
    {
        // Arrange
        var roadmap = _faker.WithChildren(3);
        var managerId = roadmap.Managers.First().ManagerId;

        var children = roadmap.Children.OrderBy(c => c.Order).ToList();

        var child1 = children[0];
        var child2 = children[1];
        var child3 = children[2];

        var childLinks = new Dictionary<Guid, int>
        {
            { child1.Id, 2 },
            { child2.Id, 17 }, // setting the higher order than the count of child links should still set it to the last
            { child3.Id, 1 }
        };

        // Act
        var result = roadmap.SetChildrenOrder(childLinks, managerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        roadmap.Children.First(x => x.Id == child1.Id).Order.Should().Be(2);
        roadmap.Children.First(x => x.Id == child2.Id).Order.Should().Be(3);
        roadmap.Children.First(x => x.Id == child3.Id).Order.Should().Be(1);
    }

    [Fact]
    public void SetChildLinksOrder_ForAll_WhenUserIsNotManager_ShouldReturnFailure()
    {
        // Arrange
        var roadmap = _faker.WithChildren(3);
        var childLinks = new Dictionary<Guid, int>();
        var nonManagerId = Guid.NewGuid();

        // Act
        var result = roadmap.SetChildrenOrder(childLinks, nonManagerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("User is not a manager of this roadmap.");
    }

    [Fact]
    public void SetChildLinksOrder_ForAll_WhenChildLinksCountMismatch_ShouldReturnFailure()
    {
        // Arrange
        var roadmap = _faker.WithChildren(2);
        var managerId = roadmap.Managers.First().ManagerId;
        var childLinks = new Dictionary<Guid, int> { { Guid.NewGuid(), 1 } };

        // Act
        var result = roadmap.SetChildrenOrder(childLinks, managerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Not all child roadmaps provided were found.");
    }

    [Fact]
    public void SetChildLinksOrder_ForAll_WhenChildLinkNotFound_ShouldReturnFailure()
    {
        // Arrange
        var roadmap = _faker.WithChildren(1);
        var managerId = roadmap.Managers.First().ManagerId;
        var childLinks = new Dictionary<Guid, int> { { Guid.NewGuid(), 1 } };

        // Act
        var result = roadmap.SetChildrenOrder(childLinks, managerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Not all child roadmaps provided were found.");
    }

    [Fact]
    public void SetChildLinksOrder_ForOne_WhenMovingDown_ShouldReturnSuccess()
    {
        // Arrange
        var roadmap = _faker.WithChildren(5);
        var managerId = roadmap.Managers.First().ManagerId;

        var children = roadmap.Children.OrderBy(c => c.Order).ToList();

        var child1 = children[0];
        var child2 = children[1];
        var child3 = children[2];
        var child4 = children[3];
        var child5 = children[4];

        // Act
        var result = roadmap.SetChildrenOrder(child2.Id, 4, managerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        roadmap.Children.First(x => x.Id == child1.Id).Order.Should().Be(1);
        roadmap.Children.First(x => x.Id == child2.Id).Order.Should().Be(4);
        roadmap.Children.First(x => x.Id == child3.Id).Order.Should().Be(2);
        roadmap.Children.First(x => x.Id == child4.Id).Order.Should().Be(3);
        roadmap.Children.First(x => x.Id == child5.Id).Order.Should().Be(5);
    }

    [Fact]
    public void SetChildLinksOrder_ForOne_WhenMovingUp_ShouldReturnSuccess()
    {
        // Arrange
        var roadmap = _faker.WithChildren(5);
        var managerId = roadmap.Managers.First().ManagerId;

        var children = roadmap.Children.OrderBy(c => c.Order).ToList();

        var child1 = children[0];
        var child2 = children[1];
        var child3 = children[2];
        var child4 = children[3];
        var child5 = children[4];

        // Act
        var result = roadmap.SetChildrenOrder(child4.Id, 2, managerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        roadmap.Children.First(x => x.Id == child1.Id).Order.Should().Be(1);
        roadmap.Children.First(x => x.Id == child2.Id).Order.Should().Be(3);
        roadmap.Children.First(x => x.Id == child3.Id).Order.Should().Be(4);
        roadmap.Children.First(x => x.Id == child4.Id).Order.Should().Be(2);
        roadmap.Children.First(x => x.Id == child5.Id).Order.Should().Be(5);
    }

    [Fact]
    public void SetChildLinksOrder_ForOne_WhenUserIsNotManager_ShouldReturnFailure()
    {
        // Arrange
        var roadmap = _faker.WithChildren(3);
        var children = new Dictionary<Guid, int>();
        var nonManagerId = Guid.NewGuid();

        // Act
        var result = roadmap.SetChildrenOrder(children, nonManagerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("User is not a manager of this roadmap.");
    }

    [Fact]
    public void SetChildLinksOrder_ForOne_WhenChildLinkNotFound_ShouldReturnFailure()
    {
        // Arrange
        var roadmap = _faker.WithChildren(3);
        var managerId = roadmap.Managers.First().ManagerId;

        // Act
        var result = roadmap.SetChildrenOrder(Guid.NewGuid(), 1, managerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Child roadmap does not exist on this roadmap.");
    }
}