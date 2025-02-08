using Moda.Common.Domain.Enums.StrategicManagement;
using Moda.ProjectPortfolioManagement.Domain.Models;
using Moda.Tests.Shared.Data;

namespace Moda.ProjectPortfolioManagement.Domain.Tests.Data;
public sealed class StrategicThemeFaker : PrivateConstructorFaker<StrategicTheme>
{
    public StrategicThemeFaker()
    {
        RuleFor(x => x.Id, f => f.Random.Guid());
        RuleFor(x => x.Key, f => f.Random.Int());
        RuleFor(x => x.Name, f => f.Lorem.Word());
        RuleFor(x => x.Description, f => f.Lorem.Paragraph());
        RuleFor(x => x.State, f => StrategicThemeState.Active);
    }
}

public static class StrategicThemeFakerExtensions
{
    public static StrategicThemeFaker WithData(
        this StrategicThemeFaker faker,
        Guid? id = null,
        int? key = null,
        string? name = null,
        string? description = null,
        StrategicThemeState? state = null)
    {
        if (id.HasValue) { faker.RuleFor(x => x.Id, id.Value); }
        if (key.HasValue) { faker.RuleFor(x => x.Key, key.Value); }
        if (!string.IsNullOrWhiteSpace(name)) { faker.RuleFor(x => x.Name, name); }
        if (!string.IsNullOrWhiteSpace(description)) { faker.RuleFor(x => x.Description, description); }
        if (state.HasValue) { faker.RuleFor(x => x.State, state); }
        return faker;
    }
}
