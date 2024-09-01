namespace Moda.Planning.Application.Roadmaps.Dtos;
public sealed record RoadmapListDto : IMapFrom<Roadmap>
{
    /// <summary>Gets or sets the identifier.</summary>
    /// <value>The identifier.</value>
    public Guid Id { get; set; }

    /// <summary>Gets the key.</summary>
    /// <value>The key.</value>
    public int Key { get; set; }

    /// <summary>
    /// The name of the Roadmap.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The Roadmap start date.
    /// </summary>
    public required LocalDate Start { get; set; }

    /// <summary>
    /// The Roadmap end date.
    /// </summary>
    public required LocalDate End { get; set; }

    /// <summary>
    /// Indicates if the Roadmap is public.  If true, the Roadmap is visible to all users. If false, the Roadmap is only visible to the managers.
    /// </summary>
    public bool IsPublic { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<Roadmap, RoadmapListDto>()
            .Map(dest => dest.Start, src => src.DateRange.Start)
            .Map(dest => dest.End, src => src.DateRange.End);
    }
}
