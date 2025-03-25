using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.ProjectPortfolioManagement.Domain.Models;
using Moda.Tests.Shared.Data;

namespace Moda.ProjectPortfolioManagement.Domain.Tests.Data;
public sealed class ExpenditureCategoryFaker : PrivateConstructorFaker<ExpenditureCategory>
{
    public ExpenditureCategoryFaker()
    {
        RuleFor(x => x.Id, f => f.Random.Int(1, 1000));
        RuleFor(x => x.Name, f => f.Commerce.Department()); 
        RuleFor(x => x.Description, f => f.Lorem.Sentence());
        RuleFor(x => x.State, f => f.PickRandom<ExpenditureCategoryState>());
        RuleFor(x => x.IsCapitalizable, f => f.Random.Bool());
        RuleFor(x => x.RequiresDepreciation, (f, x) => x.IsCapitalizable && f.Random.Bool()); // If capitalizable, may require depreciation
        RuleFor(x => x.AccountingCode, f => f.Random.Bool() ? f.Finance.Account() : null);
    }

    public ExpenditureCategoryFaker WithData(
        string? name = null,
        string? description = null,
        ExpenditureCategoryState? state = null,
        bool? isCapitalizable = null,
        bool? requiresDepreciation = null,
        string? accountingCode = null)
    {
        if (!string.IsNullOrWhiteSpace(name)) RuleFor(x => x.Name, name);
        if (!string.IsNullOrWhiteSpace(description)) RuleFor(x => x.Description, description);
        if (state.HasValue) RuleFor(x => x.State, state.Value);
        if (isCapitalizable.HasValue) RuleFor(x => x.IsCapitalizable, isCapitalizable.Value);
        if (requiresDepreciation.HasValue) RuleFor(x => x.RequiresDepreciation, requiresDepreciation.Value);
        if (!string.IsNullOrWhiteSpace(accountingCode)) RuleFor(x => x.AccountingCode, accountingCode);

        return this;
    }

    public ExpenditureCategory GenerateProposed() => WithData(state: ExpenditureCategoryState.Proposed).Generate();
    public ExpenditureCategory GenerateActive() => WithData(state: ExpenditureCategoryState.Active).Generate();
    public ExpenditureCategory GenerateArchived() => WithData(state: ExpenditureCategoryState.Archived).Generate();
}
