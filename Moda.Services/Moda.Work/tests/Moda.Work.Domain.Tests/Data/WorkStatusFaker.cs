using Moda.Tests.Shared.Data;
using Moda.Work.Domain.Models;

namespace Moda.Work.Domain.Tests.Data;
public class WorkStatusFaker : PrivateConstructorFaker<WorkStatus>
{
    public WorkStatusFaker()
    {
        RuleFor(x => x.Id, f => f.Random.Number(1, 10000));
        RuleFor(x => x.Name, f => f.Random.String2(10));
        RuleFor(x => x.Description, f => f.Random.String2(10));
        RuleFor(x => x.IsActive, true);
    }
}

public static class WorkStatusFakerExtensions
{
    public static WorkStatusFaker WithData(this WorkStatusFaker faker, string? name = null, string? description = null, bool? isActive = null)
    {
        if (!string.IsNullOrWhiteSpace(name)) { faker.RuleFor(x => x.Name, name); }
        if (!string.IsNullOrWhiteSpace(description)) { faker.RuleFor(x => x.Description, description); }
        if (isActive.HasValue) { faker.RuleFor(x => x.IsActive, isActive.Value); }

        return faker;
    }
}
