using Moda.Common.Application.Requests.Goals.Dtos;
using Moda.Common.Application.Requests.Goals.Queries;
using Moda.Goals.Application.Persistence;

namespace Moda.Goals.Application.Objectives.Queries;

internal sealed class SearchObjectivesByNameQueryHandler(IGoalsDbContext goalsDbContext)
    : IQueryHandler<SearchObjectivesByNameQuery, IReadOnlyList<ObjectiveSearchResultDto>>
{
    public async Task<IReadOnlyList<ObjectiveSearchResultDto>> Handle(SearchObjectivesByNameQuery request, CancellationToken cancellationToken)
    {
        return await goalsDbContext.Objectives
            .Where(o => o.PlanId.HasValue && o.Name.Contains(request.SearchTerm))
            .Select(o => new ObjectiveSearchResultDto
            {
                Id = o.Id,
                Name = o.Name,
                PlanId = o.PlanId,
                OwnerId = o.OwnerId
            })
            .Take(request.MaxResults)
            .ToListAsync(cancellationToken);
    }
}
