using Mapster;
using Microsoft.EntityFrameworkCore;
using Wayd.Common.Application.Interfaces;
using Wayd.Health.Dtos;

namespace Wayd.Health.Queries;

public sealed record GetHealthChecksQuery(IEnumerable<Guid> Ids) : IQuery<IReadOnlyList<HealthCheckDto>>;

internal sealed class GetHealthChecksQueryHandler(IHealthDbContext healthDbContext) : IQueryHandler<GetHealthChecksQuery, IReadOnlyList<HealthCheckDto>>
{
    private readonly IHealthDbContext _healthDbContext = healthDbContext;

    public async Task<IReadOnlyList<HealthCheckDto>> Handle(GetHealthChecksQuery request, CancellationToken cancellationToken)
    {
        if (request.Ids is null || !request.Ids.Any())
            return [];

        return await _healthDbContext.HealthChecks
            .AsNoTracking()
            .Where(h => request.Ids.Contains(h.Id))
            .ProjectToType<HealthCheckDto>()
            .ToListAsync(cancellationToken);
    }
}
