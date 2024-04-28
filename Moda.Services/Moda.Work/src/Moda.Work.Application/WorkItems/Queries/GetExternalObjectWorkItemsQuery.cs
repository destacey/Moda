using Moda.Work.Application.WorkItems.Dtos;

namespace Moda.Work.Application.WorkItems.Queries;
public sealed record GetExternalObjectWorkItemsQuery(Guid ObjectId) : IQuery<IReadOnlyCollection<WorkItemListDto>>;

internal sealed class GetExternalObjectWorkItemsQueryHandler(IWorkDbContext workDbContext, ILogger<GetExternalObjectWorkItemsQueryHandler> logger) : IQueryHandler<GetExternalObjectWorkItemsQuery, IReadOnlyCollection<WorkItemListDto>>
{
    private const string AppRequestName = nameof(GetExternalObjectWorkItemsQuery);

    private readonly IWorkDbContext _workDbContext = workDbContext;
    private readonly ILogger<GetExternalObjectWorkItemsQueryHandler> _logger = logger;

    public async Task<IReadOnlyCollection<WorkItemListDto>> Handle(GetExternalObjectWorkItemsQuery request, CancellationToken cancellationToken)
    {
        return await _workDbContext.WorkItemLinks
            .Where(e => e.ObjectId == request.ObjectId)
            .ProjectToType<WorkItemListDto>()
            .ToListAsync(cancellationToken);
    }
}
