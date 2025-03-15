using Moda.Common.Domain.Models.KeyPerformanceIndicators;

namespace Moda.Common.Domain.Tests.Data.Models;

/// <summary>
/// Concrete implementation of Kpi for testing purposes.
/// </summary>
public sealed class TestKpi : Kpi
{
    private TestKpi() : base() { }

    private TestKpi(string name, string description, double targetValue, KpiUnit unit, KpiTargetDirection direction)
        : base(name, description, targetValue, unit, direction)
    {
    }

    public static TestKpi Create(string name, string description, double targetValue, KpiUnit unit, KpiTargetDirection direction)
    {
        return new TestKpi(name, description, targetValue, unit, direction);
    }
}
