using Moda.Common.Application.Dtos;

namespace Moda.Planning.Application.PlanningIntervals.Dtos;

public sealed record PlanningIntervalListDto
{
    /// <summary>Gets or sets the identifier.</summary>
    /// <value>The identifier.</value>
    public Guid Id { get; set; }

    /// <summary>Gets the key.</summary>
    /// <value>The key.</value>
    public int Key { get; set; }

    /// <summary>
    /// The name of the planning interval.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>Planning Interval start date</summary>
    /// <value>The start.</value>
    public LocalDate Start { get; set; }

    /// <summary>Planning Interval end date</summary>
    /// <value>The end.</value>
    public LocalDate End { get; set; }

    /// <summary>
    /// The current state of the Planning Interval.
    /// </summary>
    public required SimpleNavigationDto State { get; set; }

    // TODO: do this with Mapster
    public static PlanningIntervalListDto Create(PlanningInterval planningInterval, IDateTimeProvider dateTimeProvider)
    {
        var state = planningInterval.StateOn(dateTimeProvider.Now.InUtc().Date);
        return new PlanningIntervalListDto()
        {
            Id = planningInterval.Id,
            Key = planningInterval.Key,
            Name = planningInterval.Name,
            Start = planningInterval.DateRange.Start,
            End = planningInterval.DateRange.End,
            State = SimpleNavigationDto.FromEnum(state)
        };
    }
}
