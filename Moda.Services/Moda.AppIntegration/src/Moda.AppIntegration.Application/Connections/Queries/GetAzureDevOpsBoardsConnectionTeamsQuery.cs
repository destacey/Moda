using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Moda.AppIntegration.Application.Connections.Queries;
public sealed record GetAzureDevOpsBoardsConnectionTeamsQuery(Guid ConnectionId, Guid? WorkspaceId = null) : IQuery<List<AzureDevOpsBoardsWorkspaceTeamDto>>;

internal sealed class GetAzureDevOpsBoardsConnectionTeamsQueryHandler : IQueryHandler<GetAzureDevOpsBoardsConnectionTeamsQuery, List<AzureDevOpsBoardsWorkspaceTeamDto>>
{
    private readonly IAppIntegrationDbContext _appIntegrationDbContext;
    private readonly ILogger<GetAzureDevOpsBoardsConnectionTeamsQueryHandler> _logger;

    public GetAzureDevOpsBoardsConnectionTeamsQueryHandler(IAppIntegrationDbContext appIntegrationDbContext, ILogger<GetAzureDevOpsBoardsConnectionTeamsQueryHandler> logger)
    {
        _appIntegrationDbContext = appIntegrationDbContext;
        _logger = logger;
    }

    public async Task<List<AzureDevOpsBoardsWorkspaceTeamDto>> Handle(GetAzureDevOpsBoardsConnectionTeamsQuery request, CancellationToken cancellationToken)
    {
        var query = _appIntegrationDbContext.AzureDevOpsBoardsConnections
            .Where(c => c.Id == request.ConnectionId)
            .SelectMany(c => c.TeamConfiguration.WorkspaceTeams);

        if (request.WorkspaceId.HasValue)
        {
            query = query.Where(t => t.WorkspaceId == request.WorkspaceId);
        }

        var teams = await query
            .ProjectToType<AzureDevOpsBoardsWorkspaceTeamDto>()
            .ToListAsync(cancellationToken);

        return teams;
    }
}
