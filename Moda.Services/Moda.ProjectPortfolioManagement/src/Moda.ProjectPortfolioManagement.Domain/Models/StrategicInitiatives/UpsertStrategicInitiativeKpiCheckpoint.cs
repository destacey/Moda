using NodaTime;

namespace Moda.ProjectPortfolioManagement.Domain.Models.StrategicInitiatives;

public sealed record UpsertStrategicInitiativeKpiCheckpoint
{
    public Guid? Id { get; set; }
    public double TargetValue { get; set; }
    public Instant CheckpointDate { get; set; }
    public required string DateLabel { get; set; }
    public double? AtRiskValue { get; set; }

    public bool IsNew => Id.IsNullEmptyOrDefault();

    public static UpsertStrategicInitiativeKpiCheckpoint Create(Guid? id, double targetValue, Instant checkpointDate, string dateLabel, double? atRiskValue = null)
    {
        return new UpsertStrategicInitiativeKpiCheckpoint
        {
            Id = id,
            TargetValue = targetValue,
            CheckpointDate = checkpointDate,
            DateLabel = dateLabel,
            AtRiskValue = atRiskValue
        };
    }
}
