using Mapster;
using Microsoft.EntityFrameworkCore;
using Moda.AppIntegration.Application.Connections.Dtos.AzureDevOps;
using Moda.AppIntegration.Application.Connections.Dtos.AzureOpenAI;
using Moda.AppIntegration.Domain.Models.AzureOpenAI;
using Moda.Common.Domain.Enums.AppIntegrations;

namespace Moda.AppIntegration.Application.Connections.Queries;
public sealed record GetConnectionsQuery(bool IncludeInactive = false, Connector? Type = null) : IQuery<IReadOnlyList<ConnectionListDto>>;

internal sealed class GetConnectionsQueryHandler(IAppIntegrationDbContext appIntegrationDbContext) : IQueryHandler<GetConnectionsQuery, IReadOnlyList<ConnectionListDto>>
{
    private readonly IAppIntegrationDbContext _appIntegrationDbContext = appIntegrationDbContext;

    public async Task<IReadOnlyList<ConnectionListDto>> Handle(GetConnectionsQuery request, CancellationToken cancellationToken)
    {
        var query = _appIntegrationDbContext.Connections.AsQueryable();

        if (!request.IncludeInactive)
            query = query.Where(c => c.IsActive);

        if (request.Type.HasValue)
            query = query.Where(c => c.Connector == request.Type.Value);

        // Load to memory first to avoid Mapster projection issues with ISyncableConnection interface
        var connections = await query.ToListAsync(cancellationToken);

        // Map each connection to its appropriate derived DTO type for polymorphic serialization
        return connections.Select(connection => connection switch
        {
            AzureDevOpsBoardsConnection azdo => (ConnectionListDto)azdo.Adapt<AzureDevOpsConnectionListDto>(),
            AzureOpenAIConnection aoai => aoai.Adapt<AzureOpenAIConnectionListDto>(),
            _ => connection.Adapt<ConnectionListDto>()
        }).ToList();
    }
}
