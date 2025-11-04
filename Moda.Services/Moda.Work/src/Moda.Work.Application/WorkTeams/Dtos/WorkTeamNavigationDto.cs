using Moda.Common.Application.Dtos;

namespace Moda.Work.Application.WorkTeams.Dtos;
public record WorkTeamNavigationDto : NavigationDto, IMapFrom<WorkTeam>
{
    public required string Code { get; set; }
    public required string Type { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<WorkTeam, WorkTeamNavigationDto>()
            .Map(dest => dest.Type, src => src.Type.GetDisplayName());
    }

    public static WorkTeamNavigationDto FromWorkTeam(WorkTeam team)
    {
        return new WorkTeamNavigationDto()
        {
            Id = team.Id,
            Key = team.Key,
            Name = team.Name,
            Code = team.Code,
            Type = team.Type.GetDisplayName()
        };
    }
}