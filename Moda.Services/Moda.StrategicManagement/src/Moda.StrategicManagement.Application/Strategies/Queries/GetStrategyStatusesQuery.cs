using Moda.Common.Application.Models;
using Moda.StrategicManagement.Domain.Enums;

namespace Moda.StrategicManagement.Application.Strategies.Queries;
public sealed record GetStrategyStatusesQuery : IQuery<List<StrategyStatusDto>> { }

internal sealed class GetStrategyStatusesQueryHandler : IQueryHandler<GetStrategyStatusesQuery, List<StrategyStatusDto>>
{
    public Task<List<StrategyStatusDto>> Handle(GetStrategyStatusesQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(CommonEnumDto.GetValues<StrategyStatus, StrategyStatusDto>());
    }
}

public sealed record StrategyStatusDto : CommonEnumDto { }