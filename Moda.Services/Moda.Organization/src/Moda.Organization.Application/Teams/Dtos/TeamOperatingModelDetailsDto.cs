using System.ComponentModel.DataAnnotations;
using Mapster;
using Moda.Organization.Domain.Enums;
using NodaTime;

namespace Moda.Organization.Application.Teams.Dtos;

/// <summary>
/// Represents the operating model for a team.
/// </summary>
public sealed record TeamOperatingModelDetailsDto : IMapFrom<TeamOperatingModel>
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
    public Methodology Methodology { get; set; }

    /// <summary>
    /// The sizing method the team uses (e.g., StoryPoints, Count).
    /// </summary>
    [Required]
    public SizingMethod SizingMethod { get; set; }

    /// <summary>
    /// Indicates whether this operating model is current (has no end date).
    /// </summary>
    [Required]
    public bool IsCurrent { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        // Note: TeamId is set manually in query handlers after loading through Team aggregate
        config.NewConfig<TeamOperatingModel, TeamOperatingModelDetailsDto>()
            .Map(dest => dest.Start, src => src.DateRange.Start)
            .Map(dest => dest.End, src => src.DateRange.End)
            .Map(dest => dest.IsCurrent, src => src.IsCurrent);
    }
}
