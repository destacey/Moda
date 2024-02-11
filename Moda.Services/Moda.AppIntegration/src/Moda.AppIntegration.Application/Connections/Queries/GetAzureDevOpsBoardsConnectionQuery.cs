using Mapster;
using Microsoft.EntityFrameworkCore;

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
        var connection = await _appIntegrationDbContext.AzureDevOpsBoardsConnections
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.ConnectionId, cancellationToken);

        return connection?.Adapt<AzureDevOpsBoardsConnectionDetailsDto>();

        // TODO: using ProjectTo is not working with the JSON Column - .ProjectToType<AzureDevOpsBoardsConnectionDetailsDto>()
    }
}
