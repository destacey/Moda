using Moda.Work.Application.WorkItems.Dtos;

namespace Moda.Work.Application.WorkItems.Queries;
public sealed record GetExternalObjectWorkItemMetricsQuery(Guid ObjectiveId, DateOnly Start, DateOnly End) : IQuery<List<WorkItemProgressDailyRollupDto>>;

internal sealed class GetExternalObjectWorkItemMetricsQueryHandler(IWorkDbContext workDbContext, ILogger<GetExternalObjectWorkItemMetricsQueryHandler> logger) : IQueryHandler<GetExternalObjectWorkItemMetricsQuery, List<WorkItemProgressDailyRollupDto>>
{
    private const string AppRequestName = nameof(GetExternalObjectWorkItemMetricsQuery);

    private readonly IWorkDbContext _workDbContext = workDbContext;
    private readonly ILogger<GetExternalObjectWorkItemMetricsQueryHandler> _logger = logger;

    public async Task<List<WorkItemProgressDailyRollupDto>> Handle(GetExternalObjectWorkItemMetricsQuery request, CancellationToken cancellationToken)
    {
        var query = _workDbContext.WorkItems
            .Where(e => e.ReferenceLinks.Any(sl => sl.ObjectId == request.ObjectiveId));

        var progress = await new WorkItemProgressStateBuilder(_workDbContext, query).Build(cancellationToken);

        return WorkItemProgressDailyRollupDto.CreateList(request.Start, request.End, progress);
    }    
}
