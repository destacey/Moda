using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Moda.AppIntegration.Application.Connections.Queries;
public sealed record GetConnectionQuery(Guid ConnectionId) : IQuery<ConnectionDetailsDto?>;

internal sealed class GetConnectionQueryHandler : IQueryHandler<GetConnectionQuery, ConnectionDetailsDto?>
{
    private readonly IAppIntegrationDbContext _appIntegrationDbContext;
    private readonly ILogger<GetConnectionQueryHandler> _logger;

    public GetConnectionQueryHandler(IAppIntegrationDbContext appIntegrationDbContext, ILogger<GetConnectionQueryHandler> logger)
    {
        _appIntegrationDbContext = appIntegrationDbContext;
        _logger = logger;
    }

    public async Task<ConnectionDetailsDto?> Handle(GetConnectionQuery request, CancellationToken cancellationToken)
    {
        return await _appIntegrationDbContext.Connections
            .ProjectToType<ConnectionDetailsDto>()
            .FirstOrDefaultAsync(c => c.Id == request.ConnectionId, cancellationToken);
    }
}
