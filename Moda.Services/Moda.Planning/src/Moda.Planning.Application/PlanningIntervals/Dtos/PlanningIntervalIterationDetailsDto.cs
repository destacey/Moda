using Moda.Common.Application.Dtos;

namespace Moda.Planning.Application.PlanningIntervals.Dtos;
public sealed record PlanningIntervalIterationDetailsDto : IMapFrom<PlanningIntervalIteration>
{
    /// <summary>
    /// The primary identifier of the PI iteration.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The alternate key of the PI iteration.
    /// </summary>
    public int Key { get; set; }

    /// <summary>
    /// The name of the PI iteration.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The PI iteration start date.
    /// </summary>
    public required LocalDate Start { get; set; }

    /// <summary>
    /// The PI iteration end date.
    /// </summary>
    public required LocalDate End { get; set; }

    /// <summary>
    /// The PI iteration category.
    /// </summary>
    public required SimpleNavigationDto Category { get; set; }

    public required NavigationDto PlanningInterval { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<PlanningIntervalIteration, PlanningIntervalIterationDetailsDto>()
            .Map(dest => dest.Start, src => src.DateRange.Start)
            .Map(dest => dest.End, src => src.DateRange.End)
            .Map(dest => dest.Category, src => SimpleNavigationDto.FromEnum(src.Category))
            .Map(dest => dest.PlanningInterval, src => NavigationDto.FromNavigable(src.PlanningInterval!));
    }
}
