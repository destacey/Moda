using Moda.Planning.Domain.Models.Roadmaps;

namespace Moda.Planning.Application.Roadmaps.Dtos;
public sealed record RoadmapMilestoneListDto : RoadmapItemListDto, IMapFrom<RoadmapMilestone>
{
    /// <summary>
    /// The Roadmap Milestone date.
    /// </summary>
    public required LocalDate Date { get; set; }

    public override void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<RoadmapMilestone, RoadmapMilestoneListDto>()
            .Inherits<BaseRoadmapItem, RoadmapItemListDto>()
            .Map(dest => dest.Date, src => src.Date);
    }
}
