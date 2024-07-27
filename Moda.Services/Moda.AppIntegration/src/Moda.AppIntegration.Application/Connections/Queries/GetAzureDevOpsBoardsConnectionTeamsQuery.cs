using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Moda.AppIntegration.Application.Connections.Queries;
public sealed record GetAzureDevOpsBoardsConnectionTeamsQuery(Guid ConnectionId) : IQuery<AzureDevOpsBoardsTeamConfigurationDto?>;

internal sealed class GetAzureDevOpsBoardsConnectionTeamsQueryHandler : IQueryHandler<GetAzureDevOpsBoardsConnectionTeamsQuery, AzureDevOpsBoardsTeamConfigurationDto?>
{
    private readonly IAppIntegrationDbContext _appIntegrationDbContext;
    private readonly ILogger<GetAzureDevOpsBoardsConnectionTeamsQueryHandler> _logger;

    public GetAzureDevOpsBoardsConnectionTeamsQueryHandler(IAppIntegrationDbContext appIntegrationDbContext, ILogger<GetAzureDevOpsBoardsConnectionTeamsQueryHandler> logger)
    {
        _appIntegrationDbContext = appIntegrationDbContext;
        _logger = logger;
    }

    public async Task<AzureDevOpsBoardsTeamConfigurationDto?> Handle(GetAzureDevOpsBoardsConnectionTeamsQuery request, CancellationToken cancellationToken)
    {
        var teamConfiguration = await _appIntegrationDbContext.AzureDevOpsBoardsConnections
            .AsNoTracking()
            .Where(c => c.Id == request.ConnectionId)
            .Select(c => c.TeamConfiguration)
            .ProjectToType<AzureDevOpsBoardsTeamConfigurationDto>()
            .FirstOrDefaultAsync(cancellationToken);

        return teamConfiguration;

        //return connection?.Adapt<AzureDevOpsBoardsConnectionDetailsDto>();

        // TODO: using ProjectTo is not working with the JSON Column - .ProjectToType<AzureDevOpsBoardsConnectionDetailsDto>()
    }
}
