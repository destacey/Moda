using Mapster;
using Moda.Organization.Application.Models;
using NodaTime;

namespace Moda.Organization.Application.Teams.Dtos;
public sealed record TeamDetailsDto : IMapFrom<BaseTeam>
{
    /// <summary>
    /// The identifier of the team.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The key of the team.
    /// </summary>
    public int Key { get; set; }

    /// <summary>
    /// The name of the team.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The code of the team.
    /// </summary>
    public required string Code { get; set; }

    /// <summary>
    /// The description of the team.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The type of team.
    /// </summary>
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
    public bool IsActive { get; set; }

    /// <summary>
    /// The parent team of the team.
    /// </summary>
    public TeamNavigationDto? TeamOfTeams { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<BaseTeam, TeamDetailsDto>()
            .Map(dest => dest.Code, src => src.Code.Value)
            .Map(dest => dest.Type, src => src.Type.GetDisplayName())
            .Map(dest => dest.TeamOfTeams, src => src.ParentMemberships == null ? null : src.ParentMemberships.FirstOrDefault()!.Target);
    }
}
