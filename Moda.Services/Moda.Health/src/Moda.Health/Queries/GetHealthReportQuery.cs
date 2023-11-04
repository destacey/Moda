using Mapster;
using Microsoft.EntityFrameworkCore;
using Moda.Common.Application.Interfaces;
using Moda.Health.Dtos;

namespace Moda.Health.Queries;
public sealed record GetHealthReportQuery(Guid ObjectId) : IQuery<HealthReportDto?>;

internal sealed class GetHealthReportQueryHandler : IQueryHandler<GetHealthReportQuery, HealthReportDto?>
{
    private readonly IHealthDbContext _healthDbContext;

    public GetHealthReportQueryHandler(IHealthDbContext healthDbContext)
    {
        _healthDbContext = healthDbContext;
    }

    public async Task<HealthReportDto?> Handle(GetHealthReportQuery request, CancellationToken cancellationToken)
    {
        var healthChecks = await _healthDbContext.HealthChecks
            .Where(hr => hr.ObjectId == request.ObjectId)
            .ProjectToType<HealthCheckDto>()
            .ToListAsync(cancellationToken);

        return !healthChecks.Any() ? null : new HealthReportDto(healthChecks);
    }
}
