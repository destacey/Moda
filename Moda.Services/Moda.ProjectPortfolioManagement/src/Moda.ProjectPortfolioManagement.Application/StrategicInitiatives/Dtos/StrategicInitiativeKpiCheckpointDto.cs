using Moda.ProjectPortfolioManagement.Domain.Models.StrategicInitiatives;

namespace Moda.ProjectPortfolioManagement.Application.StrategicInitiatives.Dtos;

public sealed record StrategicInitiativeKpiCheckpointDto : IMapFrom<StrategicInitiativeKpiCheckpoint>
{
    public Guid Id { get; set; }
    public double TargetValue { get; set; }
    public Instant CheckpointDate { get; set; }
    public required string DateLabel { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<StrategicInitiativeKpiCheckpoint, StrategicInitiativeKpiCheckpointDto>();
    }
}
