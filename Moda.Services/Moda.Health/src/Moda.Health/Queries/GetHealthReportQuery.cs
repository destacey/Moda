using Mapster;
using Microsoft.EntityFrameworkCore;
using Wayd.Common.Application.Interfaces;
using Wayd.Health.Dtos;

namespace Wayd.Health.Queries;

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
