using Moda.Common.Extensions;
using Moda.Planning.Application.PlanningIntervals.Dtos;
using Moda.Planning.Domain.Enums;

namespace Moda.Planning.Application.PlanningIntervals.Queries;

public sealed record GetPlanningIntervalIterationTypesQuery : IQuery<IReadOnlyList<PlanningIntervalIterationTypeDto>> { }

internal sealed class GetPlanningIntervalIterationTypesQueryHandler : IQueryHandler<GetPlanningIntervalIterationTypesQuery, IReadOnlyList<PlanningIntervalIterationTypeDto>>
{
    public Task<IReadOnlyList<PlanningIntervalIterationTypeDto>> Handle(GetPlanningIntervalIterationTypesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<PlanningIntervalIterationTypeDto> values = Enum.GetValues<IterationType>().Select(c => new PlanningIntervalIterationTypeDto
        {
            Id = (int)c,
            Name = c.GetDisplayName(),
            Description = c.GetDisplayDescription(),
            Order = c.GetDisplayOrder()
        }).ToList();

        return Task.FromResult(values);
    }
}
