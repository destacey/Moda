using Moda.Common.Domain.Data;
using Moda.Common.Domain.Employees;
using Moda.Common.Extensions;
using NodaTime;

namespace Moda.Common.Domain.Models.KeyPerformanceIndicators;

/// <summary>
/// Represents the common properties and behavior for a KPI measurement (check-in).  This class is intended to be immutable.
/// </summary>
public abstract class KpiMeasurement : BaseEntity<Guid>
{
    protected string? _notes;

    protected KpiMeasurement() { }

    protected KpiMeasurement(Guid kpiId, double actualValue, Instant measurementDate, Guid measuredById, string? note)
    {
        KpiId = kpiId;
        ActualValue = actualValue;
        MeasurementDate = measurementDate;
        MeasuredById = measuredById;
        Note = note;
    }

    /// <summary>
    /// The unique identifier of the parent KPI.
    /// </summary>
    public Guid KpiId { get; protected init; }

    /// <summary>
    /// The actual measured value of the KPI.
    /// </summary>
    public double ActualValue { get; protected set; }

    /// <summary>
    /// The date and time when the measurement was recorded.
    /// </summary>
    public Instant MeasurementDate { get; protected set; }

    /// <summary>
    /// EmployeeId of the employee who measured the KPI.
    /// </summary>
    public Guid MeasuredById { get; protected set; }

    /// <summary>
    /// The employee who measured the KPI.
    /// </summary>
    public Employee MeasuredBy { get; protected set; } = default!;

    /// <summary>
    /// Additional note or context regarding the measurement.
    /// </summary>
    public string? Note
    { 
        get => _notes; 
        protected set => _notes = value.NullIfWhiteSpacePlusTrim(); 
    }
}
