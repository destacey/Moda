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

        return await _workDbContext.WorkItems
            .Where(e => e.Title.Contains(request.SearchTerm))
            //|| e.Key.ToString().Contains(request.SearchTerm))
            //.OrderBy(e => e.Key.WorkspaceKey)
            //    .ThenBy(e => e.Key.WorkItemNumber)
            .Take(request.Top)
            .ProjectToType<WorkItemListDto>()
            .ToListAsync(cancellationToken);
    }
}

