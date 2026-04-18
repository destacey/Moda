using Wayd.Planning.Domain.Models.Roadmaps;

namespace Wayd.Planning.Application.Roadmaps.Dtos;

public record RoadmapActivityNavigationDto : IMapFrom<RoadmapActivity>
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
}
