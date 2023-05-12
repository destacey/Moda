using Moda.Common.Application.Dtos;
using Moda.Common.Extensions;

namespace Moda.Planning.Application.Models;
public record TeamNavigationDto : NavigationDto, IMapFrom<PlanningTeam>
{
    public required string Type { get; set; }

    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<PlanningTeam, TeamNavigationDto>()
            .Map(dest => dest.Type, src => src.Type.GetDisplayName());
    }

    public static TeamNavigationDto FromPlanningTeam(PlanningTeam team)
    {
        return new TeamNavigationDto()
        {
            Id = team.Id,
            LocalId = team.LocalId,
            Name = team.Name,
            Type = team.Type.GetDisplayName()
        };
    }
}
