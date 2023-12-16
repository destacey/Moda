using Moda.Common.Extensions;
using Moda.Planning.Application.PlanningIntervals.Dtos;
using Moda.Planning.Domain.Enums;

namespace Moda.Planning.Application.PlanningIntervals.Queries;
public sealed record GetPlanningIntervalObjectiveTypesQuery() : IQuery<IReadOnlyList<PlanningIntervalObjectiveTypeDto>>;

internal sealed class GetPlanningIntervalObjectiveTypesQueryHandler : IQueryHandler<GetPlanningIntervalObjectiveTypesQuery, IReadOnlyList<PlanningIntervalObjectiveTypeDto>>
{
    public Task<IReadOnlyList<PlanningIntervalObjectiveTypeDto>> Handle(GetPlanningIntervalObjectiveTypesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<PlanningIntervalObjectiveTypeDto> values = Enum.GetValues<PlanningIntervalObjectiveType>().Select(t => new PlanningIntervalObjectiveTypeDto
        {
            Id = (int)t,
            Name = t.GetDisplayName(),
            Order = t.GetDisplayOrder()
        }).ToList();

        return Task.FromResult(values);
    }
}
