using Moda.Common.Models;
using Moda.Planning.Domain.Models;
using Moda.Tests.Shared.Data;
using NodaTime.Extensions;

namespace Moda.Planning.Domain.Tests.Data;
public class PlanningIntervalFaker : PrivateConstructorFaker<PlanningInterval>
{
    public PlanningIntervalFaker()
    {
        PlanningIntervalId = FakerHub.Random.Guid();
        RuleFor(x => x.Id, PlanningIntervalId);
        RuleFor(x => x.Key, f => f.Random.Int());
        RuleFor(x => x.Name, f => f.Random.String2(10));
        RuleFor(x => x.Description, f => f.Random.String2(10));
        RuleFor(x => x.DateRange, f => new LocalDateRange(f.Date.Past().ToLocalDateTime().Date, f.Date.Future().ToLocalDateTime().Date));
        RuleFor(x => x.ObjectivesLocked, false);
    }

    public Guid PlanningIntervalId { get; init; }
}

public static class PlanningIntervalFakerExtensions
{
    public static PlanningIntervalFaker WithData(this PlanningIntervalFaker faker, Guid? planningIntervalId = null, string? name = null, string? description = null, LocalDateRange? dateRange = null, bool? objectivesLocked = null)
    {
        if (planningIntervalId.HasValue) { faker.RuleFor(x => x.Id, planningIntervalId.Value); }
        if (!string.IsNullOrWhiteSpace(name)) { faker.RuleFor(x => x.Name, name); }
        if (!string.IsNullOrWhiteSpace(description)) { faker.RuleFor(x => x.Description, description); }
        if (dateRange is not null) { faker.RuleFor(x => x.DateRange, dateRange); }
        if (objectivesLocked.HasValue) { faker.RuleFor(x => x.ObjectivesLocked, objectivesLocked); }

        return faker;
    }

    public static PlanningIntervalFaker WithIterations(this PlanningIntervalFaker faker, LocalDateRange planningIntervalDates, int iterationWeeks = 2, string? iterationPrefix = "Iteration ")
    {
        var planningInterval = PlanningInterval.Create("Test", null, planningIntervalDates, iterationWeeks, iterationPrefix);

        foreach (var iteration in planningInterval.Value.Iterations.ToList())
        {
            iteration.SetPrivate(m => m.Id, Guid.NewGuid());
            iteration.SetPrivate(m => m.PlanningIntervalId, faker.PlanningIntervalId);
        }

        faker.RuleFor("_iterations", f => planningInterval.Value.Iterations.ToList());
        faker.RuleFor(x => x.DateRange, planningIntervalDates);
        return faker;
    }

    public static PlanningIntervalFaker WithObjectives(this PlanningIntervalFaker faker, PlanningTeam team, int objectivesCount = 0)
    {
        faker.RuleFor("_objectives", f => new PlanningIntervalObjectiveFaker(faker.PlanningIntervalId, team, Enums.ObjectiveStatus.NotStarted, false).Generate(objectivesCount));
        return faker;
    }
}
