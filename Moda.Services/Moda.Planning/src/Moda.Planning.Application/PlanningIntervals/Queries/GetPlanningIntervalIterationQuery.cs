using System.Linq.Expressions;
using Moda.Common.Application.Models;
using Moda.Planning.Application.PlanningIntervals.Dtos;

namespace Moda.Planning.Application.PlanningIntervals.Queries;

public sealed record GetPlanningIntervalIterationQuery : IQuery<PlanningIntervalIterationDetailsDto?>
{
    public GetPlanningIntervalIterationQuery(IdOrKey planningIntervalIdOrKey, IdOrKey iterationIdOrKey)
    {
        PlanningIntervalIdOrKeyFilter = planningIntervalIdOrKey.CreateFilter<PlanningInterval>();
        IterationIdOrKeyFilter = iterationIdOrKey.CreateFilter<PlanningIntervalIteration>();
    }

    public Expression<Func<PlanningInterval, bool>> PlanningIntervalIdOrKeyFilter { get; }
    public Expression<Func<PlanningIntervalIteration, bool>> IterationIdOrKeyFilter { get; }
}

internal sealed class GetPlanningIntervalIterationQueryHandler(IPlanningDbContext planningDbContext)
    : IQueryHandler<GetPlanningIntervalIterationQuery, PlanningIntervalIterationDetailsDto?>
{
    private readonly IPlanningDbContext _planningDbContext = planningDbContext;
    public async Task<PlanningIntervalIterationDetailsDto?> Handle(GetPlanningIntervalIterationQuery request, CancellationToken cancellationToken)
    {
        var iteration = await _planningDbContext.PlanningIntervals
            .Where(request.PlanningIntervalIdOrKeyFilter)
            .SelectMany(pi => pi.Iterations)
            .Where(request.IterationIdOrKeyFilter)
            .ProjectToType<PlanningIntervalIterationDetailsDto>()
            .FirstOrDefaultAsync(cancellationToken);

        return iteration;
    }
}