using Mapster;
using Moda.Common.Application.Dtos;
using Moda.Health.Models;
using NodaTime;

namespace Moda.Health.Dtos;
public sealed record HealthCheckDto : IMapFrom<HealthCheck>
{
    /// <summary>
    /// The id of the health check.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The objectId associated with the health check.
    /// </summary>
    public Guid ObjectId { get; set; }

    /// <summary>
    /// The status of the health check.
    /// </summary>
    public required SimpleNavigationDto Status { get; set; }

    /// <summary>
    /// The employee who reported the health check.
    /// </summary>
    public required SimpleNavigationDto ReportedBy { get; set; }

    /// <summary>
    /// The timestamp of when the health check was initially created.
    /// </summary>
    public Instant ReportedOn { get; set; }

    /// <summary>
    /// The expiration of the health check.
    /// </summary>
    public Instant Expiration { get; set; }

    /// <summary>
    /// The note for the health check.
    /// </summary>
    public string? Note { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<HealthCheck, HealthCheckDto>()
            .Map(dest => dest.Status, src => SimpleNavigationDto.FromEnum(src.Status))
            .Map(dest => dest.ReportedBy, src => NavigationDto.Create(src.ReportedBy.Id, src.ReportedBy.Key, src.ReportedBy.Name.FullName));
    }
}
