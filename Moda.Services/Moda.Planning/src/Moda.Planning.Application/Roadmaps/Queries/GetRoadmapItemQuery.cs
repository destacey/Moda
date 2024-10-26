using System.Linq.Expressions;
using Ardalis.GuardClauses;
using Moda.Common.Application.Models;
using Moda.Common.Domain.Enums;
using Moda.Planning.Application.Roadmaps.Dtos;
using Moda.Planning.Domain.Models.Roadmaps;

namespace Moda.Planning.Application.Roadmaps.Queries;
public sealed record GetRoadmapItemQuery : IQuery<RoadmapItemDetailsDto>
{
    public GetRoadmapItemQuery(IdOrKey roadmapIdOrKey, Guid itemId)
    {
        IdOrKeyFilter = roadmapIdOrKey.CreateFilter<Roadmap>();
        ItemId = itemId;
    }

    public Expression<Func<Roadmap, bool>> IdOrKeyFilter { get; }
    public Guid ItemId { get; }
}

internal sealed class GetRoadmapItemQueryHandler(IPlanningDbContext planningDbContext, ICurrentUser currentUser) : IQueryHandler<GetRoadmapItemQuery, RoadmapItemDetailsDto>
{
    private readonly IPlanningDbContext _planningDbContext = planningDbContext;
    private readonly Guid _currentUserEmployeeId = Guard.Against.NullOrEmpty(currentUser.GetEmployeeId());

    public async Task<RoadmapItemDetailsDto> Handle(GetRoadmapItemQuery request, CancellationToken cancellationToken)
    {
        var publicVisibility = Visibility.Public;

        var item = await _planningDbContext.Roadmaps
            .Where(request.IdOrKeyFilter)
            .Where(r => r.Visibility == publicVisibility || r.RoadmapManagers.Any(m => m.ManagerId == _currentUserEmployeeId))
            .SelectMany(r => r.Items)
            .Include(r => r.Parent)
            .Where(r => r.Id == request.ItemId)
            .FirstOrDefaultAsync(cancellationToken);

        return item.Adapt<RoadmapItemDetailsDto>();
    }
}
