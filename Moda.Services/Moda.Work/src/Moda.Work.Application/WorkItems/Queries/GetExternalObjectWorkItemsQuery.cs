using Moda.Work.Application.WorkItems.Dtos;

namespace Moda.Work.Application.WorkItems.Queries;
public sealed record GetExternalObjectWorkItemsQuery(Guid ObjectId) : IQuery<WorkItemsSummaryDto>;

internal sealed class GetExternalObjectWorkItemsQueryHandler(IWorkDbContext workDbContext, ILogger<GetExternalObjectWorkItemsQueryHandler> logger) : IQueryHandler<GetExternalObjectWorkItemsQuery, WorkItemsSummaryDto>
{
    private const string AppRequestName = nameof(GetExternalObjectWorkItemsQuery);

    private readonly IWorkDbContext _workDbContext = workDbContext;
    private readonly ILogger<GetExternalObjectWorkItemsQueryHandler> _logger = logger;

    public async Task<WorkItemsSummaryDto> Handle(GetExternalObjectWorkItemsQuery request, CancellationToken cancellationToken)
    {
        var query = _workDbContext.WorkItems
            .Where(e => e.SystemLinks.Any(sl => sl.ObjectId == request.ObjectId));

        var workItems = await query
            .ProjectToType<WorkItemListDto>()
            .ToListAsync(cancellationToken);

        if (workItems.Count == 0)
            return WorkItemsSummaryDto.Create(WorkItemProgressRollupDto.CreateEmpty(), workItems);

        var progressRollup = await new WorkItemProgessSummaryBuilder(_workDbContext, query).Build(cancellationToken);

        return WorkItemsSummaryDto.Create(progressRollup.RootRollup, workItems);
    }    
}
