using Moda.Common.Domain.Models.KeyPerformanceIndicators;

namespace Moda.Common.Domain.Tests.Data.Models;

public sealed class TestKpiCheckpoint : KpiCheckpoint
{
    private TestKpiCheckpoint() : base() { }

    private TestKpiCheckpoint(Guid kpiId, double targetValue, Instant checkpointDate, string dateLabel, double? atRiskValue = null)
        : base(kpiId, targetValue, checkpointDate, dateLabel, atRiskValue)
    {
    }

    public static TestKpiCheckpoint Create(Guid kpiId, double targetValue, Instant checkpointDate, string dateLabel, double? atRiskValue = null)
    {
        return new TestKpiCheckpoint(kpiId, targetValue, checkpointDate, dateLabel, atRiskValue);
    }
}
