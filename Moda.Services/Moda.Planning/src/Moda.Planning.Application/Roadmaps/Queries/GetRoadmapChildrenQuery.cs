using Ardalis.GuardClauses;
using Moda.Common.Domain.Enums;
using Moda.Planning.Application.Roadmaps.Dtos;

namespace Moda.Planning.Application.Roadmaps.Queries;
public sealed record GetRoadmapChildrenQuery(List<Guid> RoadmapIds) : IQuery<List<RoadmapChildrenDto>>;

public sealed class GetRoadmapChildrenQueryValidator : AbstractValidator<GetRoadmapChildrenQuery>
{
    public GetRoadmapChildrenQueryValidator()
    {
        RuleFor(x => x.RoadmapIds).NotEmpty();

        RuleForEach(x => x.RoadmapIds)
            .NotEmpty();
    }
}

internal sealed class GetRoadmapChildrenQueryHandler(IPlanningDbContext planningDbContext, ICurrentUser currentUser) : IQueryHandler<GetRoadmapChildrenQuery, List<RoadmapChildrenDto>>
{
    private readonly IPlanningDbContext _planningDbContext = planningDbContext;
    private readonly Guid _currentUserEmployeeId = Guard.Against.NullOrEmpty(currentUser.GetEmployeeId());

    public async Task<List<RoadmapChildrenDto>> Handle(GetRoadmapChildrenQuery request, CancellationToken cancellationToken)
    {
        var publicVisibility = Visibility.Public;

        return await _planningDbContext.Roadmaps
            .Where(r => r.ParentId != null && request.RoadmapIds.Contains(r.ParentId.Value))
            .Where(r => r.Visibility == publicVisibility || r.RoadmapManagers.Any(m => m.ManagerId == _currentUserEmployeeId))
            .ProjectToType<RoadmapChildrenDto>()
            .ToListAsync(cancellationToken);
    }
}
