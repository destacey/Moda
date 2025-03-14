using Moda.Common.Domain.Data;

namespace Moda.Common.Domain.Models;

/// <summary>
/// Represents the common properties and behavior for a KPI measurement (check-in).
/// </summary>
public abstract class KpiMeasurement : BaseEntity<Guid>
{
    protected KpiMeasurement(double actualValue, DateTime measurementDate, string? notes = null)
    {
        ActualValue = actualValue;
        MeasurementDate = measurementDate;
        Notes = notes;
    }

    /// <summary>
    /// The actual measured value of the KPI.
    /// </summary>
    public double ActualValue { get; private set; }

    /// <summary>
    /// The date and time when the measurement was recorded.
    /// </summary>
    public DateTime MeasurementDate { get; private set; }

    /// <summary>
    /// Additional notes or context regarding the measurement.
    /// </summary>
    public string? Notes { get; private set; }
}
