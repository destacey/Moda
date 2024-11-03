using Moda.Planning.Domain.Interfaces.Roadmaps;

namespace Moda.Web.Api.Models.Planning.Roadmaps;

internal record UpsertRoadmapTimeboxAdapter : IUpsertRoadmapTimebox
{
    public UpsertRoadmapTimeboxAdapter(CreateRoadmapTimeboxRequest request)
    {
        Name = request.Name;
        Description = request.Description;
        ParentId = request.ParentId;
        Color = request.Color;
        DateRange = new LocalDateRange(request.Start, request.End);
    }

    public UpsertRoadmapTimeboxAdapter(UpdateRoadmapTimeboxRequest request)
    {
        Name = request.Name;
        Description = request.Description;
        ParentId = request.ParentId;
        Color = request.Color;
        DateRange = new LocalDateRange(request.Start, request.End);
    }

    public string Name { get; }
    public string? Description { get; }
    public Guid? ParentId { get; }
    public string? Color { get; }
    public LocalDateRange DateRange { get; }
}
