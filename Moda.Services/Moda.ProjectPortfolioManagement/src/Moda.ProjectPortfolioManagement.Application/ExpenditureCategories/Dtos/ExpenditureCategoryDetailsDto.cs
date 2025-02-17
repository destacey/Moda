using Moda.Common.Application.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Models;

namespace Moda.ProjectPortfolioManagement.Application.ExpenditureCategories.Dtos;

public sealed record ExpenditureCategoryDetailsDto : IMapFrom<ExpenditureCategory>
{
    public int Id { get; set; }

    /// <summary>
    /// The name of the expenditure category (e.g., "Opex", "Capex", "Hybrid", etc.).
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Detailed description of what qualifies under this expenditure category.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Tracks the lifecycle of the category (e.g., Proposed, Active, Archived).
    /// </summary>
    public required SimpleNavigationDto State { get; set; }

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

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<ExpenditureCategory, ExpenditureCategoryDetailsDto>()
            .Map(dest => dest.State, src => SimpleNavigationDto.FromEnum(src.State));
    }
}
