using Moda.Common.Domain.Events.Health;
using Moda.Health.Tests.Data;
using Moda.Tests.Shared;
using NodaTime.Extensions;
using NodaTime.Testing;

namespace Moda.Health.Tests.Sut.Models;
public class HealthReportTests
{
    private readonly TestingDateTimeProvider _dateTimeProvider;

    public HealthReportTests()
    {
        // TODO: Replace with a FakeClock that can be injected into the constructor.
        _dateTimeProvider = new(new FakeClock(DateTime.UtcNow.ToInstant()));
    }

    #region Constructor

    [Fact]
    public void Constructor_WhenValid_ReturnsHealthReport()
    {
        // Arrange
        var objectId = Guid.NewGuid();
        var healthChecks = new HealthCheckFaker(_dateTimeProvider.Now, objectId)
            .MultipleWithSameObjectId(objectId, 5);

        // Act
        var result = new HealthReport(healthChecks);

        // Assert
        result.Should().NotBeNull();
        result.HealthChecks.Should().NotBeEmpty();
        result.HealthChecks.Should().HaveCount(5);
    }

    #endregion Constructor

    #region AddHealthCheck

    [Fact]
    public void AddHealthCheck_WhenValid_ReturnsHealthCheck()
    {
        // Arrange
        var objectId = Guid.NewGuid();
        var healthChecks = new HealthCheckFaker(_dateTimeProvider.Now.Minus(Duration.FromDays(30)), objectId)
            .MultipleWithSameObjectId(objectId, 2);
        var reportedById = Guid.NewGuid();
        var reportedOn = _dateTimeProvider.Now;
        var expiration = reportedOn.Plus(Duration.FromDays(3));
        var healthReport = new HealthReport(healthChecks);

        // Act
        var result = healthReport.AddHealthCheck(objectId, SystemContext.PlanningPlanningIntervalObjective, HealthStatus.Healthy, reportedById, reportedOn, expiration, "Test");

        // Assert
        result.Should().NotBeNull();
        healthReport.HealthChecks.Count.Should().Be(3);
        result.ObjectId.Should().Be(objectId);
        result.Context.Should().Be(SystemContext.PlanningPlanningIntervalObjective);
        result.Status.Should().Be(HealthStatus.Healthy);
        result.ReportedOn.Should().Be(reportedOn);
        result.Expiration.Should().Be(expiration);
        result.Note.Should().Be("Test");

        result.PostPersistenceActions.Should().NotBeEmpty();

        // get the first action and execute it to ensure it's a valid HealthCheckCreatedEvent
        var action = result.PostPersistenceActions.First();
        action.Should().NotBeNull();
        action();
        result.DomainEvents.Should().NotBeEmpty();
        result.DomainEvents.Should().ContainSingle(e => e is HealthCheckCreatedEvent);

        // get the event and verify its properties
        var createdEvent = result.DomainEvents.OfType<HealthCheckCreatedEvent>().First();
        createdEvent.Id.Should().Be(result.Id);
        createdEvent.ObjectId.Should().Be(objectId);
        createdEvent.Context.Should().Be(SystemContext.PlanningPlanningIntervalObjective);
        createdEvent.Status.Should().Be(HealthStatus.Healthy);
        createdEvent.ReportedById.Should().Be(reportedById);
        createdEvent.ReportedOn.Should().Be(reportedOn);
        createdEvent.Expiration.Should().Be(expiration);
    }

    [Fact]
    public void AddHealthCheck_WhenHealthCheckOverlaps_ReturnsHealthCheck()
    {
        // Arrange
        var objectId = Guid.NewGuid();
        var healthChecks = new HealthCheckFaker(_dateTimeProvider.Now.Minus(Duration.FromDays(8)), objectId)
            .MultipleWithSameObjectId(objectId, 2);
        var reportedById = Guid.NewGuid();
        var reportedOn = _dateTimeProvider.Now;
        var expiration = reportedOn.Plus(Duration.FromDays(3));
        var healthReport = new HealthReport(healthChecks);

        // Act
        var result = healthReport.AddHealthCheck(objectId, SystemContext.PlanningPlanningIntervalObjective, HealthStatus.Healthy, reportedById, reportedOn, expiration, "Test");

        var previous = healthReport.HealthChecks.OrderByDescending(h => h.ReportedOn).ToList()[1];

        // Assert
        result.Should().NotBeNull();
        healthReport.HealthChecks.Count.Should().Be(3);
        previous.Expiration.Should().Be(_dateTimeProvider.Now);
        result.ObjectId.Should().Be(objectId);
        result.Context.Should().Be(SystemContext.PlanningPlanningIntervalObjective);
        result.Status.Should().Be(HealthStatus.Healthy);
        result.ReportedOn.Should().Be(reportedOn);
        result.Expiration.Should().Be(expiration);
        result.Note.Should().Be("Test");

        result.PostPersistenceActions.Should().NotBeEmpty();

        // get the first action and execute it to ensure it's a valid HealthCheckCreatedEvent
        var action = result.PostPersistenceActions.First();
        action.Should().NotBeNull();
        action();
        result.DomainEvents.Should().NotBeEmpty();
        result.DomainEvents.Should().ContainSingle(e => e is HealthCheckCreatedEvent);

        // get the event and verify its properties
        var createdEvent = result.DomainEvents.OfType<HealthCheckCreatedEvent>().First();
        createdEvent.Id.Should().Be(result.Id);
        createdEvent.ObjectId.Should().Be(objectId);
        createdEvent.Context.Should().Be(SystemContext.PlanningPlanningIntervalObjective);
        createdEvent.Status.Should().Be(HealthStatus.Healthy);
        createdEvent.ReportedById.Should().Be(reportedById);
        createdEvent.ReportedOn.Should().Be(reportedOn);
        createdEvent.Expiration.Should().Be(expiration);
    }

    #endregion AddHealthCheck
}
