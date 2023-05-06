namespace Moda.Planning.Application.ProgramIncrements.Dtos;
public sealed record ProgramIncrementListDto : IMapFrom<ProgramIncrement>
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

    /// <summary>Gets or sets the start.</summary>
    /// <value>The start.</value>
    public LocalDate Start { get; set; }

    /// <summary>Gets or sets the end.</summary>
    /// <value>The end.</value>
    public LocalDate End { get; set; }

    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ProgramIncrement, ProgramIncrementListDto>()
            .Map(dest => dest.Start, src => src.DateRange.Start)
            .Map(dest => dest.End, src => src.DateRange.End);
    }
}
