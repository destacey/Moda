using Moda.Common.Domain.Enums;
using Moda.Common.Domain.Enums.Work;
using Moda.Tests.Shared.Data;
using Moda.Work.Domain.Models;

namespace Moda.Work.Domain.Tests.Data;
public class WorkTypeLevelFaker : PrivateConstructorFaker<WorkTypeLevel>
{
    public WorkTypeLevelFaker()
    {
        RuleFor(x => x.Id, f => f.Random.Int(1, 10000));
        RuleFor(x => x.Name, f => f.Random.String2(10));
        RuleFor(x => x.Description, f => f.Random.String2(10));
        RuleFor(x => x.Tier, f => f.PickRandom<WorkTypeTier>());
        RuleFor(x => x.Ownership, Ownership.System);
        RuleFor(x => x.Order, f => f.Random.Int(1, 100));
    }
}

public static class WorkTypeLevelFakerExtensions
{
    public static WorkTypeLevel WithData(this WorkTypeLevelFaker faker, string? name = null, string? description = null, WorkTypeTier? tier = null, int? order = null)
    {
        if (!string.IsNullOrWhiteSpace(name)) { faker.RuleFor(x => x.Name, name); }
        if (!string.IsNullOrWhiteSpace(description)) { faker.RuleFor(x => x.Description, description); }
        if (tier.HasValue) { faker.RuleFor(x => x.Tier, tier.Value); }
        if (order.HasValue) { faker.RuleFor(x => x.Order, order.Value); }

        return faker;
    }

    public static WorkTypeLevelFaker AsEpics(this WorkTypeLevelFaker faker)
    {
        faker.RuleFor(x => x.Tier, WorkTypeTier.Portfolio);
        faker.RuleFor(x => x.Name, "Epics");
        faker.RuleFor(x => x.Description, "The Epics level");
        faker.RuleFor(x => x.Order, 1);

        return faker;
    }

    public static WorkTypeLevelFaker AsFeatures(this WorkTypeLevelFaker faker)
    {
        faker.RuleFor(x => x.Tier, WorkTypeTier.Portfolio);
        faker.RuleFor(x => x.Name, "Features");
        faker.RuleFor(x => x.Description, "The Features level");
        faker.RuleFor(x => x.Order, 2);

        return faker;
    }

    public static WorkTypeLevelFaker AsStories(this WorkTypeLevelFaker faker)
    {
        faker.RuleFor(x => x.Tier, WorkTypeTier.Requirement);
        faker.RuleFor(x => x.Name, "Stories");
        faker.RuleFor(x => x.Description, "The Stories level");
        faker.RuleFor(x => x.Order, 1);

        return faker;
    }

    public static WorkTypeLevelFaker AsOther(this WorkTypeLevelFaker faker)
    {
        faker.RuleFor(x => x.Tier, WorkTypeTier.Other);
        faker.RuleFor(x => x.Name, "Other");
        faker.RuleFor(x => x.Description, "The Other level");
        faker.RuleFor(x => x.Order, 1);

        return faker;
    }

    public static WorkTypeLevel[] GetNormalHierarchy(this WorkTypeLevelFaker faker)
    {
        return
        [
            AsEpics(faker).Generate(),
            AsFeatures(faker).Generate(),
            AsStories(faker).Generate(),
            AsOther(faker).Generate()
        ];
    }

    public static WorkTypeLevel GetRandomFromNormalHierarchy(this WorkTypeLevelFaker faker)
    {
        return new Random().Next(1, 4) switch
        {
            1 => AsEpics(faker).Generate(),
            2 => AsFeatures(faker).Generate(),
            3 => AsStories(faker).Generate(),
            4 => AsOther(faker).Generate(),
            _ => throw new Exception("Invalid random number generated."),
        };
    }
}
