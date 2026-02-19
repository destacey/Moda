using Moda.Common.Domain.Enums;
using Moda.Common.Domain.Models.KeyPerformanceIndicators;

namespace Moda.Common.Domain.Tests.Sut.Models.KeyPerformanceIndicators;

public sealed class KpiUtilsTests
{
    #region IsKpiOnTrack

    [Theory]
    [InlineData(100.0, 75.0, KpiTargetDirection.Increase, true)]   // above target
    [InlineData(75.0, 75.0, KpiTargetDirection.Increase, true)]    // equal to target
    [InlineData(74.9, 75.0, KpiTargetDirection.Increase, false)]   // below target
    [InlineData(50.0, 75.0, KpiTargetDirection.Decrease, true)]    // below target
    [InlineData(75.0, 75.0, KpiTargetDirection.Decrease, true)]    // equal to target
    [InlineData(75.1, 75.0, KpiTargetDirection.Decrease, false)]   // above target
    public void IsKpiOnTrack_ShouldReturnExpected_ForDirectionAndValues(double actualValue, double targetValue, KpiTargetDirection direction, bool expected)
    {
        // Act
        var result = KpiUtils.IsKpiOnTrack(actualValue, targetValue, direction);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void IsKpiOnTrack_ShouldThrow_WhenTargetDirectionIsInvalid()
    {
        // Act
        var act = () => KpiUtils.IsKpiOnTrack(50.0, 75.0, (KpiTargetDirection)99);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("targetDirection");
    }

    #endregion

    #region GetKpiHealth

    [Theory]
    [InlineData(75.0, 75.0, KpiTargetDirection.Increase, KpiHealth.Healthy)]   // equal to target
    [InlineData(100.0, 75.0, KpiTargetDirection.Increase, KpiHealth.Healthy)]  // above target
    [InlineData(60.0, 75.0, KpiTargetDirection.Increase, KpiHealth.Unhealthy)] // below target
    [InlineData(75.0, 75.0, KpiTargetDirection.Decrease, KpiHealth.Healthy)]   // equal to target
    [InlineData(50.0, 75.0, KpiTargetDirection.Decrease, KpiHealth.Healthy)]   // below target
    [InlineData(80.0, 75.0, KpiTargetDirection.Decrease, KpiHealth.Unhealthy)] // above target
    public void GetKpiHealth_ShouldReturnExpected_WhenAtRiskValueIsNull(double actualValue, double targetValue, KpiTargetDirection direction, KpiHealth expected)
    {
        // Act
        var result = KpiUtils.GetKpiHealth(actualValue, targetValue, null, direction);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(80.0, 75.0, 60.0, KpiTargetDirection.Increase, KpiHealth.Healthy)]   // actual >= target
    [InlineData(65.0, 75.0, 60.0, KpiTargetDirection.Increase, KpiHealth.AtRisk)]    // at-risk <= actual < target
    [InlineData(55.0, 75.0, 60.0, KpiTargetDirection.Increase, KpiHealth.Unhealthy)] // actual < at-risk
    [InlineData(70.0, 75.0, 80.0, KpiTargetDirection.Decrease, KpiHealth.Healthy)]   // actual <= target
    [InlineData(78.0, 75.0, 80.0, KpiTargetDirection.Decrease, KpiHealth.AtRisk)]    // target < actual <= at-risk
    [InlineData(85.0, 75.0, 80.0, KpiTargetDirection.Decrease, KpiHealth.Unhealthy)] // actual > at-risk
    public void GetKpiHealth_ShouldReturnExpected_WhenAtRiskValueIsSet(double actualValue, double targetValue, double atRiskValue, KpiTargetDirection direction, KpiHealth expected)
    {
        // Act
        var result = KpiUtils.GetKpiHealth(actualValue, targetValue, atRiskValue, direction);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region GetKpiTrend

    [Theory]
    [InlineData(KpiTargetDirection.Increase)]
    [InlineData(KpiTargetDirection.Decrease)]
    public void GetKpiTrend_ShouldReturnNoData_WhenPreviousValueIsNull(KpiTargetDirection direction)
    {
        // Act
        var result = KpiUtils.GetKpiTrend(null, 75.0, direction);

        // Assert
        result.Should().Be(KpiTrend.NoData);
    }

    [Theory]
    [InlineData(60.0, 75.0, KpiTargetDirection.Increase, KpiTrend.Improving)]  // current > previous
    [InlineData(80.0, 75.0, KpiTargetDirection.Increase, KpiTrend.Worsening)]  // current < previous
    [InlineData(75.0, 75.0, KpiTargetDirection.Increase, KpiTrend.Stable)]     // current == previous
    [InlineData(80.0, 75.0, KpiTargetDirection.Decrease, KpiTrend.Improving)]  // current < previous
    [InlineData(60.0, 75.0, KpiTargetDirection.Decrease, KpiTrend.Worsening)]  // current > previous
    [InlineData(75.0, 75.0, KpiTargetDirection.Decrease, KpiTrend.Stable)]     // current == previous
    public void GetKpiTrend_ShouldReturnExpected_ForDirectionAndValues(double previousValue, double currentValue, KpiTargetDirection direction, KpiTrend expected)
    {
        // Act
        var result = KpiUtils.GetKpiTrend(previousValue, currentValue, direction);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetKpiTrend_ShouldThrow_WhenTargetDirectionIsInvalid()
    {
        // Act
        var act = () => KpiUtils.GetKpiTrend(50.0, 75.0, (KpiTargetDirection)99);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("targetDirection");
    }

    #endregion
}
