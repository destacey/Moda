using Moda.Common.Application.Dtos;

namespace Moda.Planning.Application.ProgramIncrements.Dtos;
public sealed record PlanningHealthCheckDto
{
    /// <summary>
    /// The id associated with the health check.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The status of the health check.
    /// </summary>
    public required SimpleNavigationDto Status { get; set; }

    /// <summary>
    /// The expiration of the health check.
    /// </summary>
    public Instant Expiration { get; set; }

    public static PlanningHealthCheckDto? Create(SimpleHealthCheck healthCheck, Instant now)
    {
        return healthCheck.IsExpired(now)
            ? null
            : new PlanningHealthCheckDto()
            {
                Id = healthCheck.Id,
                Status = SimpleNavigationDto.FromEnum(healthCheck.Status),
                Expiration = healthCheck.Expiration
            };
    }
}
