using Moda.Common.Extensions;
using Moda.Planning.Application.PlanningIntervals.Dtos;
using Moda.Planning.Domain.Enums;

namespace Moda.Planning.Application.PlanningIntervals.Queries;

public sealed record GetPlanningIntervalObjectiveStatusesQuery : IQuery<IReadOnlyList<PlanningIntervalObjectiveStatusDto>> { }

internal sealed class GetPlanningIntervalObjectiveStatusesQueryHandler : IQueryHandler<GetPlanningIntervalObjectiveStatusesQuery, IReadOnlyList<PlanningIntervalObjectiveStatusDto>>
{
    public Task<IReadOnlyList<PlanningIntervalObjectiveStatusDto>> Handle(GetPlanningIntervalObjectiveStatusesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<PlanningIntervalObjectiveStatusDto> values = Enum.GetValues<ObjectiveStatus>().Select(c => new PlanningIntervalObjectiveStatusDto
        {
            Id = (int)c,
            Name = c.GetDisplayName(),
            Description = c.GetDisplayDescription(),
            Order = c.GetDisplayOrder()
        }).ToList();

        return Task.FromResult(values);
    }
}
