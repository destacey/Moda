using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Moda.AppIntegration.Application.Connectors.Queries;
public sealed record GetAzureDevOpsBoardsConnectorConfigurationQuery : IQuery<AzureDevOpsBoardsConnectorConfigurationDto?>
{
    public GetAzureDevOpsBoardsConnectorConfigurationQuery(Guid connectorId)
    {
        ConnectorId = connectorId;
    }

    public Guid ConnectorId { get; }
}

internal sealed class GetAzureDevOpsBoardsConnectorConfigurationQueryHandler : IQueryHandler<GetAzureDevOpsBoardsConnectorConfigurationQuery, AzureDevOpsBoardsConnectorConfigurationDto?>
{
    private readonly IAppIntegrationDbContext _appIntegrationDbContext;
    private readonly ILogger<GetAzureDevOpsBoardsConnectorConfigurationQueryHandler> _logger;

    public GetAzureDevOpsBoardsConnectorConfigurationQueryHandler(IAppIntegrationDbContext appIntegrationDbContext, ILogger<GetAzureDevOpsBoardsConnectorConfigurationQueryHandler> logger)
    {
        _appIntegrationDbContext = appIntegrationDbContext;
        _logger = logger;
    }

    public async Task<AzureDevOpsBoardsConnectorConfigurationDto?> Handle(GetAzureDevOpsBoardsConnectorConfigurationQuery request, CancellationToken cancellationToken)
    {
        var connector = await _appIntegrationDbContext.AzureDevOpsBoardsConnectors
            .FirstOrDefaultAsync(c => c.Id == request.ConnectorId, cancellationToken);

        return connector is null
            ? null
            : new AzureDevOpsBoardsConnectorConfigurationDto()
                {
                    ConnectorId = connector.Id,
                    Organization = connector.Configuration?.Organization,
                    PersonalAccessToken = connector.Configuration?.PersonalAccessToken,
                    OrganizationUrl = connector.Configuration?.OrganizationUrl
                };
    }
}
