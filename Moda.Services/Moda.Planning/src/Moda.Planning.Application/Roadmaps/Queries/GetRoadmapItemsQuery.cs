using System.Linq.Expressions;
using Ardalis.GuardClauses;
using Moda.Common.Application.Models;
using Moda.Common.Domain.Enums;
using Moda.Planning.Application.Roadmaps.Dtos;
using Moda.Planning.Domain.Models.Roadmaps;

namespace Moda.Planning.Application.Roadmaps.Queries;
public sealed record GetRoadmapItemsQuery : IQuery<List<RoadmapItemListDto>>
{
    public GetRoadmapItemsQuery(IdOrKey idOrKey)
    {
        IdOrKeyFilter = idOrKey.CreateFilter<Roadmap>();
    }

    public Expression<Func<Roadmap, bool>> IdOrKeyFilter { get; }
}

internal sealed class GetRoadmapItemsQueryHandler(IPlanningDbContext planningDbContext, ICurrentUser currentUser) : IQueryHandler<GetRoadmapItemsQuery, List<RoadmapItemListDto>>
{
    private readonly IPlanningDbContext _planningDbContext = planningDbContext;
    private readonly Guid _currentUserEmployeeId = Guard.Against.NullOrEmpty(currentUser.GetEmployeeId());

    public async Task<List<RoadmapItemListDto>> Handle(GetRoadmapItemsQuery request, CancellationToken cancellationToken)
    {
        var publicVisibility = Visibility.Public;

        var items = await _planningDbContext.Roadmaps
            .Where(request.IdOrKeyFilter)
            .Where(r => r.Visibility == publicVisibility || r.RoadmapManagers.Any(m => m.ManagerId == _currentUserEmployeeId))
            .SelectMany(r => r.Items)
            //.ProjectToType<RoadmapItemDto>() // not working, it's always returning only the BaseRoadmapItem properties
            .ToListAsync(cancellationToken);

        return items
            .Where(r => r.ParentId == null)
            .Adapt<List<RoadmapItemListDto>>();
    }
}
