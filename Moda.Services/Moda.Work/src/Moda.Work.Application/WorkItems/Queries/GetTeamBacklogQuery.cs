using Moda.Common.Domain.Enums.Work;
using Moda.Organization.Domain.Models;
using Moda.Work.Application.WorkItems.Dtos;

namespace Moda.Work.Application.WorkItems.Queries;

public sealed record GetTeamBacklogQuery : IQuery<Result<List<WorkItemBacklogItemDto>>>
{
    public GetTeamBacklogQuery(Guid workspaceId)
    {
        Id = Guard.Against.NullOrEmpty(workspaceId);
    }

    public GetTeamBacklogQuery(TeamCode teamCode)
    {
        Code = Guard.Against.Null(teamCode);
    }

    public Guid? Id { get; }
    public TeamCode? Code { get; }
}

internal sealed class GetTeamBacklogQueryHandler(IWorkDbContext workDbContext, ILogger<GetTeamBacklogQueryHandler> logger) : IQueryHandler<GetTeamBacklogQuery, Result<List<WorkItemBacklogItemDto>>>
{
    private const string AppRequestName = nameof(GetTeamBacklogQuery);

    private readonly IWorkDbContext _workDbContext = workDbContext;
    private readonly ILogger<GetTeamBacklogQueryHandler> _logger = logger;

    public async Task<Result<List<WorkItemBacklogItemDto>>> Handle(GetTeamBacklogQuery request, CancellationToken cancellationToken)
    {
        Guid? teamId = request.Id;
        teamId ??= await _workDbContext.WorkTeams
                .Where(t => t.Code == request.Code!.Value)
                .Select(t => t.Id)
                .FirstOrDefaultAsync(cancellationToken);

        if (teamId is null)
        {
            _logger.LogError("{AppRequestName}: No team id or code provided. {@Request}", AppRequestName, request);
            return Result.Failure<List<WorkItemBacklogItemDto>>("No valid team id or code provided.");
        }

        var query = _workDbContext.WorkItems
            .Where(w => w.TeamId == teamId)
            .Where(w => w.StatusCategory == WorkStatusCategory.Proposed || w.StatusCategory == WorkStatusCategory.Active)
            .AsQueryable();

        var workItems = await query
            .ProjectToType<WorkItemBacklogItemDto>()
            .ToListAsync(cancellationToken);

        var rank = 1;
        var backlog = workItems.OrderBy(w => w.StackRank).ThenBy(w => w.Created).ToList();
        foreach (var workItem in backlog)
        {
            workItem.Rank = rank++;
        }

        return backlog;
    }
}