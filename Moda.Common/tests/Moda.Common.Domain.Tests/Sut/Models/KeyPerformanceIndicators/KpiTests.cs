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
        kpi.CurrentValue.Should().BeNull();
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

    [Fact]
    public void CurrentValue_ShouldReturnLatestMeasurementValue()
    {
        // Arrange
        var kpi = _kpiFaker.Generate();
        var measurement1 = _measurementFaker.WithData(kpiId: kpi.Id, measurementDate: _dateTimeProvider.Now.Minus(Duration.FromDays(10))).Generate();
        var measurement2 = _measurementFaker.WithData(kpiId: kpi.Id, measurementDate: _dateTimeProvider.Now.Minus(Duration.FromDays(5))).Generate();

        kpi.AddMeasurement(measurement1);
        kpi.AddMeasurement(measurement2);

        // Act
        var currentValue = kpi.CurrentValue;

        // Assert
        currentValue.Should().Be(measurement2.ActualValue);
    }

    #region Checkpoints

    [Fact]
    public void AddCheckpoint_ShouldAddCheckpoint_WhenValidData()
    {
        // Arrange
        var kpi = _kpiFaker.Generate();
        var checkpoint = _checkpointFaker.WithData(kpiId: kpi.Id).Generate();

        // Act
        var addResult = kpi.AddCheckpoint(checkpoint);

        // Assert
        addResult.IsSuccess.Should().BeTrue();
        kpi.Checkpoints.Should().Contain(checkpoint);
    }

    [Fact]
    public void AddCheckpoint_ShouldReturnFailure_WhenKpiIdMismatch()
    {
        // Arrange
        var kpi = _kpiFaker.Generate();
        var checkpoint = _checkpointFaker.Generate();

        // Act
        var addResult = kpi.AddCheckpoint(checkpoint);

        // Assert
        addResult.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void RemoveCheckpoint_ShouldRemoveCheckpoint_WhenValidId()
    {
        // Arrange
        var kpi = _kpiFaker.Generate();
        var checkpoint = _checkpointFaker.WithData(kpiId: kpi.Id).Generate();
        kpi.AddCheckpoint(checkpoint);

        // Act
        var removeResult = kpi.RemoveCheckpoint(checkpoint.Id);

        // Assert
        removeResult.IsSuccess.Should().BeTrue();
        kpi.Checkpoints.Should().NotContain(checkpoint);
    }

    [Fact]
    public void RemoveCheckpoint_ShouldReturnFailure_WhenInvalidId()
    {
        // Arrange
        var kpi = _kpiFaker.Generate();
        var invalidCheckpointId = Guid.NewGuid();

        // Act
        var removeResult = kpi.RemoveCheckpoint(invalidCheckpointId);

        // Assert
        removeResult.IsFailure.Should().BeTrue();
    }

    #endregion Checkpoints

    #region Measurements

    [Fact]
    public void AddMeasurement_ShouldAddMeasurement_WhenValidData()
    {
        // Arrange
        var kpi = _kpiFaker.Generate();
        var measurement = _measurementFaker.WithData(kpiId: kpi.Id).Generate();

        // Act
        var addResult = kpi.AddMeasurement(measurement);

        // Assert
        addResult.IsSuccess.Should().BeTrue();
        kpi.Measurements.Should().Contain(measurement);
        kpi.CurrentValue.Should().Be(measurement.ActualValue);
    }

    [Fact]
    public void AddMeasurement_ShouldReturnFailure_WhenKpiIdMismatch()
    {
        // Arrange
        var kpi = _kpiFaker.Generate();
        var measurement = _measurementFaker.Generate();

        // Act
        var addResult = kpi.AddMeasurement(measurement);

        // Assert
        addResult.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void RemoveMeasurement_ShouldRemoveMeasurement_WhenValidId()
    {
        // Arrange
        var kpi = _kpiFaker.Generate();
        var measurement = _measurementFaker.WithData(kpiId: kpi.Id).Generate();
        kpi.AddMeasurement(measurement);

        // Act
        var removeResult = kpi.RemoveMeasurement(measurement.Id);

        // Assert
        removeResult.IsSuccess.Should().BeTrue();
        kpi.Measurements.Should().NotContain(measurement);
        kpi.CurrentValue.Should().BeNull();
    }

    [Fact]
    public void RemoveMeasurement_ShouldReturnFailure_WhenInvalidId()
    {
        // Arrange
        var kpi = _kpiFaker.Generate();
        var invalidMeasurementId = Guid.NewGuid();

        // Act
        var removeResult = kpi.RemoveMeasurement(invalidMeasurementId);

        // Assert
        removeResult.IsFailure.Should().BeTrue();
    }

    #endregion Measurements
}
