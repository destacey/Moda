using Moda.Common.Domain.Models.KeyPerformanceIndicators;
using Moda.Common.Domain.Tests.Data.Models;
using Moda.Tests.Shared.Data;

namespace Moda.Common.Domain.Tests.Data;

public sealed class TestKpiFaker : PrivateConstructorFaker<TestKpi>
{
    public TestKpiFaker()
    {
        RuleFor(x => x.Id, f => f.Random.Guid());
        RuleFor(x => x.Name, f => f.Lorem.Sentence());
        RuleFor(x => x.Description, f => f.Lorem.Sentence());
        RuleFor(x => x.TargetValue, f => f.Random.Double(0, 100));
        RuleFor(x => x.ActualValue, f => f.Random.Double(0, 100));
        RuleFor(x => x.Unit, f => f.PickRandom<KpiUnit>());
        RuleFor(x => x.TargetDirection, f => f.PickRandom<KpiTargetDirection>());
    }
}

public static class TestKpiFakerExtensions
{
    public static TestKpiFaker WithData(
        this TestKpiFaker faker,
        Guid? id = null,
        string? name = null,
        string? description = null,
        double? targetValue = null,
        double? actualValue = null,
        KpiUnit? unit = null,
        KpiTargetDirection? kpiTargetDirection = null)
    {
        if (id.HasValue) { faker.RuleFor(x => x.Id, id.Value); }
        if (!string.IsNullOrWhiteSpace(name)) { faker.RuleFor(x => x.Name, name); }
        if (!string.IsNullOrWhiteSpace(description)) { faker.RuleFor(x => x.Description, description); }
        if (targetValue.HasValue) { faker.RuleFor(x => x.TargetValue, targetValue.Value); }
        if (actualValue.HasValue) { faker.RuleFor(x => x.ActualValue, actualValue.Value); }
        if (unit.HasValue) { faker.RuleFor(x => x.Unit, unit.Value); }
        if (kpiTargetDirection.HasValue) { faker.RuleFor(x => x.TargetDirection, kpiTargetDirection.Value); }

        return faker;
    }
}
