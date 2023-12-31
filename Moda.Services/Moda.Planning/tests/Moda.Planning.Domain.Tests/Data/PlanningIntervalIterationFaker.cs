using Moda.Common.Models;
using Moda.Planning.Domain.Enums;
using Moda.Planning.Domain.Models;
using Moda.Tests.Shared.Data;
using NodaTime.Extensions;

namespace Moda.Planning.Domain.Tests.Data;
public class PlanningIntervalIterationFaker : PrivateConstructorFaker<PlanningIntervalIteration>
{
    public PlanningIntervalIterationFaker(Guid? planningIntervalId = null)
    {
        var piId = planningIntervalId ?? Guid.NewGuid();
        RuleFor(x => x.Id, f => f.Random.Guid());
        RuleFor(x => x.Key, f => f.Random.Int());
        RuleFor(x => x.PlanningIntervalId, piId);
        RuleFor(x => x.Name, f => f.Random.String2(10));
        RuleFor(x => x.Type, f => f.PickRandom<IterationType>());
        RuleFor(x => x.DateRange, f => new LocalDateRange(f.Date.Past().ToLocalDateTime().Date, f.Date.Future().ToLocalDateTime().Date));
    }
}

public static class PlanningIntervalIterationFakerExtensions
{
    public static PlanningIntervalIterationFaker WithData(this PlanningIntervalIterationFaker faker, Guid? planningIntervalId = null, string? name = null, IterationType? iterationType = null, LocalDateRange? dateRange = null)
    {
        if (planningIntervalId.HasValue) { faker.RuleFor(x => x.PlanningIntervalId, planningIntervalId.Value); }
        if (!string.IsNullOrWhiteSpace(name)) { faker.RuleFor(x => x.Name, name); }
        if (iterationType.HasValue) { faker.RuleFor(x => x.Type, iterationType.Value); }
        if (dateRange is not null) { faker.RuleFor(x => x.DateRange, dateRange); }

        return faker;
    }
}
