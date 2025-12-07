using Moda.Common.Application.Requests.Goals.Dtos;

namespace Moda.Common.Application.Requests.Goals.Queries;

public sealed record GetObjectivesForPlanningIntervalsQuery : IQuery<IReadOnlyList<ObjectiveListDto>>
{
    public GetObjectivesForPlanningIntervalsQuery(Guid[] planningIntervalIds, Guid[]? teamIds)
    {
        PlanningIntervalIds = planningIntervalIds;
        TeamIds = teamIds;
    }

    public Guid[] PlanningIntervalIds { get; }
    public Guid[]? TeamIds { get; }
}