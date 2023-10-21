using Mapster;
using Moda.Organization.Application.Models;

namespace Moda.Organization.Application.Teams.Dtos;
public sealed record TeamDetailsDto : IMapFrom<BaseTeam>
{
    /// <summary>Gets or sets the identifier.</summary>
    /// <value>The identifier.</value>
    public Guid Id { get; set; }

    /// <summary>Gets the key.</summary>
    /// <value>The key.</value>
    public int Key { get; set; }

    /// <summary>
    /// The name of the team.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>Gets the code.</summary>
    /// <value>The code.</value>
    public required string Code { get; set; }

    /// <summary>
    /// The description of the team.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>Gets the team type.</summary>
    /// <value>The team type.</value>
    public required string Type { get; set; }

    /// <summary>
    /// Indicates whether the organization is active or not.  
    /// </summary>
    public bool IsActive { get; set; }

    public TeamNavigationDto? TeamOfTeams { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<BaseTeam, TeamDetailsDto>()
            .Map(dest => dest.Code, src => src.Code.Value)
            .Map(dest => dest.Type, src => src.Type.GetDisplayName())
            .Map(dest => dest.TeamOfTeams, src => src.ParentMemberships == null ? null : src.ParentMemberships.FirstOrDefault()!.Target);
    }
}
