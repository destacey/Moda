using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.ProjectPortfolioManagement.Domain.Enums;

namespace Moda.ProjectPortfolioManagement.Domain.Models;

public sealed class ExpenditureCategory : BaseEntity<int>, ISystemAuditable
{
    private ExpenditureCategory() { }

    private ExpenditureCategory(string name, string description, ExpenditureCategoryState state, bool isCapitalizable, bool requiresDepreciation, string? accountingCode)
    {
        Name = name;
        Description = description;
        State = state;
        IsCapitalizable = isCapitalizable;
        RequiresDepreciation = requiresDepreciation;
        AccountingCode = accountingCode;
    }

    /// <summary>
    /// The name of the expenditure category (e.g., "Opex", "Capex", "Hybrid", etc.).
    /// </summary>
    public string Name
    {
        get;
        private set => field = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    } = default!;

    /// <summary>
    /// Detailed description of what qualifies under this expenditure category.
    /// </summary>
    public string Description
    {
        get;
        private set => field = Guard.Against.NullOrWhiteSpace(value, nameof(Description)).Trim();
    } = default!;

    /// <summary>
    /// Tracks the lifecycle of the category (e.g., Proposed, Active, Archived).
    /// </summary>
    public ExpenditureCategoryState State { get; private set; }

    /// <summary>
    /// Defines whether the expenditure is treated as capitalizable.
    /// </summary>
    public bool IsCapitalizable { get; private set; }

    /// <summary>
    /// Defines whether the expenditure requires asset depreciation tracking.
    /// </summary>
    public bool RequiresDepreciation { get; private set; }

    /// <summary>
    /// Optional - Regulatory constraints, reporting codes, or financial classifications.
    /// </summary>
    public string? AccountingCode { get; private set; }

    /// <summary>
    /// Indicates whether the expenditure category can be deleted.
    /// </summary>
    /// <returns></returns>
    public bool CanBeDeleted() => State is ExpenditureCategoryState.Proposed;

    /// <summary>
    /// Updates the expenditure category details. Once active, capitalizable and depreciation settings cannot be changed.
    /// </summary>
    public Result Update(string name, string description, bool isCapitalizable, bool requiresDepreciation, string? accountingCode)
    {
        if (State == ExpenditureCategoryState.Archived)
        {
            return Result.Failure("Cannot update an archived expenditure category.");
        }

        if (State == ExpenditureCategoryState.Proposed)
        {
            IsCapitalizable = isCapitalizable;
            RequiresDepreciation = requiresDepreciation;
        }
        else if (State == ExpenditureCategoryState.Active)
        {
            if (IsCapitalizable != isCapitalizable || RequiresDepreciation != requiresDepreciation)
            {
                return Result.Failure("Cannot change capitalizable or depreciation settings once the category is active.");
            }
        }

        Name = name;
        Description = description;
        AccountingCode = accountingCode;

        return Result.Success();
    }

    #region Lifecycle Methods

    /// <summary>
    /// Activates the expenditure category, making its properties immutable.
    /// </summary>
    public Result Activate()
    {
        if (State != ExpenditureCategoryState.Proposed)
        {
            return Result.Failure("Only proposed categories can be activated.");
        }

        State = ExpenditureCategoryState.Active;
        return Result.Success();
    }

    /// <summary>
    /// Archives the expenditure category, preventing further modifications.
    /// </summary>
    public Result Archive()
    {
        if (State != ExpenditureCategoryState.Active)
        {
            return Result.Failure("Only active categories can be archived.");
        }

        State = ExpenditureCategoryState.Archived;
        return Result.Success();
    }

    #endregion Lifecycle Methods

    /// <summary>
    /// Creates a new expenditure category in the proposed state.
    /// </summary>
    public static ExpenditureCategory Create(string name, string description, bool isCapitalizable, bool requiresDepreciation, string? accountingCode = null)
    {
        return new ExpenditureCategory(name, description, ExpenditureCategoryState.Proposed, isCapitalizable, requiresDepreciation, accountingCode);
    }
}
