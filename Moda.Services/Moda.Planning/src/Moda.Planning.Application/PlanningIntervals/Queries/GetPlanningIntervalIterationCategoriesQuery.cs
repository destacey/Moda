using Moda.Common.Extensions;
using Moda.Planning.Application.PlanningIntervals.Dtos;
using Moda.Planning.Domain.Enums;

namespace Moda.Planning.Application.PlanningIntervals.Queries;

public sealed record GetPlanningIntervalIterationCategoriesQuery : IQuery<List<PlanningIntervalIterationCategoryDto>> { }

internal sealed class GetPlanningIntervalIterationCategoriesQueryHandler : IQueryHandler<GetPlanningIntervalIterationCategoriesQuery, List<PlanningIntervalIterationCategoryDto>>
{
    public Task<List<PlanningIntervalIterationCategoryDto>> Handle(GetPlanningIntervalIterationCategoriesQuery request, CancellationToken cancellationToken)
    {
        List<PlanningIntervalIterationCategoryDto> values = [.. Enum.GetValues<IterationCategory>().Select(c => new PlanningIntervalIterationCategoryDto
        {
            Id = (int)c,
            Name = c.GetDisplayName(),
            Description = c.GetDisplayDescription(),
            Order = c.GetDisplayOrder()
        })];

        return Task.FromResult(values);
    }
}
