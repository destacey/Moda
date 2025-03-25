using Moda.ProjectPortfolioManagement.Application.ExpenditureCategories.Commands;

namespace Moda.Web.Api.Models.Ppm.ExpenditureCategories;

public sealed record UpdateExpenditureCategoryRequest
{
    public int Id { get; set; }

    /// <summary>
    /// The name of the expenditure category (e.g., "Opex", "Capex", "Hybrid", etc.).
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Detailed description of what qualifies under this expenditure category.
    /// </summary>
    public string Description { get; set; } = default!;

    /// <summary>
    /// Defines whether the expenditure is treated as capitalizable.
    /// </summary>
    public bool IsCapitalizable { get; set; }

    /// <summary>
    /// Defines whether the expenditure requires asset depreciation tracking.
    /// </summary>
    public bool RequiresDepreciation { get; set; }

    /// <summary>
    /// Reporting codes or financial classifications for the expenditure category.
    /// </summary>
    public string? AccountingCode { get; set; }

    public UpdateExpenditureCategoryCommand ToUpdateExpenditureCategoryCommand()
    {
        return new UpdateExpenditureCategoryCommand(Id, Name, Description, IsCapitalizable, RequiresDepreciation, AccountingCode);
    }
}

public sealed class UpdateExpenditureCategoryRequestValidator : AbstractValidator<UpdateExpenditureCategoryRequest>
{
    public UpdateExpenditureCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(1024);

        RuleFor(x => x.AccountingCode)
            .MaximumLength(64);
    }
}