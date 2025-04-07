using Moda.Common.Domain.Models.KeyPerformanceIndicators;

namespace Moda.ProjectPortfolioManagement.Domain.Models.StrategicInitiatives;

/// <summary>
/// Represents the parameters required to create or update a strategic initiative KPI.
/// </summary>
/// <param name="Name">The name of the KPI.</param>
/// <param name="Description">A description of what the KPI measures.</param>
/// <param name="TargetValue">The target value that defines success for the KPI.</param>
/// <param name="Unit">The unit of measurement for the KPI.</param>
/// <param name="TargetDirection">The target direction for the KPI.</param>
public sealed record StrategicInitiativeKpiUpsertParameters(string Name, string? Description, double TargetValue, KpiUnit Unit, KpiTargetDirection TargetDirection);

