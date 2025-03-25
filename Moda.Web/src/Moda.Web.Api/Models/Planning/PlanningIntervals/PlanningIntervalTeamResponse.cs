using Mapster;
using Moda.Organization.Application.Models;
using Moda.Organization.Application.Teams.Dtos;
using Moda.Organization.Application.TeamsOfTeams.Dtos;

namespace Moda.Web.Api.Models.Planning.PlanningIntervals;

public class PlanningIntervalTeamResponse : IMapFrom<TeamListDto>, IMapFrom<TeamOfTeamsListDto>
{
    /// <summary>Gets or sets the identifier.</summary>
    /// <value>The identifier.</value>
    public Guid Id { get; set; }

    /// <summary>Gets the key.</summary>
    /// <value>The key.</value>
    public int Key { get; set; }

    /// <summary>
    /// The name of the workspace.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>Gets the code.</summary>
    /// <value>The code.</value>
    public string Code { get; set; } = default!;

    /// <summary>Gets the team type.</summary>
    /// <value>The team type.</value>
    public string Type { get; set; } = default!;

    /// <summary>
    /// Indicates whether the organization is active or not.  
    /// </summary>
    public bool IsActive { get; set; }

    public TeamNavigationDto? TeamOfTeams { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<TeamListDto, PlanningIntervalTeamResponse>();
        config.NewConfig<TeamOfTeamsListDto, PlanningIntervalTeamResponse>();
    }
}
