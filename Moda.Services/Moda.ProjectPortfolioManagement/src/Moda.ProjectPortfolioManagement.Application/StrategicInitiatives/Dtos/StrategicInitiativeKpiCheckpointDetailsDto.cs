using Moda.Common.Domain.Enums;
using Moda.Common.Domain.Models.KeyPerformanceIndicators;
using Moda.ProjectPortfolioManagement.Domain.Models.StrategicInitiatives;

namespace Moda.ProjectPortfolioManagement.Application.StrategicInitiatives.Dtos;

public sealed record StrategicInitiativeKpiCheckpointDetailsDto : IMapFrom<StrategicInitiativeKpiCheckpoint>
{
    public Guid Id { get; set; }
    public double TargetValue { get; set; }
    public double? AtRiskValue { get; set; }
    public Instant CheckpointDate { get; set; }
    public required string DateLabel { get; set; }
    public StrategicInitiativeKpiMeasurementDto? Measurement { get; set; }
    public KpiHealth? Health { get; set; }
    public KpiTrend? Trend { get; set; }

    public void Enrich(StrategicInitiativeKpiMeasurementDto measurement, StrategicInitiativeKpiMeasurementDto? previousCheckpointMeasurement, KpiTargetDirection kpiTargetDirection)
    {
        Measurement = measurement;
        Health = KpiUtils.GetKpiHealth(measurement.ActualValue, TargetValue, AtRiskValue, kpiTargetDirection);
        Trend = KpiUtils.GetKpiTrend(previousCheckpointMeasurement?.ActualValue, measurement.ActualValue, kpiTargetDirection);
    }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<StrategicInitiativeKpiCheckpoint, StrategicInitiativeKpiCheckpointDetailsDto>();
    }
}
