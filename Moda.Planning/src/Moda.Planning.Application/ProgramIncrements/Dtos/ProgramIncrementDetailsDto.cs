namespace Moda.Planning.Application.ProgramIncrements.Dtos;
public sealed record ProgramIncrementDetailsDto : IMapFrom<ProgramIncrement>
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
}
