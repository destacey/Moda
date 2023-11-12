using Mapster;
using Microsoft.EntityFrameworkCore;
using Moda.Common.Application.Interfaces;
using Moda.Health.Dtos;

namespace Moda.Health.Queries;
public sealed record GetHealthReportQuery(Guid ObjectId) : IQuery<IReadOnlyList<HealthCheckDto>>;

internal sealed class GetHealthReportQueryHandler : IQueryHandler<GetHealthReportQuery, IReadOnlyList<HealthCheckDto>>
{
    private readonly IHealthDbContext _healthDbContext;

    public GetHealthReportQueryHandler(IHealthDbContext healthDbContext)
    {
        _healthDbContext = healthDbContext;
    }

    public async Task<IReadOnlyList<HealthCheckDto>> Handle(GetHealthReportQuery request, CancellationToken cancellationToken)
    {
        var healthChecks = await _healthDbContext.HealthChecks
            .AsNoTracking()
            .Where(h => h.ObjectId == request.ObjectId)
            .ProjectToType<HealthCheckDto>()
            .ToListAsync(cancellationToken);

        return (IReadOnlyList<HealthCheckDto>)(healthChecks?.OrderByDescending(h => h.ReportedOn).ToList() ?? Enumerable.Empty<HealthCheckDto>());
    }
}
