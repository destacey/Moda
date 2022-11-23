using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Moda.AppIntegration.Application.Connectors.Queries;
public sealed record GetConnectorQuery : IQuery<ConnectorDetailsDto?>
{
    public GetConnectorQuery(Guid connectorId)
    {
        ConnectorId = connectorId;
    }

    public Guid ConnectorId { get; }
}

internal sealed class GetConnectorQueryHandler : IQueryHandler<GetConnectorQuery, ConnectorDetailsDto?>
{
    private readonly IAppIntegrationDbContext _appIntegrationDbContext;
    private readonly ILogger<GetConnectorQueryHandler> _logger;

    public GetConnectorQueryHandler(IAppIntegrationDbContext appIntegrationDbContext, ILogger<GetConnectorQueryHandler> logger)
    {
        _appIntegrationDbContext = appIntegrationDbContext;
        _logger = logger;
    }

    public async Task<ConnectorDetailsDto?> Handle(GetConnectorQuery request, CancellationToken cancellationToken)
    {
        return await _appIntegrationDbContext.Connectors
            .ProjectToType<ConnectorDetailsDto>()
            .FirstOrDefaultAsync(c => c.Id == request.ConnectorId, cancellationToken);
    }
}
