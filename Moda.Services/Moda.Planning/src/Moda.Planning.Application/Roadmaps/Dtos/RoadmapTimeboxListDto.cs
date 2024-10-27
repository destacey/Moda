using Moda.Planning.Domain.Models.Roadmaps;

namespace Moda.Planning.Application.Roadmaps.Dtos;
public sealed record RoadmapTimeboxListDto : RoadmapItemListDto, IMapFrom<RoadmapTimebox>
{
    /// <summary>
    /// The Roadmap Timebox start date.
    /// </summary>
    public required LocalDate Start { get; set; }

    /// <summary>
    /// The Roadmap Timebox end date.
    /// </summary>
    public required LocalDate End { get; set; }

    public override void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<RoadmapTimebox, RoadmapTimeboxListDto>()
            .Inherits<BaseRoadmapItem, RoadmapItemListDto>()
            .Map(dest => dest.Start, src => src.DateRange.Start)
            .Map(dest => dest.End, src => src.DateRange.End);
    }
}
