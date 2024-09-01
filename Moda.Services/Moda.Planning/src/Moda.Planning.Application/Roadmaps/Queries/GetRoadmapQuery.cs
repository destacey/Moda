using System.Linq.Expressions;
using Moda.Common.Application.Models;
using Moda.Planning.Application.Roadmaps.Dtos;

namespace Moda.Planning.Application.Roadmaps.Queries;

public sealed record GetRoadmapQuery : IQuery<RoadmapDetailsDto?>
{
    public GetRoadmapQuery(IdOrKey idOrKey)
    {
        IdOrKeyFilter = idOrKey.Match(
            id => (Expression<Func<Roadmap, bool>>)(r => r.Id == id),
            key => (Expression<Func<Roadmap, bool>>)(r => r.Key == key)
        );
    }

    public Expression<Func<Roadmap, bool>> IdOrKeyFilter { get; }
}

internal sealed class GetRoadmapQueryHandler(IPlanningDbContext planningDbContext, ICurrentUser currentUser) : IQueryHandler<GetRoadmapQuery, RoadmapDetailsDto?>
{
    private readonly IPlanningDbContext _planningDbContext = planningDbContext;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<RoadmapDetailsDto?> Handle(GetRoadmapQuery request, CancellationToken cancellationToken)
    {
        var query = _planningDbContext.Roadmaps
            //.Include(r => r.Managers)
            .Where(r => r.IsPublic || r.Managers.Any(m => m.ManagerId == _currentUser.GetUserId()))
            .AsQueryable();

        return await query
            .Where(request.IdOrKeyFilter)
            .ProjectToType<RoadmapDetailsDto>()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
