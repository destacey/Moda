using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Moda.AppIntegration.Application.Connectors.Queries;
public sealed record GetConnectorsQuery : IQuery<IReadOnlyList<ConnectorListDto>>
{
    public GetConnectorsQuery(bool includeInactive = false, ConnectorType? type = null)
    {
        IncludeInactive = includeInactive;
        Type = type;
    }

    public bool IncludeInactive { get; }
    public ConnectorType? Type { get; }
}

internal sealed class GetConnectorsQueryHandler : IQueryHandler<GetConnectorsQuery, IReadOnlyList<ConnectorListDto>>
{
    private readonly IAppIntegrationDbContext _appIntegrationDbContext;

    public GetConnectorsQueryHandler(IAppIntegrationDbContext appIntegrationDbContext)
    {
        _appIntegrationDbContext = appIntegrationDbContext;
    }

    public async Task<IReadOnlyList<ConnectorListDto>> Handle(GetConnectorsQuery request, CancellationToken cancellationToken)
    {
        var query = _appIntegrationDbContext.Connectors.AsQueryable();

        if (!request.IncludeInactive)
            query = query.Where(c => c.IsActive);

        if (request.Type.HasValue)
            query = query.Where(c => c.Type == request.Type.Value);

        return await query.ProjectToType<ConnectorListDto>().ToListAsync(cancellationToken);
    }
}
