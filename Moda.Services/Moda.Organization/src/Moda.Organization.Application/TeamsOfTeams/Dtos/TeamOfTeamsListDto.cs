using System.ComponentModel.DataAnnotations;
using Mapster;
using Moda.Organization.Application.Models;
using NodaTime;

namespace Moda.Organization.Application.TeamsOfTeams.Dtos;
public class TeamOfTeamsListDto : IMapFrom<BaseTeam>
{
    /// <summary>Gets or sets the identifier.</summary>
    /// <value>The identifier.</value>
    [Required]
    public Guid Id { get; set; }

    /// <summary>Gets the key.</summary>
    /// <value>The key.</value>
    [Required]
    public int Key { get; set; }

    /// <summary>
    /// The name of the workspace.
    /// </summary>
    [Required]
    public required string Name { get; set; }

    /// <summary>Gets the code.</summary>
    /// <value>The code.</value>
    [Required]
    public required string Code { get; set; }

    /// <summary>Gets the team type.</summary>
    /// <value>The team type.</value>
    [Required]
    public required string Type { get; set; }

    /// <summary>
    /// Indicates whether the organization is active or not.  
    /// </summary>
    [Required]
    public bool IsActive { get; set; }

    public TeamNavigationDto? TeamOfTeams { get; set; }

    /// <summary>
    /// Create a TypeAdapterConfig configured to map BaseTeam -> TeamOfTeamsListDto using the provided
    /// asOf date to select the appropriate parent membership. Callers can pass this config to
    /// ProjectToType to perform an EF-friendly projection that uses a captured constant date.
    /// </summary>
    public static TypeAdapterConfig CreateTypeAdapterConfig(LocalDate asOf)
    {
        var cfg = new TypeAdapterConfig();

        cfg.NewConfig<BaseTeam, TeamOfTeamsListDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Key, src => src.Key)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Code, src => src.Code.Value)
            .Map(dest => dest.Type, src => src.Type.GetDisplayName())
            .Map(dest => dest.IsActive, src => src.IsActive)
            .Map(dest => dest.TeamOfTeams,
                 src => src.ParentMemberships
                            .Where(m => m.DateRange.Start <= asOf && (m.DateRange.End == null || m.DateRange.End >= asOf))
                            .Select(m => m.Target)
                            .FirstOrDefault());

        return cfg;
    }
}
