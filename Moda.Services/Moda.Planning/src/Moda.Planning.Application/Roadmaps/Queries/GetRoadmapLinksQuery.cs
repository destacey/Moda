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

internal sealed class GetRoadmapLinksQueryHandler(IPlanningDbContext planningDbContext) : IQueryHandler<GetRoadmapLinksQuery, List<RoadmapLinkDto>>
{
    private readonly IPlanningDbContext _planningDbContext = planningDbContext;

    public async Task<List<RoadmapLinkDto>> Handle(GetRoadmapLinksQuery request, CancellationToken cancellationToken)
    {
        return await _planningDbContext.Roadmaps
            .Where(r => request.RoadmapIds.Contains(r.Id))
            .SelectMany(r => r.ChildLinks)
            .ProjectToType<RoadmapLinkDto>()
            .ToListAsync(cancellationToken);
    }
}
