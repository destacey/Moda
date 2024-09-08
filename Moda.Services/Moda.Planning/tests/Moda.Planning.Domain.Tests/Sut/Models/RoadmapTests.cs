using Moda.Common.Domain.Enums;
using Moda.Common.Models;
using Moda.Planning.Domain.Models;
using Moda.Tests.Shared;
using NodaTime.Extensions;
using NodaTime.Testing;

namespace Moda.Planning.Domain.Tests.Sut.Models;
public class RoadmapTests
{
    private readonly TestingDateTimeProvider _dateTimeProvider;

    public RoadmapTests()
    {
        _dateTimeProvider = new(new FakeClock(DateTime.UtcNow.ToInstant()));
    }

    [Fact]
    public void Create_ValidParameters_ShouldReturnSuccess()
    {
        // Arrange
        var name = "Test Roadmap";
        var description = "Test Description";
        var dateRange = new LocalDateRange(_dateTimeProvider.Today, _dateTimeProvider.Today.PlusDays(10));
        var visibility = Visibility.Public;
        var managers = new Guid[] { Guid.NewGuid() };

        // Act
        var result = Roadmap.Create(name, description, dateRange, visibility, managers);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(name);
        result.Value.Description.Should().Be(description);
        result.Value.DateRange.Should().Be(dateRange);
        result.Value.Visibility.Should().Be(visibility);
        result.Value.Managers.Should().HaveCount(1);
        result.Value.Managers.First().ManagerId.Should().Be(managers.First());
    }

    [Fact]
    public void Create_NoManagers_ShouldReturnFailure()
    {
        // Arrange
        var name = "Test Roadmap";
        var description = "Test Description";
        var dateRange = new LocalDateRange(_dateTimeProvider.Today, _dateTimeProvider.Today.PlusDays(10));
        var visibility = Visibility.Public;
        var managers = Array.Empty<Guid>();

        // Act
        var result = Roadmap.Create(name, description, dateRange, visibility, managers);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Required input managers was empty. (Parameter 'managers')");
    }

