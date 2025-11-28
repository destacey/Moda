using Moda.Tests.Shared.Data;
using Moda.Work.Domain.Models;

namespace Moda.Work.Domain.Tests.Data;

public class WorkProjectFaker : PrivateConstructorFaker<WorkProject>
{
    public WorkProjectFaker()
    {
        RuleFor(x => x.Id, f => f.Random.Guid());
        RuleFor(x => x.Key, f => f.Random.Int(1, 10000));
        RuleFor(x => x.Name, f => f.Company.CompanyName());
        RuleFor(x => x.Description, f => f.Lorem.Sentence());
    }
}

public static class WorkProjectFakerExtensions
{
    public static WorkProjectFaker WithId(this WorkProjectFaker faker, Guid id)
    {
        faker.RuleFor(x => x.Id, id);
        return faker;
    }

    public static WorkProjectFaker WithKey(this WorkProjectFaker faker, int key)
    {
        faker.RuleFor(x => x.Key, key);
        return faker;
    }

    public static WorkProjectFaker WithName(this WorkProjectFaker faker, string name)
    {
        faker.RuleFor(x => x.Name, name);
        return faker;
    }

    public static WorkProjectFaker WithDescription(this WorkProjectFaker faker, string description)
    {
        faker.RuleFor(x => x.Description, description);
        return faker;
    }
}
