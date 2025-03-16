using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Domain.Models.KeyPerformanceIndicators;

namespace Moda.ProjectPortfolioManagement.Domain.Models;

public class StrategicInitiativeKpi : Kpi, ISystemAuditable
{
    private readonly HashSet<StrategicInitiativeKpiCheckpoint> _checkpoints = [];
    private readonly HashSet<StrategicInitiativeKpiMeasurement> _measurements = [];

    private StrategicInitiativeKpi() : base() { }

    private StrategicInitiativeKpi(string name, string? description, double targetValue, KpiUnit unit, KpiTargetDirection direction, Guid strategicInitiativeId)
        : base(name, description, targetValue, unit, direction)
    {
        StrategicInitiativeId = strategicInitiativeId;
    }

    /// <summary>
    /// The unique identifier of the associated Strategic Initiative.
    /// </summary>
    public Guid StrategicInitiativeId { get; protected set; }

    /// <summary>
    /// The collection of KPI checkpoints.
    /// </summary>
    public IReadOnlyCollection<StrategicInitiativeKpiCheckpoint> Checkpoints => _checkpoints;

    /// <summary>
    /// The collection of KPI measurements.
    /// </summary>
    public IReadOnlyCollection<StrategicInitiativeKpiMeasurement> Measurements => _measurements;

    /// <summary>
    /// Computes the current value of the KPI based on the latest measurement entry.
    /// </summary>
    public double? CurrentValue => _measurements
        .OrderByDescending(m => m.MeasurementDate)
        .FirstOrDefault()?.ActualValue;

    /// <summary>
    /// Adds a new checkpoint entry to the KPI.
    /// </summary>
    /// <param name="checkpoint">The KPI checkpoint entry to add.</param>
    public virtual Result AddCheckpoint(StrategicInitiativeKpiCheckpoint checkpoint)
    {
        Guard.Against.Null(checkpoint, nameof(checkpoint));

        if (checkpoint.KpiId != Id)
            return Result.Failure("Checkpoint does not belong to this KPI.");

        _checkpoints.Add(checkpoint);

        return Result.Success();
    }

    /// <summary>
    /// Removes a checkpoint entry from the KPI.
    /// </summary>
    /// <param name="checkpointId"></param>
    /// <returns></returns>
    public virtual Result RemoveCheckpoint(Guid checkpointId)
    {
        Guard.Against.NullOrEmpty(checkpointId, nameof(checkpointId));

        var checkpoint = _checkpoints.FirstOrDefault(m => m.Id == checkpointId);
        if (checkpoint == null)
            return Result.Failure("Checkpoint not found in the KPI.");

        _checkpoints.Remove(checkpoint);

        return Result.Success();
    }

    /// <summary>
    /// Adds a new measurement entry to the KPI.
    /// </summary>
    /// <param name="measurement">The KPI measurement entry to add.</param>
    public virtual Result AddMeasurement(StrategicInitiativeKpiMeasurement measurement)
    {
        Guard.Against.Null(measurement, nameof(measurement));

        if (measurement.KpiId != Id)
            return Result.Failure("Measurement does not belong to this KPI.");

        _measurements.Add(measurement);

        return Result.Success();
    }

    /// <summary>
    /// Removes a measurement entry from the KPI.
    /// </summary>
    /// <param name="measurementId"></param>
    /// <returns></returns>
    public virtual Result RemoveMeasurement(Guid measurementId)
    {
        Guard.Against.NullOrEmpty(measurementId, nameof(measurementId));

        var measurement = _measurements.FirstOrDefault(m => m.Id == measurementId);
        if (measurement == null)
            return Result.Failure("Measurement not found in the KPI.");

        _measurements.Remove(measurement);

        return Result.Success();
    }

    /// <summary>
    /// Factory method to create a new instance of <see cref="StrategicInitiativeKpi"/>.
    /// </summary>
    /// <param name="name">The name of the KPI.</param>
    /// <param name="description">A description of what the KPI measures.</param>
    /// <param name="targetValue">The target value that defines success for the KPI.</param>
    /// <param name="unit">The unit of measurement for the KPI.</param>
    /// <param name="direction">The target direction for the KPI.</param>
    /// <param name="strategicInitiativeId">The unique identifier of the associated Strategic Initiative.</param>
    /// <returns></returns>
    public static StrategicInitiativeKpi Create(string name, string? description, double targetValue, KpiUnit unit, KpiTargetDirection direction, Guid strategicInitiativeId)
    {
        return new StrategicInitiativeKpi(name, description, targetValue, unit, direction, strategicInitiativeId);
    }
}
