using Moda.Common.Extensions;

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

    /// <summary>Gets or sets the state.</summary>
    /// <value>The state.</value>
    public required string State { get; set; }

    // TODO: do this with Mapster
    public static PlanningIntervalListDto Create(PlanningInterval planningInterval, IDateTimeService dateTimeService)
    {
        return new PlanningIntervalListDto()
        {
            Id = planningInterval.Id,
            Key = planningInterval.Key,
            Name = planningInterval.Name,
            Start = planningInterval.DateRange.Start,
            End = planningInterval.DateRange.End,
            State = planningInterval.StateOn(dateTimeService.Now.InUtc().Date).GetDisplayName()
        };
    }
}
