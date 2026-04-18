using Wayd.Planning.Domain.Models.Roadmaps;

namespace Wayd.Planning.Application.Roadmaps.Dtos;

public sealed record RoadmapMilestoneDetailsDto : RoadmapItemDetailsDto, IMapFrom<RoadmapMilestone>
{
    /// <summary>
    /// The Roadmap Milestone date.
    /// </summary>
    public required LocalDate Date { get; set; }

    public override void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<RoadmapMilestone, RoadmapMilestoneDetailsDto>()
            .Inherits<BaseRoadmapItem, RoadmapItemDetailsDto>()
            .Map(dest => dest.Date, src => src.Date);
    }
}
