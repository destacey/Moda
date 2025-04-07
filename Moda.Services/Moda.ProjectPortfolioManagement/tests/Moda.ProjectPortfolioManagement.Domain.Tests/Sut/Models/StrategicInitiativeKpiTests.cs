using FluentAssertions;
using Moda.Common.Domain.Models.KeyPerformanceIndicators;
using Moda.ProjectPortfolioManagement.Domain.Models.StrategicInitiatives;
using Moda.ProjectPortfolioManagement.Domain.Tests.Data;
using Moda.ProjectPortfolioManagement.Domain.Tests.Data.Extensions;
using Moda.Tests.Shared;
using NodaTime;
using NodaTime.Extensions;
using NodaTime.Testing;

namespace Moda.ProjectPortfolioManagement.Domain.Tests.Sut.Models;

public sealed class StrategicInitiativeKpiTests
{
    private readonly TestingDateTimeProvider _dateTimeProvider;
    private readonly StrategicInitiativeKpiFaker _kpiFaker;
    private readonly StrategicInitiativeKpiCheckpointFaker _checkpointFaker;
    private readonly StrategicInitiativeKpiMeasurementFaker _measurementFaker;

    public StrategicInitiativeKpiTests()
    {
        _dateTimeProvider = new TestingDateTimeProvider(new FakeClock(DateTime.UtcNow.ToInstant()));
        _kpiFaker = new StrategicInitiativeKpiFaker();
        _checkpointFaker = new StrategicInitiativeKpiCheckpointFaker(_dateTimeProvider);
        _measurementFaker = new StrategicInitiativeKpiMeasurementFaker(_dateTimeProvider);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenValidData()
    {
        // Arrange
        var expectedKpi = _kpiFaker.Generate();
        var kpiParameters = expectedKpi.ToUpsertParameters();

        // Act
        var kpi = StrategicInitiativeKpi.Create(expectedKpi.StrategicInitiativeId, kpiParameters);

        // Assert
        kpi.Should().NotBeNull();
        kpi.Name.Should().Be(expectedKpi.Name);
        kpi.Description.Should().Be(expectedKpi.Description);
        kpi.TargetValue.Should().Be(expectedKpi.TargetValue);
        kpi.ActualValue.Should().BeNull();
        kpi.Unit.Should().Be(expectedKpi.Unit);
        kpi.TargetDirection.Should().Be(expectedKpi.TargetDirection);
        kpi.StrategicInitiativeId.Should().Be(expectedKpi.StrategicInitiativeId);
    }

    [Fact]
    public void Update_ShouldUpdateProperties_WhenValidData()
    {
        // Arrange
        var kpi = _kpiFaker.Generate();
        var kpiParameters = kpi.ToUpsertParameters() with
        {
            Name = "Updated KPI",
            Description = "Updated description",
            TargetValue = 85.0,
            Unit = KpiUnit.Percentage,
            TargetDirection = kpi.TargetDirection is KpiTargetDirection.Increase ? KpiTargetDirection.Decrease : KpiTargetDirection.Increase
        };

        var expectedStrategicInitiativeId = kpi.StrategicInitiativeId;

        // Act
        var updateResult = kpi.Update(kpiParameters);

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
        kpi.Name.Should().Be(kpiParameters.Name);
        kpi.Description.Should().Be(kpiParameters.Description);
        kpi.TargetValue.Should().Be(kpiParameters.TargetValue);
        kpi.Unit.Should().Be(kpiParameters.Unit);
        kpi.TargetDirection.Should().Be(kpiParameters.TargetDirection);
        kpi.StrategicInitiativeId.Should().Be(expectedStrategicInitiativeId);
    }

    [Fact]
    public void CurrentValue_ShouldReturnLatestMeasurementValue()
    {
        // Arrange
        var kpi = _kpiFaker.Generate();
        var measurement1 = _measurementFaker.WithData(kpiId: kpi.Id, measurementDate: _dateTimeProvider.Now.Minus(Duration.FromDays(10))).Generate();
        var measurement2 = _measurementFaker.WithData(kpiId: kpi.Id, measurementDate: _dateTimeProvider.Now.Minus(Duration.FromDays(5))).Generate();

        // Act
        kpi.AddMeasurement(measurement1);
        kpi.AddMeasurement(measurement2);

        // Assert
        kpi.ActualValue.Should().Be(measurement2.ActualValue);
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
        kpi.ActualValue.Should().Be(measurement.ActualValue);
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
        kpi.ActualValue.Should().BeNull();
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
