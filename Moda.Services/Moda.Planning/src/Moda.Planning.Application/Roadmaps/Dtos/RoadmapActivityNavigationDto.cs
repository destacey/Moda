using Moda.Planning.Domain.Models.Roadmaps;

namespace Moda.Planning.Application.Roadmaps.Dtos;
public record RoadmapActivityNavigationDto : IMapFrom<RoadmapActivity>
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
}
