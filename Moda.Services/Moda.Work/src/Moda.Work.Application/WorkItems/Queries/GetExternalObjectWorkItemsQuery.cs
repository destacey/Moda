﻿using Moda.Work.Application.Persistence;
using Moda.Work.Application.WorkItems.Dtos;

namespace Moda.Work.Application.WorkItems.Queries;
public sealed record GetExternalObjectWorkItemsQuery(Guid ObjectId) : IQuery<WorkItemsSummaryDto>;

internal sealed class GetExternalObjectWorkItemsQueryHandler(IWorkDbContext workDbContext) : IQueryHandler<GetExternalObjectWorkItemsQuery, WorkItemsSummaryDto>
{
    private readonly IWorkDbContext _workDbContext = workDbContext;

    public async Task<WorkItemsSummaryDto> Handle(GetExternalObjectWorkItemsQuery request, CancellationToken cancellationToken)
    {
        var query = _workDbContext.WorkItems
            .Where(e => e.ReferenceLinks.Any(sl => sl.ObjectId == request.ObjectId));

        var workItems = await query
            .ProjectToType<WorkItemListDto>()
            .ToListAsync(cancellationToken);

        if (workItems.Count == 0)
            return WorkItemsSummaryDto.Create(WorkItemProgressRollupDto.CreateEmpty(), workItems);

        var progress = await new WorkItemProgressStateBuilder(_workDbContext, query).Build(cancellationToken);

        var summary = WorkItemProgressSummary.Create(progress);
        return WorkItemsSummaryDto.Create(summary.RootRollup, workItems);
    }    
}
