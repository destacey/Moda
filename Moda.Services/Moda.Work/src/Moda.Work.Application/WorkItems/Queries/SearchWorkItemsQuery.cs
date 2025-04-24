using Moda.Work.Application.Persistence;
using Moda.Work.Application.WorkItems.Dtos;

namespace Moda.Work.Application.WorkItems.Queries;
public sealed record SearchWorkItemsQuery(string SearchTerm, int Top) : IQuery<Result<IReadOnlyCollection<WorkItemListDto>>>;

internal sealed class SearchWorkItemsQueryHandler(IWorkDbContext workDbContext, ILogger<SearchWorkItemsQueryHandler> logger) : IQueryHandler<SearchWorkItemsQuery, Result<IReadOnlyCollection<WorkItemListDto>>>
{
    private const string AppRequestName = nameof(SearchWorkItemsQuery);

    private readonly IWorkDbContext _workDbContext = workDbContext;
    private readonly ILogger<SearchWorkItemsQueryHandler> _logger = logger;

    public async Task<Result<IReadOnlyCollection<WorkItemListDto>>> Handle(SearchWorkItemsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            _logger.LogWarning("{AppRequestName}: No search term provided. {@Request}", AppRequestName, request);
            return Result.Failure<IReadOnlyCollection<WorkItemListDto>>("No search term provided.");
        }

        var workitems = await _workDbContext.WorkItems
            .Where(e => e.Title.Contains(request.SearchTerm)
                || ((string)e.Key).Contains(request.SearchTerm) 
                || (e.ParentId.HasValue && ((string)e.Parent!.Key).Contains(request.SearchTerm)))
            .ProjectToType<WorkItemListDto>()
            .ToArrayAsync(cancellationToken);

        // TODO: This is a temporary solution to sort the work items by key.  Need to link the STRING_SPLIT function in the OrderBy clause.

        return workitems
            .OrderByKey(true)
            .Take(request.Top)
            .ToArray();
    }
}

