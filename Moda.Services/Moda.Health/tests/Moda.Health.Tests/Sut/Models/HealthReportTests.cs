﻿using Moda.Common.Domain.Events;
using Moda.Health.Tests.Data;
using Moda.Tests.Shared;
using NodaTime.Extensions;
using NodaTime.Testing;

namespace Moda.Health.Tests.Sut.Models;
public class HealthReportTests
{
    private readonly TestingDateTimeProvider _dateTimeManager;

    public HealthReportTests()
    {
        // TODO: Replace with a FakeClock that can be injected into the constructor.
        _dateTimeManager = new(new FakeClock(DateTime.UtcNow.ToInstant()));
    }

    #region Constructor

    [Fact]
    public void Constructor_WhenValid_ReturnsHealthReport()
    {
        // Arrange
        var objectId = Guid.NewGuid();
        var healthChecks = new HealthCheckFaker(_dateTimeManager.Now, objectId)
            .MultipleWithSameObjectId(objectId,5);

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
        var healthChecks = new HealthCheckFaker(_dateTimeManager.Now.Minus(Duration.FromDays(30)), objectId)
            .MultipleWithSameObjectId(objectId, 2);
        var reportedById = Guid.NewGuid();
        var healthReport = new HealthReport(healthChecks);

        // Act
        var result = healthReport.AddHealthCheck(objectId, SystemContext.PlanningPlanningIntervalObjective, HealthStatus.Healthy, reportedById, _dateTimeManager.Now, _dateTimeManager.Now.Plus(Duration.FromDays(5)), "Test");

        // Assert
        result.Should().NotBeNull();
        healthReport.HealthChecks.Count.Should().Be(3);
        result.ObjectId.Should().Be(objectId);
        result.Context.Should().Be(SystemContext.PlanningPlanningIntervalObjective);
        result.Status.Should().Be(HealthStatus.Healthy);
        result.ReportedOn.Should().Be(_dateTimeManager.Now);
        result.Expiration.Should().Be(_dateTimeManager.Now.Plus(Duration.FromDays(5)));
        result.Note.Should().Be("Test");

        result.DomainEvents.Should().NotBeEmpty();
        result.DomainEvents.Should().ContainSingle(e => e is EntityCreatedEvent<HealthCheck>);
    }

    [Fact]
    public void AddHealthCheck_WhenHealthCheckOverlaps_ReturnsHealthCheck()
    {
        // Arrange
        var objectId = Guid.NewGuid();
        var healthChecks = new HealthCheckFaker(_dateTimeManager.Now.Minus(Duration.FromDays(8)), objectId)
            .MultipleWithSameObjectId(objectId, 2);
        var reportedById = Guid.NewGuid();
        var healthReport = new HealthReport(healthChecks);

        // Act
        var result = healthReport.AddHealthCheck(objectId, SystemContext.PlanningPlanningIntervalObjective, HealthStatus.Healthy, reportedById, _dateTimeManager.Now, _dateTimeManager.Now.Plus(Duration.FromDays(5)), "Test");

        var previous = healthReport.HealthChecks.OrderByDescending(h => h.ReportedOn).ToList()[1];

        // Assert
        result.Should().NotBeNull();
        healthReport.HealthChecks.Count.Should().Be(3);
        previous.Expiration.Should().Be(_dateTimeManager.Now);
        result.ObjectId.Should().Be(objectId);
        result.Context.Should().Be(SystemContext.PlanningPlanningIntervalObjective);
        result.Status.Should().Be(HealthStatus.Healthy);
        result.ReportedOn.Should().Be(_dateTimeManager.Now);
        result.Expiration.Should().Be(_dateTimeManager.Now.Plus(Duration.FromDays(5)));
        result.Note.Should().Be("Test");

        result.DomainEvents.Should().NotBeEmpty();
        result.DomainEvents.Should().ContainSingle(e => e is EntityCreatedEvent<HealthCheck>);
    }

    #endregion AddHealthCheck
}
