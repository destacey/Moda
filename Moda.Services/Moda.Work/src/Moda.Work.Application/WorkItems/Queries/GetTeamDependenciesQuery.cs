using Moda.Common.Domain.Enums.Work;
using Moda.Common.Domain.Extensions;
using Moda.Work.Application.WorkItems.Dtos;

namespace Moda.Work.Application.WorkItems.Queries;

// TODO: add TeamBoundary
public sealed record GetTeamDependenciesQuery(Guid TeamId, List<DependencyStatus> DependencyStatuses) : IQuery<List<DependencyDto>?>;


internal sealed class GetTeamDependenciesQueryHandler(IWorkDbContext workDbContext, ILogger<GetTeamDependenciesQueryHandler> logger) : IQueryHandler<GetTeamDependenciesQuery, List<DependencyDto>?>
{
    private const string AppRequestName = nameof(GetTeamDependenciesQuery);

    private readonly IWorkDbContext _workDbContext = workDbContext;
    private readonly ILogger<GetTeamDependenciesQueryHandler> _logger = logger;

    public async Task<List<DependencyDto>?> Handle(GetTeamDependenciesQuery request, CancellationToken cancellationToken)
    {
        var exists = await _workDbContext.WorkTeams
            .AnyAsync(t => t.Id == request.TeamId, cancellationToken);

        if (!exists)
        {
            _logger.LogError("{AppRequestName}: No team found for team {TeamId}.", AppRequestName, request.TeamId);
            return null;
        }

        // convert the dependency statuses to WorkStatusCategory
        var statusCategories = request.DependencyStatuses.Select(d => d.ToWorkStatusCategory()).ToArray();

        var dependencies = await _workDbContext.WorkItemLinks
            .Where(l => l.LinkType == WorkItemLinkType.Dependency && l.RemovedOn == null)
            .Where(l => l.Source!.TeamId == request.TeamId || l.Target!.TeamId == request.TeamId)
            .Where(l => statusCategories.Contains(l.Source!.StatusCategory))
            .ProjectToType<DependencyDto>()
            .ToListAsync(cancellationToken);

        _logger.LogDebug("{AppRequestName}: Found {DependencyCount} dependencies for team {TeamId}.", AppRequestName, dependencies.Count, request.TeamId);

        return dependencies;
    }
}
