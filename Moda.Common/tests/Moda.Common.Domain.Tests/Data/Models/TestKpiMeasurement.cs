using Moda.Common.Domain.Models.KeyPerformanceIndicators;

namespace Moda.Common.Domain.Tests.Data.Models;

/// <summary>
/// Concrete implementation of KpiMeasurement for testing purposes.
/// </summary>
public class TestKpiMeasurement : KpiMeasurement
{
    private TestKpiMeasurement(): base() { }

    private TestKpiMeasurement(Guid kpiId, double actualValue, Instant measurementDate, Guid measuredById, string? note)
        : base(kpiId, actualValue, measurementDate, measuredById, note)
    {
    }

    public static TestKpiMeasurement Create(Guid kpiId, double actualValue, Instant measurementDate, Guid measuredById, string? note)
    {
        return new TestKpiMeasurement(kpiId, actualValue, measurementDate, measuredById, note);
    }
}
