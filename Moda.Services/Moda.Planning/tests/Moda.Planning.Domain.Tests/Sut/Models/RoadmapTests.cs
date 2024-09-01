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
        var isPublic = true;
        var managers = new Guid[] { Guid.NewGuid() };

        // Act
        var result = Roadmap.Create(name, description, dateRange, isPublic, managers);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(name);
        result.Value.Description.Should().Be(description);
        result.Value.DateRange.Should().Be(dateRange);
        result.Value.IsPublic.Should().Be(isPublic);
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
        var isPublic = true;
        var managers = Array.Empty<Guid>();

        // Act
        var result = Roadmap.Create(name, description, dateRange, isPublic, managers);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Required input managers was empty. (Parameter 'managers')");
    }

    [Fact]
    public void Update_ValidParameters_ShouldReturnSuccess()
    {
        // Arrange
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.Create("Initial Name", "Initial Description", new LocalDateRange(_dateTimeProvider.Today, _dateTimeProvider.Today.PlusDays(10)), false, [managerId]).Value;
        var newName = "Updated Name";
        var newDescription = "Updated Description";
        var newDateRange = new LocalDateRange(_dateTimeProvider.Today.PlusDays(1), _dateTimeProvider.Today.PlusDays(11));
        var newIsPublic = true;

        // Act
        var result = roadmap.Update(newName, newDescription, newDateRange, newIsPublic, managerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        roadmap.Name.Should().Be(newName);
        roadmap.Description.Should().Be(newDescription);
        roadmap.DateRange.Should().Be(newDateRange);
        roadmap.IsPublic.Should().Be(newIsPublic);
    }

    [Fact]
    public void Update_InvalidManagerId_ShouldReturnFailure()
    {
        // Arrange
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.Create("Initial Name", "Initial Description", new LocalDateRange(_dateTimeProvider.Today, _dateTimeProvider.Today.PlusDays(10)), false, [managerId]).Value;

        // Act
        var result = roadmap.Update("Updated Name", "Updated Description", new LocalDateRange(_dateTimeProvider.Today.PlusDays(1), _dateTimeProvider.Today.PlusDays(11)), true, Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("User is not a manager of this roadmap.");
    }


    [Fact]
    public void AddManager_ValidManagerId_ShouldReturnSuccess()
    {
        // Arrange
        var initialManagers = new Guid[] { Guid.NewGuid() };
        var roadmap = Roadmap.Create("Test Roadmap", "Test Description", new LocalDateRange(_dateTimeProvider.Today, _dateTimeProvider.Today.PlusDays(10)), true, initialManagers).Value;
        var managerId = Guid.NewGuid();
        var expectedManagers = initialManagers.Append(managerId);

        // Act
        var result = roadmap.AddManager(managerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        roadmap.Managers.Should().HaveCount(2);
        roadmap.Managers.Select(x => x.ManagerId).Should().BeEquivalentTo(expectedManagers);
    }

    [Fact]
    public void AddManager_DuplicateManagerId_ShouldReturnFailure()
    {
        // Arrange
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.Create("Test Roadmap", "Test Description", new LocalDateRange(_dateTimeProvider.Today, _dateTimeProvider.Today.PlusDays(10)), true, [managerId]).Value;

        // Act
        var result = roadmap.AddManager(managerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Roadmap manager already exists on this roadmap.");
        roadmap.Managers.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveManager_ValidManagerId_ShouldReturnSuccess()
    {
        // Arrange
        var managerId1 = Guid.NewGuid();
        var managerId2 = Guid.NewGuid();
        var roadmap = Roadmap.Create("Test Roadmap", "Test Description", new LocalDateRange(_dateTimeProvider.Today, _dateTimeProvider.Today.PlusDays(10)), true, [managerId1, managerId2]).Value;

        // Act
        var result = roadmap.RemoveManager(managerId2);

        // Assert
        result.IsSuccess.Should().BeTrue();
        roadmap.Managers.Should().HaveCount(1);
        roadmap.Managers.First().ManagerId.Should().Be(managerId1);
    }

    [Fact]
    public void RemoveManager_InvalidManagerId_ShouldReturnFailure()
    {
        // Arrange
        var roadmap = Roadmap.Create("Test Roadmap", "Test Description", new LocalDateRange(_dateTimeProvider.Today, _dateTimeProvider.Today.PlusDays(10)), true, [Guid.NewGuid()]).Value;

        // Act
        var result = roadmap.RemoveManager(Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Roadmap manager does not exist on this roadmap.");
    }

    [Fact]
    public void RemoveManager_LastManager_ShouldReturnFailure()
    {
        // Arrange
        var managerId = Guid.NewGuid();
        var roadmap = Roadmap.Create("Test Roadmap", "Test Description", new LocalDateRange(_dateTimeProvider.Today, _dateTimeProvider.Today.PlusDays(10)), true, [managerId]).Value;

        // Act
        var result = roadmap.RemoveManager(managerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        Assert.Equal("Roadmap must have at least one manager.", result.Error);
    }
}
