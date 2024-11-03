using Moda.Planning.Domain.Interfaces.Roadmaps;

namespace Moda.Web.Api.Models.Planning.Roadmaps;

internal record UpsertRoadmapMilestoneAdapter : IUpsertRoadmapMilestone
{
    public UpsertRoadmapMilestoneAdapter(CreateRoadmapMilestoneRequest request)
    {
        Name = request.Name;
        Description = request.Description;
        ParentId = request.ParentId;
        Color = request.Color;
        Date = request.Date;
    }

    public UpsertRoadmapMilestoneAdapter(UpdateRoadmapMilestoneRequest request)
    {
        Name = request.Name;
        Description = request.Description;
        ParentId = request.ParentId;
        Color = request.Color;
        Date = request.Date;
    }

    public string Name { get; }
    public string? Description { get; }
    public Guid? ParentId { get; }
    public string? Color { get; }
    public LocalDate Date { get; }
}
