using Moda.Common.Application.Models;
using System.Linq.Expressions;
using Moda.Planning.Application.PlanningIntervals.Dtos;

namespace Moda.Planning.Application.PlanningIntervals.Queries;
public sealed record GetPlanningIntervalIterationsQuery : IQuery<IReadOnlyList<PlanningIntervalIterationListDto>>
{
    public GetPlanningIntervalIterationsQuery(IdOrKey idOrKey)
    {
        IdOrKeyFilter = idOrKey.CreateFilter<PlanningInterval>();
    }

    public Expression<Func<PlanningInterval, bool>> IdOrKeyFilter { get; }
}

internal sealed class GetPlanningIntervalIterationsQueryHandler : IQueryHandler<GetPlanningIntervalIterationsQuery, IReadOnlyList<PlanningIntervalIterationListDto>>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly ILogger<GetPlanningIntervalIterationsQueryHandler> _logger;

    public GetPlanningIntervalIterationsQueryHandler(IPlanningDbContext planningDbContext, ILogger<GetPlanningIntervalIterationsQueryHandler> logger)
    {
        _planningDbContext = planningDbContext;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PlanningIntervalIterationListDto>> Handle(GetPlanningIntervalIterationsQuery request, CancellationToken cancellationToken)
    {
        var iterations = await _planningDbContext.PlanningIntervals
            //.Include(p => p.Iterations)
            .Where(request.IdOrKeyFilter)
            .SelectMany(p => p.Iterations)
            .ProjectToType<PlanningIntervalIterationListDto>()
            .ToListAsync(cancellationToken);

        return iterations.OrderBy(i => i.Start).ToList();
    }
}
