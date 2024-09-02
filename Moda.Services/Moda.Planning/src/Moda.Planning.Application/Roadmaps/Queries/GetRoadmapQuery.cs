using System.Linq.Expressions;
using Ardalis.GuardClauses;
using Moda.Common.Application.Models;
using Moda.Planning.Application.Roadmaps.Dtos;

namespace Moda.Planning.Application.Roadmaps.Queries;

public sealed record GetRoadmapQuery : IQuery<RoadmapDetailsDto?>
{
    public GetRoadmapQuery(IdOrKey idOrKey)
    {
        IdOrKeyFilter = idOrKey.CreateFilter<Roadmap>(r => r.Id, r => r.Key);
    }

    public Expression<Func<Roadmap, bool>> IdOrKeyFilter { get; }
}

internal sealed class GetRoadmapQueryHandler(IPlanningDbContext planningDbContext, ICurrentUser currentUser) : IQueryHandler<GetRoadmapQuery, RoadmapDetailsDto?>
{
    private readonly IPlanningDbContext _planningDbContext = planningDbContext;
    private readonly Guid _currentUserEmployeeId = Guard.Against.NullOrEmpty(currentUser.GetEmployeeId());

    public async Task<RoadmapDetailsDto?> Handle(GetRoadmapQuery request, CancellationToken cancellationToken)
    {
        //var query = _planningDbContext.Roadmaps
        //    //.Include(r => r.Managers)
        //    .Where(r => r.IsPublic || r.Managers.Any(m => m.ManagerId == _currentUser.GetUserId()))
        //    .AsQueryable();


        var roadmap = await _planningDbContext.Roadmaps
            .Where(request.IdOrKeyFilter)
            .Where(r => r.IsPublic || r.Managers.Any(m => m.ManagerId == _currentUserEmployeeId))
            .ProjectToType<RoadmapDetailsDto>()
            .FirstOrDefaultAsync(cancellationToken);

        return roadmap;
    }
}
