using Moda.Common.Application.Requests.Goals.Dtos;
using Moda.Common.Application.Requests.Goals.Queries;
using Moda.Goals.Application.Persistence;

namespace Moda.Goals.Application.Objectives.Queries;

internal sealed class GetObjectiveForPlanningIntervalQueryHandler(IGoalsDbContext goalsDbContext) : IQueryHandler<GetObjectiveForPlanningIntervalQuery, ObjectiveDetailsDto?>
{
    private readonly IGoalsDbContext _goalsDbContext = goalsDbContext;

    public async Task<ObjectiveDetailsDto?> Handle(GetObjectiveForPlanningIntervalQuery request, CancellationToken cancellationToken)
    {
        return await _goalsDbContext.Objectives
            .Where(o => o.Id == request.Id && o.PlanId == request.PlanningIntervalId)
            .ProjectToType<ObjectiveDetailsDto>()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
