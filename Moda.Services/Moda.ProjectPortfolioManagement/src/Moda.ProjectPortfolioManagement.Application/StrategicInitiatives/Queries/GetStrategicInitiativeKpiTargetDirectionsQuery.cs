using Moda.Common.Application.Models;
using Moda.Common.Domain.Models.KeyPerformanceIndicators;

namespace Moda.ProjectPortfolioManagement.Application.StrategicInitiatives.Queries;
public sealed record GetStrategicInitiativeKpiTargetDirectionsQuery() : IQuery<List<StrategicInitiativeKpiTargetDirectionDto>>;

internal sealed class GetStrategicInitiativeKpiTargetDirectionsQueryHandler : IQueryHandler<GetStrategicInitiativeKpiTargetDirectionsQuery, List<StrategicInitiativeKpiTargetDirectionDto>>
{
    public Task<List<StrategicInitiativeKpiTargetDirectionDto>> Handle(GetStrategicInitiativeKpiTargetDirectionsQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(CommonEnumDto.GetValues<KpiTargetDirection, StrategicInitiativeKpiTargetDirectionDto>());
    }
}



public sealed record StrategicInitiativeKpiTargetDirectionDto : CommonEnumDto { }