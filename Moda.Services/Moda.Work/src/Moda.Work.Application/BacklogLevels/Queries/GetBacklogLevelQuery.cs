namespace Moda.Work.Application.BacklogLevels.Queries;
public sealed record GetBacklogLevelQuery : IQuery<BacklogLevelDto?>
{
    public GetBacklogLevelQuery(int id)
    {
        Id = id;
    }

    public int Id { get; }
}

internal sealed class GetBacklogLevelQueryHandler : IQueryHandler<GetBacklogLevelQuery, BacklogLevelDto?>
{
    private readonly IWorkDbContext _workDbContext;
    private readonly ILogger<GetBacklogLevelQueryHandler> _logger;

    public GetBacklogLevelQueryHandler(IWorkDbContext workDbContext, ILogger<GetBacklogLevelQueryHandler> logger)
    {
        _workDbContext = workDbContext;
        _logger = logger;
    }

    public async Task<BacklogLevelDto?> Handle(GetBacklogLevelQuery request, CancellationToken cancellationToken)
    {
        return await _workDbContext.BacklogLevelSchemes
            .SelectMany(s => s.BacklogLevels.Where(c => c.Id == request.Id))
            .ProjectToType<BacklogLevelDto>()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
