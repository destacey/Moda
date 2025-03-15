using Moda.Common.Domain.Models.KeyPerformanceIndicators;

namespace Moda.ProjectPortfolioManagement.Domain.Models;

public class StrategicInitiativeKpi : Kpi, ISystemAuditable
{
    private StrategicInitiativeKpi() : base() { }

    private StrategicInitiativeKpi(string name, string description, double targetValue, KpiUnit unit, KpiTargetDirection direction, Guid strategicInitiativeId)
        : base(name, description, targetValue, unit, direction)
    {
        StrategicInitiativeId = strategicInitiativeId;
    }

    /// <summary>
    /// The unique identifier of the associated Strategic Initiative.
    /// </summary>
    public Guid StrategicInitiativeId { get; protected set; }

    /// <summary>
    /// Factory method to create a new instance of <see cref="StrategicInitiativeKpi"/>.
    /// </summary>
    /// <param name="name">The name of the KPI.</param>
    /// <param name="description">A description of what the KPI measures.</param>
    /// <param name="targetValue">The target value that defines success for the KPI.</param>
    /// <param name="unit">The unit of measurement for the KPI.</param>
    /// <param name="direction">The target direction for the KPI.</param>
    /// <param name="strategicInitiativeId">The unique identifier of the associated Strategic Initiative.</param>
    /// <returns></returns>
    public static StrategicInitiativeKpi Create(string name, string description, double targetValue, KpiUnit unit, KpiTargetDirection direction, Guid strategicInitiativeId)
    {
        return new StrategicInitiativeKpi(name, description, targetValue, unit, direction, strategicInitiativeId);
    }
}
