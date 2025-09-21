using Mapster;
using Microsoft.EntityFrameworkCore;
using Moda.Common.Domain.Enums.AppIntegrations;

namespace Moda.AppIntegration.Application.Connections.Queries;
public sealed record GetConnectionsQuery : IQuery<IReadOnlyList<ConnectionListDto>>
{
    public GetConnectionsQuery(bool includeInactive = false, Connector? type = null)
    {
        IncludeInactive = includeInactive;
        Type = type;
    }

    public bool IncludeInactive { get; }
    public Connector? Type { get; }
}

internal sealed class GetConnectionsQueryHandler : IQueryHandler<GetConnectionsQuery, IReadOnlyList<ConnectionListDto>>
{
    private readonly IAppIntegrationDbContext _appIntegrationDbContext;

    public GetConnectionsQueryHandler(IAppIntegrationDbContext appIntegrationDbContext)
    {
        _appIntegrationDbContext = appIntegrationDbContext;
    }

    public async Task<IReadOnlyList<ConnectionListDto>> Handle(GetConnectionsQuery request, CancellationToken cancellationToken)
    {
        var query = _appIntegrationDbContext.Connections.AsQueryable();

        if (!request.IncludeInactive)
            query = query.Where(c => c.IsActive);

        if (request.Type.HasValue)
            query = query.Where(c => c.Connector == request.Type.Value);

        return await query.ProjectToType<ConnectionListDto>().ToListAsync(cancellationToken);
    }
}
