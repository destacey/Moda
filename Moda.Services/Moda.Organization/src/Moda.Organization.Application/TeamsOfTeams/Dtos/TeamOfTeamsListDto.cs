﻿using System.ComponentModel.DataAnnotations;
using Mapster;
using Moda.Organization.Application.Models;

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

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<BaseTeam, TeamOfTeamsListDto>()
            .Map(dest => dest.Code, src => src.Code.Value)
            .Map(dest => dest.Type, src => src.Type.GetDisplayName())
            .Map(dest => dest.TeamOfTeams, src => src.ParentMemberships == null ? null : src.ParentMemberships.FirstOrDefault()!.Target);
    }
}
