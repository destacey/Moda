using Moda.Common.Application.Models;
using Moda.Common.Domain.Enums.Planning;

namespace Moda.Planning.Application.Roadmaps.Queries;

public sealed record GetRoadmapStatesQuery : IQuery<List<RoadmapStateDto>> { }

internal sealed class GetRoadmapStatesQueryHandler : IQueryHandler<GetRoadmapStatesQuery, List<RoadmapStateDto>>
{
    public Task<List<RoadmapStateDto>> Handle(GetRoadmapStatesQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(CommonEnumDto.GetValues<RoadmapState, RoadmapStateDto>());
    }
}

public sealed record RoadmapStateDto : CommonEnumDto { }
