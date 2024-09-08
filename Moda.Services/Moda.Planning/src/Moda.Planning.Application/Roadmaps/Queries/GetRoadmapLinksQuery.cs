using Ardalis.GuardClauses;
using Moda.Common.Domain.Enums;
using Moda.Planning.Application.Roadmaps.Dtos;

namespace Moda.Planning.Application.Roadmaps.Queries;
public sealed record GetRoadmapLinksQuery(List<Guid> RoadmapIds) : IQuery<List<RoadmapLinkDto>>;

public sealed class GetRoadmapLinksQueryValidator : AbstractValidator<GetRoadmapLinksQuery>
{
    public GetRoadmapLinksQueryValidator()
    {
        RuleFor(x => x.RoadmapIds).NotEmpty();

        RuleForEach(x => x.RoadmapIds)
            .NotEmpty();
    }
}

internal sealed class GetRoadmapLinksQueryHandler(IPlanningDbContext planningDbContext, ICurrentUser currentUser) : IQueryHandler<GetRoadmapLinksQuery, List<RoadmapLinkDto>>
{
    private readonly IPlanningDbContext _planningDbContext = planningDbContext;
    private readonly Guid _currentUserEmployeeId = Guard.Against.NullOrEmpty(currentUser.GetEmployeeId());

    public async Task<List<RoadmapLinkDto>> Handle(GetRoadmapLinksQuery request, CancellationToken cancellationToken)
    {
        var publicVisibility = Visibility.Public;

        return await _planningDbContext.Roadmaps
            .Where(r => request.RoadmapIds.Contains(r.Id))
            .Where(r => r.Visibility == publicVisibility || r.Managers.Any(m => m.ManagerId == _currentUserEmployeeId))
            .SelectMany(r => r.ChildLinks)
            .ProjectToType<RoadmapLinkDto>()
            .ToListAsync(cancellationToken);
    }
}
