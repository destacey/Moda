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
        var startingValue = 25.0;
        var targetValue = 75.0;
        var unit = KpiUnit.Percentage;
        var direction = KpiTargetDirection.Increase;

        // Act
        var kpi = TestKpi.Create(name, description, startingValue, targetValue, unit, direction);

        // Assert
        kpi.Should().NotBeNull();
        kpi.Name.Should().Be(name);
        kpi.Description.Should().Be(description);
        kpi.StartingValue.Should().Be(startingValue);
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
        var newStartingValue = 10.0;
        var newTargetValue = 85.0;
        var newUnit = KpiUnit.Percentage;
        var newTargetDirection = KpiTargetDirection.Increase;

        // Act
        var updateResult = kpi.Update(newName, newDescription, newStartingValue, newTargetValue, newUnit, newTargetDirection);

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
        kpi.Name.Should().Be(newName);
        kpi.Description.Should().Be(newDescription);
        kpi.StartingValue.Should().Be(newStartingValue);
        kpi.TargetValue.Should().Be(newTargetValue);
        kpi.Unit.Should().Be(newUnit);
        kpi.TargetDirection.Should().Be(newTargetDirection);
    }

    [Fact]
    public void Create_ShouldSucceed_WhenStartingValueIsNull()
    {
        // Act
        var kpi = TestKpi.Create("Test KPI", null, null, 75.0, KpiUnit.Percentage, KpiTargetDirection.Increase);

        // Assert
        kpi.StartingValue.Should().BeNull();
    }

    [Theory]
    [InlineData(25.0, 75.0, KpiTargetDirection.Increase)] // less than target
    [InlineData(85.0, 75.0, KpiTargetDirection.Decrease)] // greater than target
    public void Create_ShouldSucceed_WhenStartingValueIsValid(double startingValue, double targetValue, KpiTargetDirection direction)
    {
        // Act
        var kpi = TestKpi.Create("Test KPI", null, startingValue, targetValue, KpiUnit.Percentage, direction);

        // Assert
        kpi.StartingValue.Should().Be(startingValue);
    }

    [Theory]
    [InlineData(75.0, 75.0, KpiTargetDirection.Increase)] // equal to target
    [InlineData(80.0, 75.0, KpiTargetDirection.Increase)] // greater than target
    [InlineData(75.0, 75.0, KpiTargetDirection.Decrease)] // equal to target
    [InlineData(70.0, 75.0, KpiTargetDirection.Decrease)] // less than target
    public void Create_ShouldThrow_WhenStartingValueIsInvalid(double startingValue, double targetValue, KpiTargetDirection direction)
    {
        // Act
        var act = () => TestKpi.Create("Test KPI", null, startingValue, targetValue, KpiUnit.Percentage, direction);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("startingValue");
    }

    [Fact]
    public void Update_ShouldSucceed_WhenStartingValueIsNull()
    {
        // Arrange
        var kpi = _kpiFaker.Generate();

        // Act
        var result = kpi.Update("Test KPI", null, null, 75.0, KpiUnit.Percentage, KpiTargetDirection.Increase);

        // Assert
        result.IsSuccess.Should().BeTrue();
        kpi.StartingValue.Should().BeNull();
    }

    [Theory]
    [InlineData(25.0, 75.0, KpiTargetDirection.Increase)] // less than target
    [InlineData(85.0, 75.0, KpiTargetDirection.Decrease)] // greater than target
    public void Update_ShouldSucceed_WhenStartingValueIsValid(double startingValue, double targetValue, KpiTargetDirection direction)
    {
        // Arrange
        var kpi = _kpiFaker.Generate();

        // Act
        var result = kpi.Update("Test KPI", null, startingValue, targetValue, KpiUnit.Percentage, direction);

        // Assert
        result.IsSuccess.Should().BeTrue();
        kpi.StartingValue.Should().Be(startingValue);
    }

    [Theory]
    [InlineData(75.0, 75.0, KpiTargetDirection.Increase)] // equal to target
    [InlineData(80.0, 75.0, KpiTargetDirection.Increase)] // greater than target
    [InlineData(75.0, 75.0, KpiTargetDirection.Decrease)] // equal to target
    [InlineData(70.0, 75.0, KpiTargetDirection.Decrease)] // less than target
    public void Update_ShouldReturnFailure_WhenStartingValueIsInvalid(double startingValue, double targetValue, KpiTargetDirection direction)
    {
        // Arrange
        var kpi = _kpiFaker.Generate();

        // Act
        var result = kpi.Update("Test KPI", null, startingValue, targetValue, KpiUnit.Percentage, direction);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}

