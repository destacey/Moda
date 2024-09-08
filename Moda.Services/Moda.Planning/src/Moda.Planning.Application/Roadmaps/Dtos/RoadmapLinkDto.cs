namespace Moda.Planning.Application.Roadmaps.Dtos;

public sealed record RoadmapLinkDto : IMapFrom<RoadmapLink>
{
    public Guid ParentId { get; set; }
    public required RoadmapListDto Roadmap { get; set; }
    public int Order { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<RoadmapLink, RoadmapLinkDto>()
            .Map(dest => dest.Roadmap, src => src.Child);
    }
}