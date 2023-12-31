using Moda.Common.Extensions;

namespace Moda.Planning.Application.PlanningIntervals.Dtos;

public sealed record PlanningIntervalDetailsDto
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

    /// <summary>Gets or sets the description.</summary>
    /// <value>The description.</value>
    public string? Description { get; set; }

    /// <summary>Gets or sets the start.</summary>
    /// <value>The start.</value>
    public required LocalDate Start { get; set; }

    /// <summary>Gets or sets the end.</summary>
    /// <value>The end.</value>
    public required LocalDate End { get; set; }

    /// <summary>Gets or sets the state.</summary>
    /// <value>The state.</value>
    public required string State { get; set; }

    /// <summary>Gets or sets a value indicating whether [objectives locked].</summary>
    /// <value><c>true</c> if [objectives locked]; otherwise, <c>false</c>.</value>
    public bool ObjectivesLocked { get; set; }

    public double? Predictability { get; set; }

    // TODO: do this with Mapster
    public static PlanningIntervalDetailsDto Create(PlanningInterval planningInterval, IDateTimeService dateTimeService)
    {
        return new PlanningIntervalDetailsDto()
        {
            Id = planningInterval.Id,
            Key = planningInterval.Key,
            Name = planningInterval.Name,
            Description = planningInterval.Description,
            Start = planningInterval.DateRange.Start,
            End = planningInterval.DateRange.End,
            State = planningInterval.StateOn(dateTimeService.Now.InUtc().Date).GetDisplayName(),
            ObjectivesLocked = planningInterval.ObjectivesLocked,
            Predictability = planningInterval.CalculatePredictability(dateTimeService.Now.InUtc().Date)
        };
    }
}
