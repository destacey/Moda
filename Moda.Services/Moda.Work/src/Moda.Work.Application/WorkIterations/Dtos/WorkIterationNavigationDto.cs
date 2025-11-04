using Moda.Common.Application.Dtos;
using Moda.Work.Application.WorkTeams.Dtos;

namespace Moda.Work.Application.WorkIterations.Dtos;
public sealed record WorkIterationNavigationDto : NavigationDto, IMapFrom<WorkIteration>
{
    public WorkTeamNavigationDto? Team { get; set; }
}
