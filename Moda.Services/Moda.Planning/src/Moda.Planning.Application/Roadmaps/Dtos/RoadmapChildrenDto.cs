using Moda.Common.Application.Dtos;

namespace Moda.Planning.Application.Roadmaps.Dtos;
public sealed record RoadmapChildrenDto : IMapFrom<Roadmap>
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
    /// The visibility of the Roadmap. If the Roadmap is public, all users can see the Roadmap. Otherwise, only the Roadmap Managers can see the Roadmap.
    /// </summary>
    public required SimpleNavigationDto Visibility { get; set; }

    /// <summary>
    /// The order of the Roadmap within its parent.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// The parent Roadmap.
    /// </summary>
    public required NavigationDto Parent { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<Roadmap, RoadmapChildrenDto>()
            .Map(dest => dest.Start, src => src.DateRange.Start)
            .Map(dest => dest.End, src => src.DateRange.End)
            .Map(dest => dest.Visibility, src => SimpleNavigationDto.FromEnum(src.Visibility))
            .Map(dest => dest.Parent, src => NavigationDto.Create(src.Parent!.Id, src.Parent.Key, src.Parent.Name));
    }
}
