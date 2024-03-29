﻿using Mapster;
using Moda.Common.Application.Dtos;

namespace Moda.Organization.Application.Models;
public record TeamNavigationDto : NavigationDto, IMapFrom<BaseTeam>
{
    public required string Type { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<BaseTeam, TeamNavigationDto>()
            .Map(dest => dest.Type, src => src.Type.GetDisplayName());
    }

    public static TeamNavigationDto FromBaseTeam(BaseTeam team)
    {
        return new TeamNavigationDto()
        {
            Id = team.Id,
            Key = team.Key,
            Name = team.Name,
            Type = team.Type.GetDisplayName()
        };
    }
}
