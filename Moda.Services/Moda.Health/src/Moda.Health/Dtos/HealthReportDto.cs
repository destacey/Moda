namespace Moda.Health.Dtos;
public sealed record HealthReportDto
{
    public HealthReportDto(IReadOnlyCollection<HealthCheckDto> healthChecks)
    {
        HealthChecks = healthChecks.OrderByDescending(h => h.ReportedOn).ToList();
    }

    public IReadOnlyCollection<HealthCheckDto> HealthChecks { get; set; }
}
