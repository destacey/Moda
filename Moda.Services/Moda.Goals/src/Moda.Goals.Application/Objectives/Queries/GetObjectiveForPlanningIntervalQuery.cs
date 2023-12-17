using Moda.Goals.Application.Objectives.Dtos;
using Moda.Goals.Application.Persistence;

namespace Moda.Goals.Application.Objectives.Queries;
public sealed record GetObjectiveForPlanningIntervalQuery(Guid Id, Guid PlanningIntervalId) : IQuery<ObjectiveDetailsDto?>;

internal sealed class GetObjectiveForPlanningIntervalQueryHandler : IQueryHandler<GetObjectiveForPlanningIntervalQuery, ObjectiveDetailsDto?>
{
    private readonly IGoalsDbContext _goalsDbContext;

    public GetObjectiveForPlanningIntervalQueryHandler(IGoalsDbContext goalsDbContext)
    {
        _goalsDbContext = goalsDbContext;
    }

    public async Task<ObjectiveDetailsDto?> Handle(GetObjectiveForPlanningIntervalQuery request, CancellationToken cancellationToken)
    {
        return await _goalsDbContext.Objectives
            .Where(o => o.Id == request.Id && o.PlanId == request.PlanningIntervalId)
            .ProjectToType<ObjectiveDetailsDto>()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
