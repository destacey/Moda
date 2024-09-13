namespace Moda.Web.Api.Models.Planning.Roadmaps;

public sealed class UpdateRoadmapChildrenOrderRequest
{
    public Guid RoadmapId { get; set; }
    public Dictionary<Guid, int> ChildrenOrder { get; set; } = [];
}
