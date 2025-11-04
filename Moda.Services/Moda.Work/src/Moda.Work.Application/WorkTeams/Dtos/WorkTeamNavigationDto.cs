using Moda.Common.Application.Dtos;

namespace Moda.Work.Application.WorkTeams.Dtos;
public record WorkTeamNavigationDto : NavigationDto, IMapFrom<WorkTeam>
{
    public required string Code { get; set; }
    public required string Type { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<WorkTeam, WorkTeamNavigationDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Key, src => src.Key)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Code, src => src.Code)
            .Map(dest => dest.Type, src => src.Type.GetDisplayName());
    }

    public static WorkTeamNavigationDto From(WorkTeam team)
        => new()
        {
            Id = team.Id,
            Key = team.Key,
            Name = team.Name,
            Code = team.Code,
            Type = team.Type.GetDisplayName()
        };
}