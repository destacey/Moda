using Moda.Common.Application.Models;
using Moda.Common.Domain.Enums.StrategicManagement;

namespace Moda.StrategicManagement.Application.StrategicThemes.Queries;
public sealed record GetStrategicThemeStatesQuery : IQuery<List<StrategicThemeStateDto>> { }

internal sealed class GetStrategicThemeStatesQueryHandler : IQueryHandler<GetStrategicThemeStatesQuery, List<StrategicThemeStateDto>>
{
    public Task<List<StrategicThemeStateDto>> Handle(GetStrategicThemeStatesQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(CommonEnumDto.GetValues<StrategicThemeState, StrategicThemeStateDto>());
    }
}

public sealed record StrategicThemeStateDto : CommonEnumDto { }