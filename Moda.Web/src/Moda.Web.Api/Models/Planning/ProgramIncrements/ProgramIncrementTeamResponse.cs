using Mapster;
using Moda.Common.Application.Interfaces;
using Moda.Organization.Application.Models;
using Moda.Organization.Application.Teams.Dtos;
using Moda.Organization.Application.TeamsOfTeams.Dtos;

namespace Moda.Web.Api.Models.Planning.ProgramIncrements;

public class ProgramIncrementTeamResponse : IMapFrom<TeamListDto>, IMapFrom<TeamOfTeamsListDto>
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
    public required string Name { get; set; }

    /// <summary>Gets the code.</summary>
    /// <value>The code.</value>
    public required string Code { get; set; }

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
        config.NewConfig<TeamListDto, ProgramIncrementTeamResponse>();
        config.NewConfig<TeamOfTeamsListDto, ProgramIncrementTeamResponse>();
    }
}
