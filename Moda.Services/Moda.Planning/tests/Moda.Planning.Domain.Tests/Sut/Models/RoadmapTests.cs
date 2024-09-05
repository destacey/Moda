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
        Assert.Equal("Roadmap must have at least one manager.", result.Error);
    }
}
