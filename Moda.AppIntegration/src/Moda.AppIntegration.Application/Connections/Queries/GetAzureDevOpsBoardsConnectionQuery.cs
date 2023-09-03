using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Moda.AppIntegration.Application.Connections.Queries;
public sealed record GetAzureDevOpsBoardsConnectionQuery(Guid ConnectionId) : IQuery<AzureDevOpsBoardsConnectionDetailsDto?>;

internal sealed class GetAzureDevOpsBoardsConnectionQueryHandler : IQueryHandler<GetAzureDevOpsBoardsConnectionQuery, AzureDevOpsBoardsConnectionDetailsDto?>
{
    private readonly IAppIntegrationDbContext _appIntegrationDbContext;
    private readonly ILogger<GetAzureDevOpsBoardsConnectionQueryHandler> _logger;

    public GetAzureDevOpsBoardsConnectionQueryHandler(IAppIntegrationDbContext appIntegrationDbContext, ILogger<GetAzureDevOpsBoardsConnectionQueryHandler> logger)
    {
        _appIntegrationDbContext = appIntegrationDbContext;
        _logger = logger;
    }

    public async Task<AzureDevOpsBoardsConnectionDetailsDto?> Handle(GetAzureDevOpsBoardsConnectionQuery request, CancellationToken cancellationToken)
    {
        return await _appIntegrationDbContext.AzureDevOpsBoardsConnections
            .ProjectToType<AzureDevOpsBoardsConnectionDetailsDto>()
            .FirstOrDefaultAsync(c => c.Id == request.ConnectionId, cancellationToken);
    }
}
