using Moda.Common.Domain.Models.KeyPerformanceIndicators;
using Moda.Common.Domain.Tests.Data;
using Moda.Common.Domain.Tests.Data.Models;
using Moda.Tests.Shared;
using NodaTime.Extensions;
using NodaTime.Testing;

namespace Moda.Common.Domain.Tests.Sut.Models.KeyPerformanceIndicators;

public sealed class KpiTests
{
    private readonly TestingDateTimeProvider _dateTimeProvider;
    private readonly TestKpiFaker _kpiFaker;
    private readonly TestKpiCheckpointFaker _checkpointFaker;
    private readonly TestKpiMeasurementFaker _measurementFaker;

    public KpiTests()
    {
        _dateTimeProvider = new TestingDateTimeProvider(new FakeClock(DateTime.UtcNow.ToInstant()));
        _kpiFaker = new TestKpiFaker();
        _checkpointFaker = new TestKpiCheckpointFaker(_dateTimeProvider);
        _measurementFaker = new TestKpiMeasurementFaker(_dateTimeProvider);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenValidData()
    {
        // Arrange
        var name = "Test KPI";
        var description = "Test description";
        var targetValue = 75.0;
        var unit = KpiUnit.Percentage;
        var direction = KpiTargetDirection.Increase;

        // Act
        var kpi = TestKpi.Create(name, description, targetValue, unit, direction);

        // Assert
        kpi.Should().NotBeNull();
        kpi.Name.Should().Be(name);
        kpi.Description.Should().Be(description);
        kpi.TargetValue.Should().Be(targetValue);
        kpi.Unit.Should().Be(unit);
        kpi.TargetDirection.Should().Be(direction);
    }

    [Fact]
    public void Update_ShouldUpdateProperties_WhenValidData()
    {
        // Arrange
        var kpi = _kpiFaker.Generate();
        var newName = "Updated KPI";
        var newDescription = "Updated description";
        var newTargetValue = 85.0;
        var newUnit = KpiUnit.Percentage;
        var newTargetDirection = kpi.TargetDirection is KpiTargetDirection.Increase ? KpiTargetDirection.Decrease : KpiTargetDirection.Increase;

        // Act
        var updateResult = kpi.Update(newName, newDescription, newTargetValue, newUnit, newTargetDirection);

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
        kpi.Name.Should().Be(newName);
        kpi.Description.Should().Be(newDescription);
        kpi.TargetValue.Should().Be(newTargetValue);
        kpi.Unit.Should().Be(newUnit);
        kpi.TargetDirection.Should().Be(newTargetDirection);
    }
}
