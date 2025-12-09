using Moda.Common.Domain.Enums;
using NodaTime;

namespace Moda.Health.Models;
public sealed class HealthReport
{
    private readonly List<HealthCheck> _healthChecks = [];

    private HealthReport() { }
    public HealthReport(List<HealthCheck> healthChecks)
    {
        _healthChecks = [.. healthChecks.OrderByDescending(h => h.ReportedOn)];
    }

    public IReadOnlyCollection<HealthCheck> HealthChecks => _healthChecks.AsReadOnly();

    /// <summary>
    /// Add a health check to a health report.
    /// </summary>
    /// <param name="objectId"></param>
    /// <param name="context"></param>
    /// <param name="status"></param>
    /// <param name="reportedById"></param>
    /// <param name="reportedOn"></param>
    /// <param name="expiration"></param>
    /// <param name="note"></param>
    /// <returns></returns>
    internal HealthCheck AddHealthCheck(Guid objectId, SystemContext context, HealthStatus status, Guid reportedById, Instant reportedOn, Instant expiration, string? note)
    {
        var healthCheck = HealthCheck.Create(objectId, context, status, reportedById, reportedOn, expiration, note, reportedOn);

        // health checks should not overlap
        // expire latest health check if not expired
        var latestHealthCheck = _healthChecks.FirstOrDefault();
        if (latestHealthCheck is not null && reportedOn < latestHealthCheck.Expiration)
        {
            latestHealthCheck.ChangeExpiration(reportedOn);
        }

        _healthChecks.Add(healthCheck);

        return healthCheck;
    }

}
