using Wayd.Common.Extensions;
using Wayd.ProjectPortfolioManagement.Domain.Models;

namespace Wayd.ProjectPortfolioManagement.Application.ExpenditureCategories.Dtos;

public sealed record ExpenditureCategoryOptionDto : IMapFrom<ExpenditureCategory>
{
    public int Id { get; set; }

    /// <summary>
    /// The name of the expenditure category (e.g., "Opex", "Capex", "Hybrid", etc.).
    /// </summary>
    public required string Name { get; set; }

    public required string State { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<ExpenditureCategory, ExpenditureCategoryOptionDto>()
            .Map(dest => dest.State, src => src.State.GetDisplayName());
    }
}
