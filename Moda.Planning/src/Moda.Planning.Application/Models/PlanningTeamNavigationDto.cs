using Moda.Common.Application.Dtos;
using Moda.Common.Extensions;

namespace Moda.Planning.Application.Models;
public record PlanningTeamNavigationDto : NavigationDto, IMapFrom<PlanningTeam>
{
    public required string Type { get; set; }

    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<PlanningTeam, PlanningTeamNavigationDto>()
            .Map(dest => dest.Type, src => src.Type.GetDisplayName());
    }

    public static PlanningTeamNavigationDto FromPlanningTeam(PlanningTeam team)
    {
        return new PlanningTeamNavigationDto()
        {
            Id = team.Id,
            Key = team.Key,
            Name = team.Name,
            Type = team.Type.GetDisplayName()
        };
    }
}
