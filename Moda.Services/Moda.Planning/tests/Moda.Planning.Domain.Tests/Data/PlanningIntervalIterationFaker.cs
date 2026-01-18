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
        var piId = planningIntervalId ?? FakerHub.Random.Guid();
        RuleFor(x => x.Id, f => f.Random.Guid());
        RuleFor(x => x.Key, f => f.Random.Int());
        RuleFor(x => x.PlanningIntervalId, piId);
        RuleFor(x => x.Name, f => f.Random.String2(10));
        RuleFor(x => x.Category, f => f.PickRandom<IterationCategory>());
        RuleFor(x => x.DateRange, f => new LocalDateRange(f.Date.Past().ToLocalDateTime().Date, f.Date.Future().ToLocalDateTime().Date));
    }
}

public static class PlanningIntervalIterationFakerExtensions
{
    public static PlanningIntervalIterationFaker WithData(this PlanningIntervalIterationFaker faker, Guid? planningIntervalId = null, string? name = null, IterationCategory? iterationCategory = null, LocalDateRange? dateRange = null)
    {
        if (planningIntervalId.HasValue) { faker.RuleFor(x => x.PlanningIntervalId, planningIntervalId.Value); }
        if (!string.IsNullOrWhiteSpace(name)) { faker.RuleFor(x => x.Name, name); }
        if (iterationCategory.HasValue) { faker.RuleFor(x => x.Category, iterationCategory.Value); }
        if (dateRange is not null) { faker.RuleFor(x => x.DateRange, dateRange); }

        return faker;
    }

    public static PlanningIntervalIterationFaker WithSprints(this PlanningIntervalIterationFaker faker, params PlanningIntervalIterationSprint[] sprints)
    {
        faker.RuleFor("_sprints", f => sprints.ToList());
        return faker;
    }
}
