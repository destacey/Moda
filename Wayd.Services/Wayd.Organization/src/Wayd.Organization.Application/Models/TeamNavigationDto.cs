using Mapster;
using Wayd.Common.Application.Dtos;

namespace Wayd.Organization.Application.Models;

public record TeamNavigationDto : NavigationDto, IMapFrom<BaseTeam>
{
    public required string Code { get; set; }
    public required string Type { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<BaseTeam, TeamNavigationDto>()
            .Map(dest => dest.Code, src => src.Code.Value)
            .Map(dest => dest.Type, src => src.Type.GetDisplayName());
    }

    public static TeamNavigationDto FromBaseTeam(BaseTeam team)
    {
        return new TeamNavigationDto()
        {
            Id = team.Id,
            Key = team.Key,
            Name = team.Name,
            Code = team.Code.Value,
            Type = team.Type.GetDisplayName()
        };
    }
}
