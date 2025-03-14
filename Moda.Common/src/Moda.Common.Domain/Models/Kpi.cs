using Ardalis.GuardClauses;
using Moda.Common.Domain.Data;
using Moda.Common.Extensions;

namespace Moda.Common.Domain.Models;

/// <summary>
/// Represents the common properties and behavior for a Key Performance Indicator (KPI).
/// </summary>
public abstract class Kpi : BaseEntity<Guid>
{
    private string _name = default!;
    private string? _description;

    private string _unit = default!;


    private readonly HashSet<KpiMeasurement> _measurements = new();



    /// <summary>
    /// The name of the KPI.
    /// </summary>
    public string Name
    {
        get => _name;
        private set => _name = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    }

    /// <summary>
    /// The description detailing what the KPI measures.
    /// </summary>
    public string? Description
    {
        get => _description;
        private set => _description = value.NullIfWhiteSpacePlusTrim();
    }


    /// <summary>
    /// The target value that defines success for the KPI.
    /// </summary>
    public double TargetValue { get; private set; }

    /// <summary>
    /// The unit of measurement for the KPI (e.g., "%", "units", "dollars").
    /// </summary>
    public string Unit
    {
        get => _unit;
        private set => _unit = Guard.Against.NullOrWhiteSpace(value, nameof(Unit)).Trim();
    }

    /// <summary>
    /// The collection of KPI measurement entries.
    /// </summary>
    public IReadOnlyCollection<KpiMeasurement> Measurements => _measurements;

    /// <summary>
    /// Computes the current value of the KPI based on the latest measurement entry.
    /// </summary>
    public double? CurrentValue => _measurements
        .OrderByDescending(m => m.MeasurementDate)
        .FirstOrDefault()?.ActualValue;

    /// <summary>
    /// Adds a new measurement entry to the KPI.
    /// </summary>
    /// <param name="measurement">The KPI measurement entry to add.</param>
    public void AddMeasurement(KpiMeasurement measurement)
    {
        Guard.Against.Null(measurement, nameof(measurement));
        _measurements.Add(measurement);
    }
}
