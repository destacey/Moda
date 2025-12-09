using Moda.Common.Application.Requests.Goals.Dtos;

namespace Moda.Common.Application.Requests.Goals.Queries;

public sealed record GetObjectiveForPlanningIntervalQuery(Guid Id, Guid PlanningIntervalId) : IQuery<ObjectiveDetailsDto?>;
