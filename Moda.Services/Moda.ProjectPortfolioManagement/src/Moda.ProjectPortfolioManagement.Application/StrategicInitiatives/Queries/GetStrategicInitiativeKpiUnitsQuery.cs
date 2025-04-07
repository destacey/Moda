using Moda.Common.Application.Models;
using Moda.Common.Domain.Models.KeyPerformanceIndicators;

namespace Moda.ProjectPortfolioManagement.Application.StrategicInitiatives.Queries;
public sealed record GetStrategicInitiativeKpiUnitsQuery() : IQuery<List<StrategicInitiativeKpiUnitDto>>;

internal sealed class GetStrategicInitiativeKpiUnitsQueryHandler : IQueryHandler<GetStrategicInitiativeKpiUnitsQuery, List<StrategicInitiativeKpiUnitDto>>
{
    public Task<List<StrategicInitiativeKpiUnitDto>> Handle(GetStrategicInitiativeKpiUnitsQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(CommonEnumDto.GetValues<KpiUnit, StrategicInitiativeKpiUnitDto>());
    }
}

public sealed record StrategicInitiativeKpiUnitDto : CommonEnumDto { }