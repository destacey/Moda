using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Domain.Data;
using Moda.Common.Extensions;

namespace Moda.Common.Domain.Models.KeyPerformanceIndicators;

/// <summary>
/// Represents the common properties and behavior for a Key Performance Indicator (KPI).
/// </summary>
public abstract class Kpi : BaseEntity<Guid>
{
    protected string _name = default!;
    protected string? _description;

    // TODO: commented out the relationship parts because they weren't playing well with EF Core (TPC)
    //protected readonly HashSet<KpiCheckpoint> _checkpoints = [];
    //protected readonly HashSet<KpiMeasurement> _measurements = [];

    protected Kpi() { }

    protected Kpi(string name, string? description, double targetValue, KpiUnit unit, KpiTargetDirection targetDirection)
    {
        Name = name;
        Description = description;
        TargetValue = targetValue;
        Unit = unit;
        TargetDirection = targetDirection;
    }

    /// <summary>
    /// The unique key of the KPI.  This is an alternate key to the Id.
    /// </summary>
    public int Key { get; private init; }

    /// <summary>
    /// The name of the KPI.
    /// </summary>
    public string Name
    {
        get => _name;
        protected set => _name = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    }

    /// <summary>
    /// The description detailing what the KPI measures.
    /// </summary>
    public string? Description
    {
        get => _description;
        protected set => _description = value.NullIfWhiteSpacePlusTrim();
    }

    /// <summary>
    /// The target value that defines success for the KPI.
    /// </summary>
    public double TargetValue { get; protected set; }

    /// <summary>
    /// Gets the unit of measurement for the KPI.
    /// </summary>
    public KpiUnit Unit { get; protected set; }

    /// <summary>
    /// Gets the target direction for the KPI. This indicates whether the KPI is expected to increase or decrease.
    /// </summary>
    public KpiTargetDirection TargetDirection { get; protected set; }

    /// <summary>
    /// The collection of KPI checkpoints.
    /// </summary>
    //public IReadOnlyCollection<KpiCheckpoint> Checkpoints => _checkpoints;

    /// <summary>
    /// The collection of KPI measurements.
    /// </summary>
    //public IReadOnlyCollection<KpiMeasurement> Measurements => _measurements;

    /// <summary>
    /// Computes the current value of the KPI based on the latest measurement entry.
    /// </summary>
    //public double? CurrentValue => _measurements
    //    .OrderByDescending(m => m.MeasurementDate)
    //    .FirstOrDefault()?.ActualValue;

    /// <summary>
    /// Updates the KPI properties.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="targetValue"></param>
    /// <param name="unit"></param>
    /// <param name="targetDirection"></param>
    /// <returns></returns>
    public virtual Result Update(string name, string? description, double targetValue, KpiUnit unit, KpiTargetDirection targetDirection)
    {
        Name = name;
        Description = description;
        TargetValue = targetValue;
        Unit = unit;
        TargetDirection = targetDirection;

        return Result.Success();
    }

    ///// <summary>
    ///// Adds a new checkpoint entry to the KPI.
    ///// </summary>
    ///// <param name="checkpoint">The KPI checkpoint entry to add.</param>
    //public virtual Result AddCheckpoint(KpiCheckpoint checkpoint)
    //{
    //    Guard.Against.Null(checkpoint, nameof(checkpoint));

    //    if (checkpoint.KpiId != Id)
    //        return Result.Failure("Checkpoint does not belong to this KPI.");

    //    _checkpoints.Add(checkpoint);

    //    return Result.Success();
    //}

    ///// <summary>
    ///// Removes a checkpoint entry from the KPI.
    ///// </summary>
    ///// <param name="checkpointId"></param>
    ///// <returns></returns>
    //public virtual Result RemoveCheckpoint(Guid checkpointId)
    //{
    //    Guard.Against.NullOrEmpty(checkpointId, nameof(checkpointId));

    //    var checkpoint = _checkpoints.FirstOrDefault(m => m.Id == checkpointId);
    //    if (checkpoint == null)
    //        return Result.Failure("Checkpoint not found in the KPI.");

    //    _checkpoints.Remove(checkpoint);

    //    return Result.Success();
    //}

    ///// <summary>
    ///// Adds a new measurement entry to the KPI.
    ///// </summary>
    ///// <param name="measurement">The KPI measurement entry to add.</param>
    //public virtual Result AddMeasurement(KpiMeasurement measurement)
    //{
    //    Guard.Against.Null(measurement, nameof(measurement));

    //    if (measurement.KpiId != Id)
    //        return Result.Failure("Measurement does not belong to this KPI.");

    //    _measurements.Add(measurement);

    //    return Result.Success();
    //}

    ///// <summary>
    ///// Removes a measurement entry from the KPI.
    ///// </summary>
    ///// <param name="measurementId"></param>
    ///// <returns></returns>
    //public virtual Result RemoveMeasurement(Guid measurementId)
    //{
    //    Guard.Against.NullOrEmpty(measurementId, nameof(measurementId));

    //    var measurement = _measurements.FirstOrDefault(m => m.Id == measurementId);
    //    if (measurement == null)
    //        return Result.Failure("Measurement not found in the KPI.");

    //    _measurements.Remove(measurement);

    //    return Result.Success();
    //}
}
