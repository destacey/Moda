using Mapster;
using Microsoft.EntityFrameworkCore;
using Moda.Common.Application.Interfaces;
using Moda.Health.Dtos;

namespace Moda.Health.Queries;
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
