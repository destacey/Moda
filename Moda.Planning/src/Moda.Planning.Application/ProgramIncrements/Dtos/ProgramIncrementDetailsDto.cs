using Moda.Common.Extensions;

namespace Moda.Planning.Application.ProgramIncrements.Dtos;

public sealed record ProgramIncrementDetailsDto
{
    /// <summary>Gets or sets the identifier.</summary>
    /// <value>The identifier.</value>
    public Guid Id { get; set; }

    /// <summary>Gets the local identifier.</summary>
    /// <value>The local identifier.</value>
    public int LocalId { get; set; }

    /// <summary>
    /// The name of the program increment.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>Gets or sets the description.</summary>
    /// <value>The description.</value>
    public string? Description { get; set; }

    /// <summary>Gets or sets the start.</summary>
    /// <value>The start.</value>
    public LocalDate Start { get; set; }

    /// <summary>Gets or sets the end.</summary>
    /// <value>The end.</value>
    public LocalDate End { get; set; }

    /// <summary>Gets or sets the state.</summary>
    /// <value>The state.</value>
    public required string State { get; set; }

    /// <summary>Gets or sets a value indicating whether [objectives locked].</summary>
    /// <value><c>true</c> if [objectives locked]; otherwise, <c>false</c>.</value>
    public bool ObjectivesLocked { get; set; }

    // TODO: do this with Mapster
    public static ProgramIncrementDetailsDto Create(ProgramIncrement programIncrement, IDateTimeService dateTimeService)
    {
        return new ProgramIncrementDetailsDto()
        {
            Id = programIncrement.Id,
            LocalId = programIncrement.LocalId,
            Name = programIncrement.Name,
            Description = programIncrement.Description,
            Start = programIncrement.DateRange.Start,
            End = programIncrement.DateRange.End,
            State = programIncrement.StateOn(dateTimeService.Now.InUtc().Date).GetDisplayName(),
            ObjectivesLocked = programIncrement.ObjectivesLocked
        };
    }
}
