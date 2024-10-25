using System.Linq.Expressions;
using Ardalis.GuardClauses;
using Moda.Common.Application.Models;
using Moda.Common.Domain.Enums;
using Moda.Planning.Application.Roadmaps.Dtos;
using Moda.Planning.Domain.Enums;
using Moda.Planning.Domain.Models.Roadmaps;

namespace Moda.Planning.Application.Roadmaps.Queries;
public sealed record GetRoadmapActivitiesQuery : IQuery<List<RoadmapActivityListDto>>
{
    public GetRoadmapActivitiesQuery(IdOrKey idOrKey)
    {
        IdOrKeyFilter = idOrKey.CreateFilter<Roadmap>();
    }

    public Expression<Func<Roadmap, bool>> IdOrKeyFilter { get; }
}

internal sealed class GetRoadmapActivitiesQueryHandler(IPlanningDbContext planningDbContext, ICurrentUser currentUser) : IQueryHandler<GetRoadmapActivitiesQuery, List<RoadmapActivityListDto>>
{
    private readonly IPlanningDbContext _planningDbContext = planningDbContext;
    private readonly Guid _currentUserEmployeeId = Guard.Against.NullOrEmpty(currentUser.GetEmployeeId());

    public async Task<List<RoadmapActivityListDto>> Handle(GetRoadmapActivitiesQuery request, CancellationToken cancellationToken)
    {
        var publicVisibility = Visibility.Public;

        var items = await _planningDbContext.Roadmaps
            .Where(request.IdOrKeyFilter)
            .Where(r => r.Visibility == publicVisibility || r.RoadmapManagers.Any(m => m.ManagerId == _currentUserEmployeeId))
            .SelectMany(r => r.Items)
            .Where(ri => ri.Type == RoadmapItemType.Activity)
            .OfType<RoadmapActivity>()
            //.ProjectToType<RoadmapActivityListDto>() // not working, it's always returning only the BaseRoadmapItem properties
            .ToListAsync(cancellationToken);

        return items
            .Where(ri => ri.Parent == null)
            .OrderBy(ri => ri.Order)
            //.ToList();
            .Adapt<List<RoadmapActivityListDto>>();
    }
}
