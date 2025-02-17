using Moda.ProjectPortfolioManagement.Application.ExpenditureCategories.Commands;

namespace Moda.Web.Api.Models.Ppm.ExpenditureCategories;

public sealed record CreateExpenditureCategoryRequest
{
    /// <summary>
    /// The name of the expenditure category (e.g., "Opex", "Capex", "Hybrid", etc.).
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Detailed description of what qualifies under this expenditure category.
    /// </summary>
    public required string Description { get; set; }

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

    public CreateExpenditureCategoryCommand ToCreateExpenditureCategoryCommand()
    {
        return new CreateExpenditureCategoryCommand(Name, Description, IsCapitalizable, RequiresDepreciation, AccountingCode);
    }
}

public sealed class CreateExpenditureCategoryRequestValidator : AbstractValidator<CreateExpenditureCategoryRequest>
{
    public CreateExpenditureCategoryRequestValidator()
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