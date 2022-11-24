using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Moda.AppIntegration.Application.Connections.Queries;
public sealed record GetAzureDevOpsBoardsConnectionConfigurationQuery : IQuery<AzureDevOpsBoardsConnectionConfigurationDto?>
{
    public GetAzureDevOpsBoardsConnectionConfigurationQuery(Guid connectionId)
    {
        ConnectionId = connectionId;
    }

    public Guid ConnectionId { get; }
}

internal sealed class GetAzureDevOpsBoardsConnectionConfigurationQueryHandler : IQueryHandler<GetAzureDevOpsBoardsConnectionConfigurationQuery, AzureDevOpsBoardsConnectionConfigurationDto?>
{
    private readonly IAppIntegrationDbContext _appIntegrationDbContext;
    private readonly ILogger<GetAzureDevOpsBoardsConnectionConfigurationQueryHandler> _logger;

    public GetAzureDevOpsBoardsConnectionConfigurationQueryHandler(IAppIntegrationDbContext appIntegrationDbContext, ILogger<GetAzureDevOpsBoardsConnectionConfigurationQueryHandler> logger)
    {
        _appIntegrationDbContext = appIntegrationDbContext;
        _logger = logger;
    }

    public async Task<AzureDevOpsBoardsConnectionConfigurationDto?> Handle(GetAzureDevOpsBoardsConnectionConfigurationQuery request, CancellationToken cancellationToken)
    {
        var connection = await _appIntegrationDbContext.AzureDevOpsBoardsConnections
            .FirstOrDefaultAsync(c => c.Id == request.ConnectionId, cancellationToken);

        return connection is null
            ? null
            : new AzureDevOpsBoardsConnectionConfigurationDto()
                {
                    ConnectionId = connection.Id,
                    Organization = connection.Configuration?.Organization,
                    PersonalAccessToken = connection.Configuration?.PersonalAccessToken,
                    OrganizationUrl = connection.Configuration?.OrganizationUrl
                };
    }
}
