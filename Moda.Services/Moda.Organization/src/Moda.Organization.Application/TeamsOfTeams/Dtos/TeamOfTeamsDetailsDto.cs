using System.ComponentModel.DataAnnotations;
using Mapster;
using Moda.Organization.Application.Models;
using NodaTime;

namespace Moda.Organization.Application.TeamsOfTeams.Dtos;
public class TeamOfTeamsDetailsDto : IMapFrom<BaseTeam>
{
    /// <summary>
    /// The identifier of the team.
    /// </summary>
    [Required]
    public Guid Id { get; set; }

    /// <summary>
    /// The key of the team.
    /// </summary>
    [Required]
    public int Key { get; set; }

    /// <summary>
    /// The name of the team.
    /// </summary>
    [Required]
    public required string Name { get; set; }

    /// <summary>
    /// The code of the team.
    /// </summary>
    [Required]
    public required string Code { get; set; }

    /// <summary>
    /// The description of the team.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The type of team.
    /// </summary>
    [Required]
    public required string Type { get; set; }

    /// <summary>
    /// The date for when the team became active.
    /// </summary>
    public LocalDate ActiveDate { get; set; }

    /// <summary>
    /// The date for when the team became inactive.
    /// </summary>
    public LocalDate? InactiveDate { get; set; }

    /// <summary>
    /// Indicates whether the team is active or not.  
    /// </summary>
    [Required]
    public bool IsActive { get; set; }

    /// <summary>
    /// The parent team of the team.
    /// </summary>
    public TeamNavigationDto? TeamOfTeams { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<BaseTeam, TeamOfTeamsDetailsDto>()
            .Map(dest => dest.Code, src => src.Code.Value)
            .Map(dest => dest.Type, src => src.Type.GetDisplayName())
            .Map(dest => dest.TeamOfTeams, src => src.ParentMemberships == null ? null : src.ParentMemberships.FirstOrDefault()!.Target);
    }
}
