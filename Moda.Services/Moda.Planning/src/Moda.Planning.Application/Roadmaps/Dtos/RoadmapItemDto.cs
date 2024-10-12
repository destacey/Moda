using System.Text.Json.Serialization;
using Moda.Common.Application.Dtos;
using Moda.Planning.Domain.Models.Roadmaps;

namespace Moda.Planning.Application.Roadmaps.Dtos;

[JsonDerivedType(typeof(RoadmapItemDto), typeDiscriminator: "RoadmapItemDto")]
[JsonDerivedType(typeof(RoadmapActivityDto), typeDiscriminator: "RoadmapActivityDto")]
[JsonDerivedType(typeof(RoadmapMilestoneDto), typeDiscriminator: "RoadmapMilestoneDto")]
[JsonDerivedType(typeof(RoadmapTimeboxDto), typeDiscriminator: "RoadmapTimeboxDto")]
public record RoadmapItemDto : IMapFrom<BaseRoadmapItem>
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
        config.NewConfig<BaseRoadmapItem, RoadmapItemDto>()
            .Map(dest => dest.Type, src => SimpleNavigationDto.FromEnum(src.Type));
    }
}
