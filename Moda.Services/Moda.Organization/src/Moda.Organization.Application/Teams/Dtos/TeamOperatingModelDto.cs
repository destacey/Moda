using System.ComponentModel.DataAnnotations;
using Mapster;
using NodaTime;

namespace Moda.Organization.Application.Teams.Dtos;

/// <summary>
/// Represents the operating model for a team.
/// </summary>
public sealed record TeamOperatingModelDto : IMapFrom<TeamOperatingModel>
{
    /// <summary>
    /// The identifier of the operating model.
    /// </summary>
    [Required]
    public Guid Id { get; set; }

    /// <summary>
    /// The identifier of the team.
    /// </summary>
    [Required]
    public Guid TeamId { get; set; }

    /// <summary>
    /// The start date of the operating model.
    /// </summary>
    [Required]
    public LocalDate Start { get; set; }

    /// <summary>
    /// The end date of the operating model. Null indicates the model is current.
    /// </summary>
    public LocalDate? End { get; set; }

    /// <summary>
    /// The methodology the team uses (e.g., Scrum, Kanban).
    /// </summary>
    [Required]
    public required string Methodology { get; set; }

    /// <summary>
    /// The sizing method the team uses (e.g., Story Points, Count).
    /// </summary>
    [Required]
    public required string SizingMethod { get; set; }

    /// <summary>
    /// Indicates whether this operating model is current (has no end date).
    /// </summary>
    [Required]
    public bool IsCurrent { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<TeamOperatingModel, TeamOperatingModelDto>()
            .Map(dest => dest.Start, src => src.DateRange.Start)
            .Map(dest => dest.End, src => src.DateRange.End)
            .Map(dest => dest.Methodology, src => src.Methodology.GetDisplayName())
            .Map(dest => dest.SizingMethod, src => src.SizingMethod.GetDisplayName())
            .Map(dest => dest.IsCurrent, src => src.IsCurrent);
    }
}
