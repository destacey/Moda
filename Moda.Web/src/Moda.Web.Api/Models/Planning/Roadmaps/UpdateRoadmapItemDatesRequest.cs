using System.Text.Json.Serialization;

namespace Moda.Web.Api.Models.Planning.Roadmaps;

[JsonDerivedType(typeof(UpdateRoadmapActivityDatesRequest), typeDiscriminator: "activity")]
[JsonDerivedType(typeof(UpdateRoadmapMilestoneDatesRequest), typeDiscriminator: "milestone")]
[JsonDerivedType(typeof(UpdateRoadmapTimeboxDatesRequest), typeDiscriminator: "timebox")]
public abstract record UpdateRoadmapItemDatesRequest
{
    /// <summary>
    /// The Roadmap Id the Roadmap Item belongs to.
    /// </summary>
    public Guid RoadmapId { get; set; }

    /// <summary>
    /// The Roadmap Item Id.
    /// </summary>
    public Guid ItemId { get; set; }
}

public sealed class UpdateRoadmapItemDatesRequestValidator : CustomValidator<UpdateRoadmapItemDatesRequest>
{
    public UpdateRoadmapItemDatesRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.RoadmapId)
            .NotEmpty();

        RuleFor(t => t.ItemId)
            .NotEmpty();
    }
}
