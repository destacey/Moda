using Mapster;
using Microsoft.EntityFrameworkCore;
using Moda.AppIntegration.Application.Connections.Dtos.AzureDevOps;
using Moda.AppIntegration.Application.Connections.Dtos.AzureOpenAI;
using Moda.AppIntegration.Domain.Models.AzureOpenAI;

namespace Moda.AppIntegration.Application.Connections.Queries;

public sealed record GetConnectionQuery(Guid Id) : IQuery<ConnectionDetailsDto?>;

internal sealed class GetConnectionQueryHandler(IAppIntegrationDbContext appIntegrationDbContext) : IQueryHandler<GetConnectionQuery, ConnectionDetailsDto?>
{
    private readonly IAppIntegrationDbContext _appIntegrationDbContext = appIntegrationDbContext;

    public async Task<ConnectionDetailsDto?> Handle(GetConnectionQuery request, CancellationToken cancellationToken)
    {
        var connection = await _appIntegrationDbContext.Connections
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsDeleted, cancellationToken);

        if (connection == null)
        {
            return null;
        }

        return connection switch
        {
            AzureDevOpsBoardsConnection => connection.Adapt<AzureDevOpsConnectionDetailsDto>(),
            AzureOpenAIConnection => connection.Adapt<AzureOpenAIConnectionDetailsDto>(),
            // case OpenAIConnection:
            _ => connection.Adapt<ConnectionDetailsDto>(),
        };

        // connection?.Adapt<ConnectionDetailsDto>();  // TODO: this is not working
    }
}
