using Moda.Planning.Domain.Models.PlanningPoker;

namespace Moda.Planning.Application.EstimationScales.Dtos;

public class EstimationScaleListDto : IMapFrom<EstimationScale>
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public bool IsPreset { get; set; }
    public int ValueCount { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<EstimationScale, EstimationScaleListDto>()
            .Map(dest => dest.ValueCount, src => src.Values.Count);
    }
}
