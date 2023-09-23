using Moda.Common.Models;
using Moda.Planning.Domain.Models;
using NodaTime.Extensions;

namespace Moda.Planning.Domain.Tests.Data;
public class ProgramIncrementFaker : Faker<ProgramIncrement>
{
    public ProgramIncrementFaker()
    {
        ProgramIncrementId = Guid.NewGuid();
        RuleFor(x => x.Id, ProgramIncrementId);
        RuleFor(x => x.Key, f => f.Random.Int());
        RuleFor(x => x.Name, f => f.Random.String2(10));
        RuleFor(x => x.Description, f => f.Random.String2(10));
        RuleFor(x => x.DateRange, f => new LocalDateRange(f.Date.Past().ToLocalDateTime().Date, f.Date.Future().ToLocalDateTime().Date));
        RuleFor(x => x.ObjectivesLocked, false);
    }

    public Guid ProgramIncrementId { get; init; }
}

public static class ProgramIncrementFakerExtensions
{
    public static ProgramIncrementFaker WithObjectives(this ProgramIncrementFaker faker, PlanningTeam team, int objectivesCount = 0)
    {
        faker.RuleFor("_objectives", f => new ProgramIncrementObjectiveFaker(faker.ProgramIncrementId, team, Enums.ObjectiveStatus.NotStarted, false).UsePrivateConstructor().Generate(objectivesCount));
        return faker;
    }
}
