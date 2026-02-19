using Moda.Common.Domain.Enums;

namespace Moda.Common.Domain.Models.KeyPerformanceIndicators;

public static class KpiUtils
{
    public static bool IsKpiOnTrack(double actualValue, double targetValue, KpiTargetDirection targetDirection)
    {
        return targetDirection switch
        {
            KpiTargetDirection.Increase => actualValue >= targetValue,
            KpiTargetDirection.Decrease => actualValue <= targetValue,
            _ => throw new ArgumentOutOfRangeException(nameof(targetDirection), $"Unsupported target direction: {targetDirection}")
        };
    }

    public static KpiTrend GetKpiTrend(double? previousValue, double currentValue, KpiTargetDirection targetDirection)
    {
        if (!previousValue.HasValue)
            return KpiTrend.NoData;

        return targetDirection switch
        {
            KpiTargetDirection.Increase => currentValue > previousValue.Value ? KpiTrend.Improving : (currentValue < previousValue.Value ? KpiTrend.Worsening : KpiTrend.Stable),
            KpiTargetDirection.Decrease => currentValue < previousValue.Value ? KpiTrend.Improving : (currentValue > previousValue.Value ? KpiTrend.Worsening : KpiTrend.Stable),
            _ => throw new ArgumentOutOfRangeException(nameof(targetDirection), $"Unsupported target direction: {targetDirection}")
        };
    }
}