    [Fact]
    public void Update_ValidParameters_ShouldReturnSuccess()
    {
        // Arrange
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.Create("Initial Name", "Initial Description", new LocalDateRange(_dateTimeProvider.Today, _dateTimeProvider.Today.PlusDays(10)), Visibility.Public, [managerId]).Value;
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
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.Create("Initial Name", "Initial Description", new LocalDateRange(_dateTimeProvider.Today, _dateTimeProvider.Today.PlusDays(10)), Visibility.Private, [managerId]).Value;

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
        var userEmployeeId = Guid.NewGuid();
        var initialManagers = new Guid[] { userEmployeeId };
        var roadmap = Roadmap.Create("Test Roadmap", "Test Description", new LocalDateRange(_dateTimeProvider.Today, _dateTimeProvider.Today.PlusDays(10)), Visibility.Public, initialManagers).Value;
        var managerId2 = Guid.NewGuid();
        var expectedManagers = initialManagers.Append(managerId2);

        // Act
        var result = roadmap.AddManager(managerId2, userEmployeeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        roadmap.Managers.Should().HaveCount(2);
        roadmap.Managers.Select(x => x.ManagerId).Should().BeEquivalentTo(expectedManagers);
    }

    [Fact]
    public void AddManager_DuplicateManagerId_ShouldReturnFailure()
    {
        // Arrange
        var userEmployeeId = Guid.NewGuid();
        var roadmap = Roadmap.Create("Test Roadmap", "Test Description", new LocalDateRange(_dateTimeProvider.Today, _dateTimeProvider.Today.PlusDays(10)), Visibility.Public, [userEmployeeId]).Value;

        // Act
        var result = roadmap.AddManager(userEmployeeId, userEmployeeId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Roadmap manager already exists on this roadmap.");
        roadmap.Managers.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveManager_ValidManagerId_ShouldReturnSuccess()
    {
        // Arrange
        var userEmployeeId = Guid.NewGuid();
        var managerId2 = Guid.NewGuid();
        var roadmap = Roadmap.Create("Test Roadmap", "Test Description", new LocalDateRange(_dateTimeProvider.Today, _dateTimeProvider.Today.PlusDays(10)), Visibility.Public, [userEmployeeId, managerId2]).Value;

        // Act
        var result = roadmap.RemoveManager(managerId2, userEmployeeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        roadmap.Managers.Should().HaveCount(1);
        roadmap.Managers.First().ManagerId.Should().Be(userEmployeeId);
    }

    [Fact]
    public void RemoveManager_InvalidManagerId_ShouldReturnFailure()
    {
        // Arrange
        var userEmployeeId = Guid.NewGuid();
        var roadmap = Roadmap.Create("Test Roadmap", "Test Description", new LocalDateRange(_dateTimeProvider.Today, _dateTimeProvider.Today.PlusDays(10)), Visibility.Public, [userEmployeeId]).Value;

        // Act
        var result = roadmap.RemoveManager(Guid.NewGuid(), userEmployeeId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Roadmap manager does not exist on this roadmap.");
    }

    [Fact]
    public void RemoveManager_LastManager_ShouldReturnFailure()
    {
        // Arrange
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.Create("Test Roadmap", "Test Description", new LocalDateRange(_dateTimeProvider.Today, _dateTimeProvider.Today.PlusDays(10)), Visibility.Public, [managerId]).Value;

        // Act
        var result = roadmap.RemoveManager(managerId, managerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Roadmap must have at least one manager.");
    }

    [Fact]
    public void AddChildLink_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var roadmap = Roadmap.Create("Test Roadmap", "Test Description", new LocalDateRange(_dateTimeProvider.Today, _dateTimeProvider.Today.PlusDays(10)), Visibility.Public, [Guid.NewGuid()]).Value;
        var childId = Guid.NewGuid();
        var currentUserEmployeeId = roadmap.Managers.First().ManagerId;

        // Act
        var result = roadmap.AddChildLink(childId, currentUserEmployeeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        roadmap.ChildLinks.Should().Contain(link => link.ChildId == childId);
        roadmap.ChildLinks.Count.Should().Be(1);
    }

    [Fact]
    public void SetChildLinkOrder_WithMultiple_ShouldReturnCorrectOrder()
    {
        // Arrange
        var roadmap = Roadmap.Create("Test Roadmap", "Test Description", new LocalDateRange(_dateTimeProvider.Today, _dateTimeProvider.Today.PlusDays(10)), Visibility.Public, [Guid.NewGuid()]).Value;
        var currentUserEmployeeId = roadmap.Managers.First().ManagerId;

        var childLink1Id = Guid.NewGuid();
        var childLink2Id = Guid.NewGuid();
        var childLink3Id = Guid.NewGuid();

        // Act
        var result1 = roadmap.AddChildLink(childLink1Id, currentUserEmployeeId);
        var result2 = roadmap.AddChildLink(childLink2Id, currentUserEmployeeId);
        var result3 = roadmap.AddChildLink(childLink3Id, currentUserEmployeeId);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        roadmap.ChildLinks.First(x => x.ChildId == childLink1Id).Order.Should().Be(1);
        result2.IsSuccess.Should().BeTrue();
        roadmap.ChildLinks.First(x => x.ChildId == childLink2Id).Order.Should().Be(2);
        result3.IsSuccess.Should().BeTrue();
        roadmap.ChildLinks.First(x => x.ChildId == childLink3Id).Order.Should().Be(3);
    }

    [Fact]
    public void AddChildLink_ShouldReturnFailure_WhenChildLinkAlreadyExists()
    {
        // Arrange
        var roadmap = Roadmap.Create("Test Roadmap", "Test Description", new LocalDateRange(_dateTimeProvider.Today, _dateTimeProvider.Today.PlusDays(10)), Visibility.Public, [Guid.NewGuid()]).Value;
        var childId = Guid.NewGuid();
        var currentUserEmployeeId = roadmap.Managers.First().ManagerId;
        roadmap.AddChildLink(childId, currentUserEmployeeId);

        // Act
        var result = roadmap.AddChildLink(childId, currentUserEmployeeId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Child Roadmap already exists on this roadmap.");
        roadmap.ChildLinks.Count.Should().Be(1);
    }

    [Fact]
    public void RemoveChildLink_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var roadmap = Roadmap.Create("Test Roadmap", "Test Description", new LocalDateRange(_dateTimeProvider.Today, _dateTimeProvider.Today.PlusDays(10)), Visibility.Public, [Guid.NewGuid()]).Value;
        var childId = Guid.NewGuid();
        var currentUserEmployeeId = roadmap.Managers.First().ManagerId;
        roadmap.AddChildLink(childId, currentUserEmployeeId);

        // Act
        var result = roadmap.RemoveChildLink(childId, currentUserEmployeeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        roadmap.ChildLinks.Count.Should().Be(0);
    }

    [Fact]
    public void RemoveChildLink_WhenChildLinkDoesNotExist_ShouldReturnFailure()
    {
        // Arrange
        var roadmap = Roadmap.Create("Test Roadmap", "Test Description", new LocalDateRange(_dateTimeProvider.Today, _dateTimeProvider.Today.PlusDays(10)), Visibility.Public, [Guid.NewGuid()]).Value;
        var childId = Guid.NewGuid();
        var currentUserEmployeeId = roadmap.Managers.First().ManagerId;
        roadmap.AddChildLink(Guid.NewGuid(), currentUserEmployeeId);

        // Act
        var result = roadmap.RemoveChildLink(childId, currentUserEmployeeId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Child Roadmap does not exist on this roadmap.");
        roadmap.ChildLinks.Count.Should().Be(1);
    }

    [Fact]
    public void SetChildLinksOrder_ForAll_WhenValidChildLinksProvided_ShouldReturnSuccess()
    {
        // Arrange
        var roadmap = Roadmap.Create("Test Roadmap", "Test Description", new LocalDateRange(_dateTimeProvider.Today, _dateTimeProvider.Today.PlusDays(10)), Visibility.Public, [Guid.NewGuid()]).Value;
        var currentUserEmployeeId = roadmap.Managers.First().ManagerId;

        var childLink1Id = Guid.NewGuid();
        roadmap.AddChildLink(childLink1Id, currentUserEmployeeId);

        var childLink2Id = Guid.NewGuid();
        roadmap.AddChildLink(childLink2Id, currentUserEmployeeId);

        var childLink3Id = Guid.NewGuid();
        roadmap.AddChildLink(childLink3Id, currentUserEmployeeId);

        var childLinks = new Dictionary<Guid, int> 
        { 
            { childLink1Id, 2 }, 
            { childLink2Id, 17 }, // setting the higher order than the count of child links should still set it to the last
            { childLink3Id, 1 } 
        };

        // Act
        var result = roadmap.SetChildLinksOrder(childLinks, currentUserEmployeeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        roadmap.ChildLinks.First(x => x.ChildId == childLink1Id).Order.Should().Be(2);
        roadmap.ChildLinks.First(x => x.ChildId == childLink2Id).Order.Should().Be(3);
        roadmap.ChildLinks.First(x => x.ChildId == childLink3Id).Order.Should().Be(1);
    }

    [Fact]
    public void SetChildLinksOrder_ForAll_WhenUserIsNotManager_ShouldReturnFailure()
    {
        // Arrange
        var roadmap = Roadmap.Create("Test Roadmap", "Test Description", new LocalDateRange(_dateTimeProvider.Today, _dateTimeProvider.Today.PlusDays(10)), Visibility.Public, [Guid.NewGuid()]).Value;
        var childLinks = new Dictionary<Guid, int>();
        var nonManagerId = Guid.NewGuid();

        // Act
        var result = roadmap.SetChildLinksOrder(childLinks, nonManagerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("User is not a manager of this roadmap.");
    }

    [Fact]
    public void SetChildLinksOrder_ForAll_WhenChildLinksCountMismatch_ShouldReturnFailure()
    {
        // Arrange
        var roadmap = Roadmap.Create("Test Roadmap", "Test Description", new LocalDateRange(_dateTimeProvider.Today, _dateTimeProvider.Today.PlusDays(10)), Visibility.Public, [Guid.NewGuid()]).Value;
        var childLinks = new Dictionary<Guid, int> { { Guid.NewGuid(), 1 } };
        var currentUserEmployeeId = roadmap.Managers.First().ManagerId;

        // Act
        var result = roadmap.SetChildLinksOrder(childLinks, currentUserEmployeeId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Not all child roadmap links provided were found.");
    }

    [Fact]
    public void SetChildLinksOrder_ForAll_WhenChildLinkNotFound_ShouldReturnFailure()
    {
        // Arrange
        var roadmap = Roadmap.Create("Test Roadmap", "Test Description", new LocalDateRange(_dateTimeProvider.Today, _dateTimeProvider.Today.PlusDays(10)), Visibility.Public, [Guid.NewGuid()]).Value;
        var childLinkId = Guid.NewGuid();
        roadmap.AddChildLink(childLinkId, roadmap.Managers.First().ManagerId);
        var childLinks = new Dictionary<Guid, int> { { Guid.NewGuid(), 1 } };
        var currentUserEmployeeId = roadmap.Managers.First().ManagerId;

        // Act
        var result = roadmap.SetChildLinksOrder(childLinks, currentUserEmployeeId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Not all child roadmap links provided were found.");
    }

    [Fact]
    public void SetChildLinksOrder_ForOne_WhenMovingDown_ShouldReturnSuccess()
    {
        // Arrange
        var roadmap = Roadmap.Create("Test Roadmap", "Test Description", new LocalDateRange(_dateTimeProvider.Today, _dateTimeProvider.Today.PlusDays(10)), Visibility.Public, [Guid.NewGuid()]).Value;
        var currentUserEmployeeId = roadmap.Managers.First().ManagerId;

        var childLink1Id = Guid.NewGuid();
        roadmap.AddChildLink(childLink1Id, currentUserEmployeeId);

        var childLink2Id = Guid.NewGuid();
        roadmap.AddChildLink(childLink2Id, currentUserEmployeeId);

        var childLink3Id = Guid.NewGuid();
        roadmap.AddChildLink(childLink3Id, currentUserEmployeeId);

        var childLink4Id = Guid.NewGuid();
        roadmap.AddChildLink(childLink4Id, currentUserEmployeeId);

        var childLink5Id = Guid.NewGuid();
        roadmap.AddChildLink(childLink5Id, currentUserEmployeeId);

        // Act
        var result = roadmap.SetChildLinksOrder(childLink2Id, 4, currentUserEmployeeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        roadmap.ChildLinks.First(x => x.ChildId == childLink1Id).Order.Should().Be(1);
        roadmap.ChildLinks.First(x => x.ChildId == childLink2Id).Order.Should().Be(4);
        roadmap.ChildLinks.First(x => x.ChildId == childLink3Id).Order.Should().Be(2);
        roadmap.ChildLinks.First(x => x.ChildId == childLink4Id).Order.Should().Be(3);
        roadmap.ChildLinks.First(x => x.ChildId == childLink5Id).Order.Should().Be(5);
    }

    [Fact]
    public void SetChildLinksOrder_ForOne_WhenMovingUp_ShouldReturnSuccess()
    {
        // Arrange
        var roadmap = Roadmap.Create("Test Roadmap", "Test Description", new LocalDateRange(_dateTimeProvider.Today, _dateTimeProvider.Today.PlusDays(10)), Visibility.Public, [Guid.NewGuid()]).Value;
        var currentUserEmployeeId = roadmap.Managers.First().ManagerId;

        var childLink1Id = Guid.NewGuid();
        roadmap.AddChildLink(childLink1Id, currentUserEmployeeId);

        var childLink2Id = Guid.NewGuid();
        roadmap.AddChildLink(childLink2Id, currentUserEmployeeId);

        var childLink3Id = Guid.NewGuid();
        roadmap.AddChildLink(childLink3Id, currentUserEmployeeId);

        var childLink4Id = Guid.NewGuid();
        roadmap.AddChildLink(childLink4Id, currentUserEmployeeId);

        var childLink5Id = Guid.NewGuid();
        roadmap.AddChildLink(childLink5Id, currentUserEmployeeId);

        // Act
        var result = roadmap.SetChildLinksOrder(childLink4Id, 2, currentUserEmployeeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        roadmap.ChildLinks.First(x => x.ChildId == childLink1Id).Order.Should().Be(1);
        roadmap.ChildLinks.First(x => x.ChildId == childLink2Id).Order.Should().Be(3);
        roadmap.ChildLinks.First(x => x.ChildId == childLink3Id).Order.Should().Be(4);
        roadmap.ChildLinks.First(x => x.ChildId == childLink4Id).Order.Should().Be(2);
        roadmap.ChildLinks.First(x => x.ChildId == childLink5Id).Order.Should().Be(5);
    }

    [Fact]
    public void SetChildLinksOrder_ForOne_WhenUserIsNotManager_ShouldReturnFailure()
    {
        // Arrange
        var roadmap = Roadmap.Create("Test Roadmap", "Test Description", new LocalDateRange(_dateTimeProvider.Today, _dateTimeProvider.Today.PlusDays(10)), Visibility.Public, [Guid.NewGuid()]).Value;
        var childLinks = new Dictionary<Guid, int>();
        var nonManagerId = Guid.NewGuid();

        // Act
        var result = roadmap.SetChildLinksOrder(childLinks, nonManagerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("User is not a manager of this roadmap.");
    }

    [Fact]
    public void SetChildLinksOrder_ForOne_WhenChildLinkNotFound_ShouldReturnFailure()
    {
        // Arrange
        var roadmap = Roadmap.Create("Test Roadmap", "Test Description", new LocalDateRange(_dateTimeProvider.Today, _dateTimeProvider.Today.PlusDays(10)), Visibility.Public, [Guid.NewGuid()]).Value;
        var childLinkId = Guid.NewGuid();
        roadmap.AddChildLink(childLinkId, roadmap.Managers.First().ManagerId);
        var currentUserEmployeeId = roadmap.Managers.First().ManagerId;

        // Act
        var result = roadmap.SetChildLinksOrder(Guid.NewGuid(), 1, currentUserEmployeeId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Child roadmap link does not exist on this roadmap.");
    }
}
