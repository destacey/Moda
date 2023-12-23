using System.Reflection;
using Moda.Common.Models;
using Moda.Planning.Domain.Models;
using Moda.Tests.Shared.Data;
using NodaTime.Extensions;

namespace Moda.Planning.Domain.Tests.Data;
public class PlanningIntervalFaker : Faker<PlanningInterval>
{
    public PlanningIntervalFaker() : base("en", IncludePrivateFieldBinder.Create())
    {
        PlanningIntervalId = Guid.NewGuid();
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
    public static PlanningIntervalFaker WithObjectives(this PlanningIntervalFaker faker, PlanningTeam team, int objectivesCount = 0)
    {
        faker.RuleFor("_objectives", f => new PlanningIntervalObjectiveFaker(faker.PlanningIntervalId, team, Enums.ObjectiveStatus.NotStarted, false).UsePrivateConstructor().Generate(objectivesCount));
        return faker;
    }
}
