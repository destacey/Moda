using Moda.Common.Application.Dtos;

namespace Moda.Planning.Application.PlanningIntervals.Dtos;
public sealed record PlanningIntervalIterationListDto : IMapFrom<PlanningIntervalIteration>
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
    /// The PI iteration state.
    /// </summary>
    public required string State { get; set; }

    /// <summary>
    /// The PI iteration category.
    /// </summary>
    public required SimpleNavigationDto Category { get; set; }

    public static TypeAdapterConfig CreateTypeAdapterConfig(LocalDate asOf)
    {
        var config = new TypeAdapterConfig();

        config.NewConfig<PlanningIntervalIteration, PlanningIntervalIterationListDto>()
            .Map(dest => dest.Start, src => src.DateRange.Start)
            .Map(dest => dest.End, src => src.DateRange.End)
            .Map(dest => dest.Category, src => SimpleNavigationDto.FromEnum(src.Category))
            .Map(dest => dest.State, src =>
                asOf < src.DateRange.Start
                    ? "Future"
                    : asOf > src.DateRange.End
                        ? "Completed"
                        : "Active"
            );

        return config;
    }
}
