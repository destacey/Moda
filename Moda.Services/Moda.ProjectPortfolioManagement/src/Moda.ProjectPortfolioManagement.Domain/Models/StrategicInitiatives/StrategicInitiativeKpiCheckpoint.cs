using Moda.Common.Domain.Models.KeyPerformanceIndicators;
using NodaTime;

namespace Moda.ProjectPortfolioManagement.Domain.Models.StrategicInitiatives;

public sealed class StrategicInitiativeKpiCheckpoint : KpiCheckpoint, ISystemAuditable
{
    private StrategicInitiativeKpiCheckpoint() : base() { }

    private StrategicInitiativeKpiCheckpoint(Guid kpiId, double targetValue, Instant checkpointDate, string dateLabel)
        : base(kpiId, targetValue, checkpointDate, dateLabel)
    {
    }

    /// <summary>
    /// Factory method to create a new instance of <see cref="StrategicInitiativeKpiCheckpoint"/>.
    /// </summary>
    /// <param name="kpiId">The unique identifier of the parent KPI.</param>
    /// <param name="targetValue">The planned target value for the KPI at this checkpoint.</param>
    /// <param name="checkpointDate">The date and time when this planned target is expected to be achieved.</param>
    /// <param name="dateLabel">A short label that describes the date of this checkpoint.</param>
    /// <returns></returns>
    public static StrategicInitiativeKpiCheckpoint Create(Guid kpiId, double targetValue, Instant checkpointDate, string dateLabel)
    {
        return new StrategicInitiativeKpiCheckpoint(kpiId, targetValue, checkpointDate, dateLabel);
    }
}
