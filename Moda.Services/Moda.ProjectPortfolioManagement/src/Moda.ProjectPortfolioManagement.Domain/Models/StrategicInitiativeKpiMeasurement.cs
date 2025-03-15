using CSharpFunctionalExtensions;
using Moda.Common.Domain.Models.KeyPerformanceIndicators;
using NodaTime;

namespace Moda.ProjectPortfolioManagement.Domain.Models;

public class StrategicInitiativeKpiMeasurement : KpiMeasurement, ISystemAuditable
{
    private StrategicInitiativeKpiMeasurement() : base() { }

    private StrategicInitiativeKpiMeasurement(Guid kpiId, double actualValue, Instant measurementDate, Guid measuredById, string? note)
        : base(kpiId, actualValue, measurementDate, measuredById, note)
    {
    }

    /// <summary>
    /// Factory method to create a new instance of <see cref="StrategicInitiativeKpiMeasurement"/>.
    /// </summary>
    /// <param name="kpiId">The unique identifier of the parent KPI.</param>
    /// <param name="actualValue">The actual measured value for the KPI at this check-in.</param>
    /// <param name="measurementDate">The date and time (in UTC) when the measurement was taken.</param>
    /// <param name="measuredById">The unique identifier of the employee who measured the KPI.</param>
    /// <param name="note">Optional note providing context for the measurement.</param>
    /// <param name="currentTimestamp">The current timestamp.</param>
    /// <returns></returns>
    public static Result<StrategicInitiativeKpiMeasurement> Create(Guid kpiId, double actualValue, Instant measurementDate, Guid measuredById, string? note, Instant currentTimestamp)
    {
        if (measurementDate > currentTimestamp)
        {
            return Result.Failure<StrategicInitiativeKpiMeasurement>("Measurement date cannot be in the future.");
        }

        return new StrategicInitiativeKpiMeasurement(kpiId, actualValue, measurementDate, measuredById, note);
    }
}
