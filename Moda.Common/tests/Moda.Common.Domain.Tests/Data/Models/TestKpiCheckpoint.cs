using Moda.Common.Domain.Models.KeyPerformanceIndicators;

namespace Moda.Common.Domain.Tests.Data.Models;

public sealed class TestKpiCheckpoint : KpiCheckpoint
{
    private TestKpiCheckpoint() : base() { }

    private TestKpiCheckpoint(Guid kpiId, double targetValue, Instant checkpointDate) 
        : base(kpiId, targetValue, checkpointDate)
    {
    }

    public static TestKpiCheckpoint Create(Guid kpiId, double targetValue, Instant checkpointDate)
    {
        return new TestKpiCheckpoint(kpiId, targetValue, checkpointDate);
    }
}
