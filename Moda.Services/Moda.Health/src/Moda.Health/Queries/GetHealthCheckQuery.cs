using Mapster;
using Microsoft.EntityFrameworkCore;
using Wayd.Common.Application.Interfaces;
using Wayd.Health.Dtos;

namespace Wayd.Health.Queries;

public sealed record GetHealthCheckQuery(Guid Id) : IQuery<HealthCheckDto?>;

internal sealed class GetHealthCheckQueryHandler : IQueryHandler<GetHealthCheckQuery, HealthCheckDto?>
{
    private readonly IHealthDbContext _healthDbContext;

    public GetHealthCheckQueryHandler(IHealthDbContext healthDbContext)
    {
        _healthDbContext = healthDbContext;
    }

    public async Task<HealthCheckDto?> Handle(GetHealthCheckQuery request, CancellationToken cancellationToken)
    {
        return await _healthDbContext.HealthChecks
            .AsNoTracking()
            .ProjectToType<HealthCheckDto>()
            .FirstOrDefaultAsync(h => h.Id == request.Id, cancellationToken);
    }
}
