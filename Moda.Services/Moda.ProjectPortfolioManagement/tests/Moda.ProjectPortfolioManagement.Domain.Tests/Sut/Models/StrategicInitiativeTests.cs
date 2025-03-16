using FluentAssertions;
using Moda.Common.Models;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.ProjectPortfolioManagement.Domain.Models;
using Moda.ProjectPortfolioManagement.Domain.Tests.Data;
using Moda.Tests.Shared;
using NodaTime.Extensions;
using NodaTime.Testing;

namespace Moda.ProjectPortfolioManagement.Domain.Tests.Sut.Models;

public sealed class StrategicInitiativeTests
{
    private readonly TestingDateTimeProvider _dateTimeProvider;
    private readonly StrategicInitiativeFaker _strategicInitiativeFaker;

    public StrategicInitiativeTests()
    {
        _dateTimeProvider = new TestingDateTimeProvider(new FakeClock(DateTime.UtcNow.ToInstant()));
        _strategicInitiativeFaker = new StrategicInitiativeFaker(_dateTimeProvider);
    }

    [Fact]
    public void Create_ShouldCreateStrategicInitiativeSuccessfully()
    {
        // Arrange
        var expected = _strategicInitiativeFaker.Generate();

        // Act
        var initiative = StrategicInitiative.Create(expected.Name, expected.Description, expected.DateRange, expected.PortfolioId);

        // Assert
        initiative.Should().NotBeNull();
        initiative.Name.Should().Be(expected.Name);
        initiative.Description.Should().Be(expected.Description);
        initiative.Status.Should().Be(StrategicInitiativeStatus.Proposed);
        initiative.DateRange.Should().Be(expected.DateRange);
        initiative.PortfolioId.Should().Be(expected.PortfolioId);
        initiative.Roles.Should().BeEmpty();
        initiative.Kpis.Should().BeEmpty();
        initiative.StrategicInitiativeProjects.Should().BeEmpty();
    }

    [Theory]
    [InlineData(StrategicInitiativeStatus.Proposed, true)]
    [InlineData(StrategicInitiativeStatus.Approved, true)]
    [InlineData(StrategicInitiativeStatus.Active, false)]
    [InlineData(StrategicInitiativeStatus.Completed, false)]
    [InlineData(StrategicInitiativeStatus.Cancelled, false)]
    public void CanBeDeleted_ShouldReturnExpectedBasedOnStatus(StrategicInitiativeStatus status, bool expected)
    {
        // Arrange
        var initiative = _strategicInitiativeFaker.WithData(status: status).Generate();

        // Act & Assert
        initiative.CanBeDeleted().Should().Be(expected);
    }

    [Theory]
    [InlineData(StrategicInitiativeStatus.Completed, true)]
    [InlineData(StrategicInitiativeStatus.Cancelled, true)]
    [InlineData(StrategicInitiativeStatus.Proposed, false)]
    [InlineData(StrategicInitiativeStatus.Approved, false)]
    [InlineData(StrategicInitiativeStatus.Active, false)]
    public void IsClosed_ShouldReturnExpectedBasedOnStatus(StrategicInitiativeStatus status, bool expected)
    {
        // Arrange
        var initiative = _strategicInitiativeFaker.WithData(status: status).Generate();

        // Act & Assert
        initiative.IsClosed.Should().Be(expected);
    }
}
