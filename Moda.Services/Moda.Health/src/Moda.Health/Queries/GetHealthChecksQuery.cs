using Mapster;
using Microsoft.EntityFrameworkCore;
using Moda.Common.Application.Interfaces;
using Moda.Health.Dtos;

namespace Moda.Health.Queries;
public sealed record GetHealthChecksQuery(IEnumerable<Guid> Ids) : IQuery<IReadOnlyList<HealthCheckDto>>;

internal sealed class GetHealthChecksQueryHandler : IQueryHandler<GetHealthChecksQuery, IReadOnlyList<HealthCheckDto>>
{
    private readonly IHealthDbContext _healthDbContext;

    public GetHealthChecksQueryHandler(IHealthDbContext healthDbContext)
    {
        _healthDbContext = healthDbContext;
    }

    public async Task<IReadOnlyList<HealthCheckDto>> Handle(GetHealthChecksQuery request, CancellationToken cancellationToken)
    {
        if (request.Ids is null || !request.Ids.Any())
            return new List<HealthCheckDto>();

        return await _healthDbContext.HealthChecks
            .AsNoTracking()
            .Where(h => request.Ids.Contains(h.Id))
            .ProjectToType<HealthCheckDto>()
            .ToListAsync(cancellationToken);
    }
}
