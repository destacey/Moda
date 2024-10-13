using System.Text.Json.Serialization;
using Moda.Common.Application.Dtos;
using Moda.Planning.Domain.Models.Roadmaps;

namespace Moda.Planning.Application.Roadmaps.Dtos;

[JsonDerivedType(typeof(RoadmapItemListDto), typeDiscriminator: "RoadmapItemDto")]
[JsonDerivedType(typeof(RoadmapActivityListDto), typeDiscriminator: "RoadmapActivityDto")]
[JsonDerivedType(typeof(RoadmapMilestoneListDto), typeDiscriminator: "RoadmapMilestoneDto")]
[JsonDerivedType(typeof(RoadmapTimeboxListDto), typeDiscriminator: "RoadmapTimeboxDto")]
public record RoadmapItemListDto : IMapFrom<BaseRoadmapItem>
{
    /// <summary>
    /// The roadmap item Id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The roadmap Id.
    /// </summary>
    public Guid RoadmapId { get; set; }

    /// <summary>
    /// The name of the roadmap item.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The type of roadmap item.
    /// </summary>
    public required SimpleNavigationDto Type { get; set; }

    /// <summary>
    /// The parent roadmap activity.
    /// </summary>
    public RoadmapActivityNavigationDto? Parent { get; set; }

    /// <summary>
    /// The color of the roadmap item. Must be a valid hex color code.
    /// </summary>
    public string? Color { get; set; }

    public virtual void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<BaseRoadmapItem, RoadmapItemListDto>()
            .Include<RoadmapActivity, RoadmapActivityListDto>()
            .Include<RoadmapMilestone, RoadmapMilestoneListDto>()
            .Include<RoadmapTimebox, RoadmapTimeboxListDto>()
            .Map(dest => dest.Type, src => SimpleNavigationDto.FromEnum(src.Type));
    }
}
