using Moda.Common.Application.Dtos;
using Moda.Planning.Application.Models;
using Moda.Planning.Domain.Models.Iterations;

namespace Moda.Planning.Application.Iterations.Dtos;
public sealed record SprintDetailsDto : IMapFrom<Iteration>
{
    public Guid Id { get; set; }

    /// <summary>
    /// The unique key of the sprint.  This is an alternate key to the Id.
    /// </summary>
    public int Key { get; set; }

    /// <summary>
    /// The name of the sprint.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The current state of the sprint.
    /// </summary>
    public required SimpleNavigationDto State { get; set; }

    /// <summary>
    /// The sprint start date.
    /// </summary>
    public Instant Start { get; set; }

    /// <summary>
    /// The sprint end date.
    /// </summary>
    public Instant End { get; set; }

    public required PlanningTeamNavigationDto Team { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<Iteration, SprintDetailsDto>()
            .Map(dest => dest.State, src => SimpleNavigationDto.FromEnum(src.State))
            .Map(dest => dest.Start, src => src.DateRange.Start)
            .Map(dest => dest.End, src => src.DateRange.End)
            .Map(dest => dest.Team, src => PlanningTeamNavigationDto.FromPlanningTeam(src.Team!));
    }
}
