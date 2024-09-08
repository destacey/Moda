namespace Moda.Web.Api.Models.Planning.Roadmaps;

public sealed class UpdateRoadmapLinksOrderRequest
{
    public Guid RoadmapId { get; set; }
    public Dictionary<Guid, int> RoadmapLinks { get; set; } = [];
}
