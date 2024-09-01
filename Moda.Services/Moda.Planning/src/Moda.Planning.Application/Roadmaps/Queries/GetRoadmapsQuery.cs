using Moda.Planning.Application.Roadmaps.Dtos;

namespace Moda.Planning.Application.Roadmaps.Queries;

public sealed record GetRoadmapsQuery() : IQuery<List<RoadmapListDto>>;

internal sealed class GetRoadmapsQueryHandler(IPlanningDbContext planningDbContext, ICurrentUser currentUser)
    : IQueryHandler<GetRoadmapsQuery, List<RoadmapListDto>>
{
    private readonly IPlanningDbContext _planningDbContext = planningDbContext;
    private readonly Guid _currentUserId = currentUser.GetUserId();

    public async Task<List<RoadmapListDto>> Handle(GetRoadmapsQuery request, CancellationToken cancellationToken)
    {
        return await _planningDbContext.Roadmaps
            .Where(r => r.IsPublic || r.Managers.Any(m => m.ManagerId == _currentUserId))
            .ProjectToType<RoadmapListDto>()
            .ToListAsync(cancellationToken);
    }
}
