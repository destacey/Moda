using Moda.Common.Domain.Enums.Organization;
using Moda.Organization.Domain.Models;
using Moda.Tests.Shared.Data;
using NodaTime;

namespace Moda.Organization.Domain.Tests.Data;
public class TeamFaker : PrivateConstructorFaker<Team>
{
    public TeamFaker(LocalDate? activeDate = null)
    {
        activeDate ??= LocalDate.FromDateTime(FakerHub.Date.Recent()).PlusMonths(-6);

        RuleFor(x => x.Id, f => f.Random.Guid());
        RuleFor(x => x.Key, f => f.Random.Int(1000, 10000));
        RuleFor(x => x.Name, f => f.Random.Words(3));
        RuleFor(x => x.Code, f => new TeamCode(f.Random.AlphaNumeric(5)));
        RuleFor(x => x.Description, f => f.Random.Words(5));
        RuleFor(x => x.Type, f => TeamType.Team);
        RuleFor(x => x.ActiveDate, f => activeDate);
        RuleFor(x => x.IsActive, f => true);
    }
}

public static class TeamFakerExtensions
{
    public static TeamFaker WithData(this TeamFaker faker, Guid? id = null, int? key = null, string? name = null, TeamCode? code = null, string? description = null, LocalDate? activeDate = null, LocalDate? inactiveDate = null, bool? isActive = null)
    {
        if (id.HasValue) { faker.RuleFor(x => x.Id, id.Value); }
        if (key.HasValue) { faker.RuleFor(x => x.Key, key.Value); }
        if (!string.IsNullOrWhiteSpace(name)) { faker.RuleFor(x => x.Name, name); }
        if (code is not null) { faker.RuleFor(x => x.Code, code); }
        if (!string.IsNullOrWhiteSpace(description)) { faker.RuleFor(x => x.Description, description); }
        if (activeDate.HasValue) { faker.RuleFor(x => x.ActiveDate, activeDate.Value); }
        if (inactiveDate.HasValue) { faker.RuleFor(x => x.InactiveDate, inactiveDate.Value); }
        if (isActive.HasValue) { faker.RuleFor(x => x.IsActive, isActive.Value); }

        return faker;
    }
}
