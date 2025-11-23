using System.Linq.Expressions;
using Moda.Common.Application.Models.Organizations;
using Moda.Common.Domain.Enums.Work;
using Moda.Work.Application.Extensions;
using Moda.Work.Application.Persistence;
using Moda.Work.Application.WorkItems.Dtos;

namespace Moda.Work.Application.WorkItems.Queries;

public sealed record GetTeamBacklogQuery : IQuery<Result<List<WorkItemBacklogItemDto>>>
{
    public GetTeamBacklogQuery(TeamIdOrCode teamIdOrCode)
    {
        TeamFilter = teamIdOrCode.CreateWorkTeamFilter<WorkItem>();
    }

    public Expression<Func<WorkItem, bool>> TeamFilter { get; }
}

internal sealed class GetTeamBacklogQueryHandler(IWorkDbContext workDbContext) : IQueryHandler<GetTeamBacklogQuery, Result<List<WorkItemBacklogItemDto>>>
{
    private readonly IWorkDbContext _workDbContext = workDbContext;

    public async Task<Result<List<WorkItemBacklogItemDto>>> Handle(GetTeamBacklogQuery request, CancellationToken cancellationToken)
    {
        var validStatusCategories = new[]
        {
            WorkStatusCategory.Proposed,
            WorkStatusCategory.Active
        };

        var query = _workDbContext.WorkItems
            .Where(request.TeamFilter)
            .Where(item => item.Type.Level!.Tier == WorkTypeTier.Requirement)
            .Where(w => validStatusCategories.Contains(w.StatusCategory));

        var workItems = await query
            .ProjectToType<WorkItemBacklogItemDto>()
            .ToListAsync(cancellationToken);

        workItems.Sort((a, b) =>
        {
            var rankCompare = a.StackRank.CompareTo(b.StackRank);
            return rankCompare != 0 ? rankCompare : a.Created.CompareTo(b.Created);
        });

        foreach (var (index, workItem) in workItems.Index())
        {
            workItem.Rank = index + 1;
        }

        return workItems;
    }
}