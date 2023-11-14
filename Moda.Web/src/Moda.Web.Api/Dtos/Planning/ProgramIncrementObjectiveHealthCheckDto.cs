using Moda.Common.Application.Dtos;
using Moda.Health.Dtos;
using Moda.Planning.Application.Models;
using Moda.Planning.Application.ProgramIncrements.Dtos;

namespace Moda.Web.Api.Dtos.Planning;
public sealed record ProgramIncrementObjectiveHealthCheckDto
{
    /// <summary>Gets or sets the identifier.</summary>
    /// <value>The identifier.</value>
    public Guid Id { get; set; }

    /// <summary>Gets the key.</summary>
    /// <value>The key.</value>
    public int Key { get; set; }

    /// <summary>
    /// The name of the objective.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>Gets or sets the status.</summary>
    /// <value>The status.</value>
    public required SimpleNavigationDto Status { get; set; }

    /// <summary>Gets or sets the type.</summary>
    /// <value>The PI objective type.</value>
    public required SimpleNavigationDto Type { get; set; }

    public required NavigationDto ProgramIncrement { get; set; }

    public required PlanningTeamNavigationDto Team { get; set; }

    /// <summary>Gets a value indicating whether this instance is stretch.</summary>
    /// <value><c>true</c> if this instance is stretch; otherwise, <c>false</c>.</value>
    public bool IsStretch { get; set; }

    /// <summary>
    /// The id of the health check.
    /// </summary>
    public Guid? HealthCheckId { get; set; }

    /// <summary>
    /// The status of the health check.
    /// </summary>
    public SimpleNavigationDto? HealthStatus { get; set; }

    /// <summary>
    /// The timestamp of when the health check was initially created.
    /// </summary>
    public Instant? ReportedOn { get; set; }

    /// <summary>
    /// The expiration of the health check.
    /// </summary>
    public Instant? Expiration { get; set; }

    /// <summary>
    /// The note for the health check.
    /// </summary>
    public string? Note { get; set; }

    public static ProgramIncrementObjectiveHealthCheckDto Create(ProgramIncrementObjectiveListDto objective, HealthCheckDto? healthCheck)
    {
        return new ProgramIncrementObjectiveHealthCheckDto()
        {
            Id = objective.Id,
            Key = objective.Key,
            Name = objective.Name,
            Status = objective.Status,
            Type = objective.Type,
            ProgramIncrement = objective.ProgramIncrement,
            Team = objective.Team,
            IsStretch = objective.IsStretch,
            HealthCheckId = healthCheck?.Id,
            HealthStatus = healthCheck?.Status,
            ReportedOn = healthCheck?.ReportedOn,
            Expiration = healthCheck?.Expiration,
            Note = healthCheck?.Note
        };
    }
}
