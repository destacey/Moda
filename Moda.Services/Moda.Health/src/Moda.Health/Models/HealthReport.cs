using Moda.Common.Domain.Enums;
using NodaTime;

namespace Moda.Health.Models;
public sealed class HealthReport
{
    private readonly List<HealthCheck> _healthChecks = new();

    private HealthReport() { }
    public HealthReport(List<HealthCheck> healthChecks)
    {
        _healthChecks = healthChecks.OrderByDescending(h => h.Timestamp).ToList();    
    }

    public IReadOnlyCollection<HealthCheck> HealthChecks => _healthChecks.AsReadOnly();

    /// <summary>
    /// Add a health check to a health report.
    /// </summary>
    /// <param name="objectId"></param>
    /// <param name="context"></param>
    /// <param name="status"></param>
    /// <param name="timestamp"></param>
    /// <param name="expiration"></param>
    /// <param name="note"></param>
    /// <returns></returns>
    internal HealthCheck AddHealthCheck(Guid objectId, HealthCheckContext context, HealthStatus status, Instant timestamp, Instant expiration, string? note)
    {
        var healthCheck = new HealthCheck(objectId, context, status, timestamp, expiration, note);

        // health checks should not overlap
        // expire latest health check if not expired
        var latestHealthCheck = _healthChecks.FirstOrDefault();
        if (latestHealthCheck is not null && timestamp < latestHealthCheck.Expiration)
        {
            latestHealthCheck.ChangeExpiration(timestamp);
        }

        _healthChecks.Add(healthCheck);

        return healthCheck;
    }

}
