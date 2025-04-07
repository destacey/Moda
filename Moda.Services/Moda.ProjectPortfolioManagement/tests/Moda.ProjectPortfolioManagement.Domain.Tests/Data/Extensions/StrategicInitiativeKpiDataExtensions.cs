using Moda.ProjectPortfolioManagement.Domain.Models.StrategicInitiatives;

namespace Moda.ProjectPortfolioManagement.Domain.Tests.Data.Extensions;
public static class StrategicInitiativeKpiDataExtensions
{
    public static StrategicInitiativeKpiUpsertParameters ToUpsertParameters(this StrategicInitiativeKpi kpi)
    {
        return new StrategicInitiativeKpiUpsertParameters(kpi.Name, kpi.Description, kpi.TargetValue, kpi.Unit, kpi.TargetDirection);
    }
}
