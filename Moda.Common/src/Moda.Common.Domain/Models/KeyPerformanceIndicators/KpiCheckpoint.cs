using CSharpFunctionalExtensions;
using Moda.Common.Domain.Data;
using NodaTime;

namespace Moda.Common.Domain.Models.KeyPerformanceIndicators;

/// <summary>
/// Represents the common properties and behavior for a KPI checkpoint (expected progress checkpoint).
/// </summary>
public abstract class KpiCheckpoint : BaseEntity<Guid>
{
    protected KpiCheckpoint() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="KpiCheckpoint"/> class.
    /// </summary>
    /// <param name="kpiId">The unique identifier of the parent KPI.</param>
    /// <param name="targetValue">The planned target value for the KPI at this checkpoint.</param>
    /// <param name="checkpointDate">The date and time when this planned target is expected to be achieved.</param>
    protected KpiCheckpoint(Guid kpiId, double targetValue, Instant checkpointDate)
    {
        KpiId = kpiId;
        TargetValue = targetValue;
        CheckpointDate = checkpointDate;
    }

    /// <summary>
    /// The unique identifier of the parent KPI.
    /// </summary>
    public Guid KpiId { get; protected init; }

    /// <summary>
    /// The planned target value that the KPI should reach by the checkpoint date.
    /// </summary>
    public double TargetValue { get; protected set; }

    /// <summary>
    /// The date and time when the planned target is expected to be achieved.
    /// </summary>
    public Instant CheckpointDate { get; protected set; }

    /// <summary>
    /// Updates the target value and checkpoint date for this KPI checkpoint.
    /// </summary>
    /// <param name="targetValue"></param>
    /// <param name="checkpointDate"></param>
    /// <returns></returns>
    public virtual Result Update(double targetValue, Instant checkpointDate)
    {
        TargetValue = targetValue;
        CheckpointDate = checkpointDate;

        return Result.Success();
    }
}
