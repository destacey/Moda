using Moda.Planning.Domain.Models.PlanningPoker;

namespace Moda.Planning.Application.EstimationScales.Dtos;

public class EstimationScaleDetailsDto : IMapFrom<EstimationScale>
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsPreset { get; set; }
    public List<EstimationScaleValueDto> Values { get; set; } = [];
}
