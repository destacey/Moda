using Moda.Common.Application.Models;
using Moda.StrategicManagement.Domain.Enums;

namespace Moda.StrategicManagement.Application.Visions.Queries;
public sealed record GetVisionStatesQuery : IQuery<List<VisionStateDto>> { }

internal sealed class GetVisionStatesQueryHandler : IQueryHandler<GetVisionStatesQuery, List<VisionStateDto>>
{
    public Task<List<VisionStateDto>> Handle(GetVisionStatesQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(CommonEnumDto.GetValues<VisionState, VisionStateDto>());
    }
}

public sealed record VisionStateDto : CommonEnumDto { }