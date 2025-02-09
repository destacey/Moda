using Moda.Common.Application.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Models;

namespace Moda.ProjectPortfolioManagement.Application.Portfolios.Dtos;
public sealed record ProjectPortfolioDetailsDto : IMapFrom<ProjectPortfolio>
{
    public Guid Id { get; set; }

    /// <summary>
    /// The unique key of the portfolio.  This is an alternate key to the Id.
    /// </summary>
    public int Key { get; set; }

    /// <summary>
    /// The name of the portfolio.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// A detailed description of the portfolio’s purpose.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// The status of the portfolio.
    /// </summary>
    public required SimpleNavigationDto Status { get; set; }

    /// <summary>
    /// The date range defining the portfolio’s lifecycle.
    /// </summary>
    public FlexibleDateRange? DateRange { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<ProjectPortfolio, ProjectPortfolioDetailsDto>()
            .Map(dest => dest.Status, src => SimpleNavigationDto.FromEnum(src.Status));
    }
}
