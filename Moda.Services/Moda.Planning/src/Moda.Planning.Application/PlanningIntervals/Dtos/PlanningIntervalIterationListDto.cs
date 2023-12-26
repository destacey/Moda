using Moda.Common.Application.Dtos;

namespace Moda.Planning.Application.PlanningIntervals.Dtos;
public sealed record PlanningIntervalIterationListDto : IMapFrom<PlanningIntervalIteration>
{
    /// <summary>Gets or sets the identifier.</summary>
    /// <value>The identifier.</value>
    public Guid Id { get; set; }

    /// <summary>Gets the key.</summary>
    /// <value>The key.</value>
    public int Key { get; set; }

    /// <summary>
    /// The name of the objective.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>Gets or sets the start.</summary>
    /// <value>The start.</value>
    public required LocalDate Start { get; set; }

    /// <summary>Gets or sets the end.</summary>
    /// <value>The end.</value>
    public required LocalDate End { get; set; }

    /// <summary>
    /// The iteration type.
    /// </summary>
    public required SimpleNavigationDto Type { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<PlanningIntervalIteration, PlanningIntervalIterationListDto>()
            .Map(dest => dest.Start, src => src.DateRange.Start)
            .Map(dest => dest.End, src => src.DateRange.End)
            .Map(dest => dest.Type, src => SimpleNavigationDto.FromEnum(src.Type));
    }
}
