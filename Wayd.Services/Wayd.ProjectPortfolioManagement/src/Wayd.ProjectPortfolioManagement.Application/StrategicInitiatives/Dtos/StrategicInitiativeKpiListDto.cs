using Wayd.Common.Domain.Models.KeyPerformanceIndicators;
using Wayd.ProjectPortfolioManagement.Domain.Models.StrategicInitiatives;

namespace Wayd.ProjectPortfolioManagement.Application.StrategicInitiatives.Dtos;

public sealed record StrategicInitiativeKpiListDto : IMapFrom<StrategicInitiativeKpi>
{
    public Guid Id { get; set; }

    /// <summary>
    /// The unique key of the KPI.  This is an alternate key to the Id.
    /// </summary>
    public int Key { get; set; }

    /// <summary>
    /// The name of the KPI.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The starting (baseline) value of the KPI. Used to track progress relative to where the KPI began.
    /// </summary>
    public double? StartingValue { get; set; }

    /// <summary>
    /// The target value that defines success for the KPI.
    /// </summary>
    public double TargetValue { get; set; }

    /// <summary>
    /// The actual value of the KPI.
    /// </summary>
    public double? ActualValue { get; set; }

    /// <summary>
    /// The progress towards the KPI target, calculated based on the starting value, actual value, target value, and target direction.
    /// </summary>
    public double? Progress => KpiUtils.CalculateProgress(StartingValue, ActualValue, TargetValue, TargetDirection);

    /// <summary>
    /// An optional prefix symbol displayed before the numeric value (e.g. "$").
    /// </summary>
    public string? Prefix { get; set; }

    /// <summary>
    /// An optional suffix symbol displayed after the numeric value (e.g. "%", "M").
    /// </summary>
    public string? Suffix { get; set; }

    /// <summary>
    /// Gets the target direction for the KPI. This indicates whether the KPI is expected to increase or decrease.
    /// </summary>
    public KpiTargetDirection TargetDirection { get; set; }
}
