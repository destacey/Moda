using Moda.Tests.Shared.Data;
using Moda.Work.Domain.Models;

namespace Moda.Work.Domain.Tests.Data;
public class WorkTypeFaker : PrivateConstructorFaker<WorkType>
{
    public WorkTypeFaker()
    {
        var workTypeLevelFaker = new WorkTypeLevelFaker().GetRandomFromNormalHierarchy();

        RuleFor(x => x.Id, f => f.Random.Number(1, 10000));
        RuleFor(x => x.Name, f => f.Random.String2(10));
        RuleFor(x => x.Description, f => f.Random.String2(10));
        RuleFor(x => x.LevelId, workTypeLevelFaker.Id);
        RuleFor(x => x.Level, workTypeLevelFaker);
        RuleFor(x => x.IsActive, true);
    }
}

public static class WorkTypeFakerExtensions
{
    public static WorkTypeFaker WithData(this WorkTypeFaker faker, string? name = null, string? description = null, WorkTypeLevel? level = null, bool? isActive = null)
    {
        if (!string.IsNullOrWhiteSpace(name)) { faker.RuleFor(x => x.Name, name); }
        if (!string.IsNullOrWhiteSpace(description)) { faker.RuleFor(x => x.Description, description); }
        if (level != null) 
        { 
            faker.RuleFor(x => x.LevelId, level.Id);
            faker.RuleFor(x => x.Level, level); 
        }
        if (isActive.HasValue) { faker.RuleFor(x => x.IsActive, isActive.Value); }

        return faker;
    }

    public static WorkTypeFaker AsEpic(this WorkTypeFaker faker)
    {
        var level = new WorkTypeLevelFaker().AsEpics().Generate();
        faker.RuleFor(x => x.Name, "Epic");
        faker.RuleFor(x => x.Description, "The Epic type");
        faker.RuleFor(x => x.LevelId, level.Id);
        faker.RuleFor(x => x.Level, level);

        return faker;
    }

    public static WorkTypeFaker AsFeature(this WorkTypeFaker faker)
    {
        var level = new WorkTypeLevelFaker().AsFeatures().Generate();
        faker.RuleFor(x => x.Name, "Feature");
        faker.RuleFor(x => x.Description, "The Feature type");
        faker.RuleFor(x => x.LevelId, level.Id);
        faker.RuleFor(x => x.Level, level);

        return faker;
    }

    public static WorkTypeFaker AsStory(this WorkTypeFaker faker)
    {
        var level = new WorkTypeLevelFaker().AsStories().Generate();
        faker.RuleFor(x => x.Name, "Story");
        faker.RuleFor(x => x.Description, "The Story type");
        faker.RuleFor(x => x.LevelId, level.Id);
        faker.RuleFor(x => x.Level, level);

        return faker;
    }

    public static WorkTypeFaker AsBug(this WorkTypeFaker faker)
    {
        var level = new WorkTypeLevelFaker().AsStories().Generate();
        faker.RuleFor(x => x.Name, "Bug");
        faker.RuleFor(x => x.Description, "The Bug type");
        faker.RuleFor(x => x.LevelId, level.Id);
        faker.RuleFor(x => x.Level, level);

        return faker;
    }

    public static WorkTypeFaker AsOther(this WorkTypeFaker faker)
    {
        var level = new WorkTypeLevelFaker().AsOther().Generate();
        faker.RuleFor(x => x.Name, "Other");
        faker.RuleFor(x => x.Description, "The other type");
        faker.RuleFor(x => x.LevelId, level.Id);
        faker.RuleFor(x => x.Level, level);

        return faker;
    }

    public static WorkType[] GetNormalWorkTypes(this WorkTypeFaker faker)
    {
        return
        [
            AsEpic(faker).Generate(),
            AsFeature(faker).Generate(),
            AsStory(faker).Generate(),
            AsBug(faker).Generate(),
            AsOther(faker).Generate()
        ];
    }
}
