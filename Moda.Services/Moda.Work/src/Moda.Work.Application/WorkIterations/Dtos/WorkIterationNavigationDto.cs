using Moda.Common.Application.Dtos;
using Moda.Work.Application.WorkTeams.Dtos;

namespace Moda.Work.Application.WorkIterations.Dtos;
public sealed record WorkIterationNavigationDto : NavigationDto, IMapFrom<WorkIteration>
{
    public WorkTeamNavigationDto? Team { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<WorkIteration, WorkIterationNavigationDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Key, src => src.Key)
            .Map(dest => dest.Name, src => src.Name);

        // TODO: revisit mapping for Team, neither of these are working
        //.Map(dest => dest.Team, src => src.Team);        
        //.Map(dest => dest.Team, src => src.Team == null ? null : WorkTeamNavigationDto.From(src.Team));
    }
}
