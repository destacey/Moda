using Moda.Planning.Domain.Models.PlanningPoker;

namespace Moda.Planning.Application.EstimationScales.Dtos;

public class EstimationScaleDto : IMapFrom<EstimationScale>
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public List<string> Values { get; set; } = [];
}
