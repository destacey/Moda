using Moda.Common.Extensions;

namespace Moda.Planning.Application.ProgramIncrements.Dtos;

public sealed record ProgramIncrementListDto
{
    /// <summary>Gets or sets the identifier.</summary>
    /// <value>The identifier.</value>
    public Guid Id { get; set; }

    /// <summary>Gets the key.</summary>
    /// <value>The key.</value>
    public int Key { get; set; }

    /// <summary>
    /// The name of the program increment.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>Program Increment start date</summary>
    /// <value>The start.</value>
    public LocalDate Start { get; set; }

    /// <summary>Program Increment end date</summary>
    /// <value>The end.</value>
    public LocalDate End { get; set; }

    /// <summary>Gets or sets the state.</summary>
    /// <value>The state.</value>
    public required string State { get; set; }

    // TODO: do this with Mapster
    public static ProgramIncrementListDto Create(ProgramIncrement programIncrement, IDateTimeService dateTimeService)
    {
        return new ProgramIncrementListDto()
        {
            Id = programIncrement.Id,
            Key = programIncrement.Key,
            Name = programIncrement.Name,
            Start = programIncrement.DateRange.Start,
            End = programIncrement.DateRange.End,
            State = programIncrement.StateOn(dateTimeService.Now.InUtc().Date).GetDisplayName()
        };
    }
}
