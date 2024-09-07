namespace Moda.Planning.Application.Roadmaps.Dtos;

public sealed record RoadmapChildDto : IMapFrom<RoadmapLink>
{
    public required RoadmapListDto Roadmap { get; set; }
    public int Order { get; init; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<RoadmapLink, RoadmapChildDto>()
            .Map(dest => dest.Roadmap, src => src.Child);
    }
}
