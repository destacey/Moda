using Moda.Planning.Domain.Models.PlanningPoker;

namespace Moda.Planning.Application.EstimationScales.Dtos;

public class EstimationScaleValueDto : IMapFrom<EstimationScaleValue>
{
    public string Value { get; set; } = default!;
    public int Order { get; set; }
}
