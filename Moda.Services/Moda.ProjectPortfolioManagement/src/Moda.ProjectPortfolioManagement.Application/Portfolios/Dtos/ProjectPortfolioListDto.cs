using Moda.Common.Application.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Models;

namespace Moda.ProjectPortfolioManagement.Application.Portfolios.Dtos;
public sealed record ProjectPortfolioListDto : IMapFrom<ProjectPortfolio>
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
    /// The description of the portfolio.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// The status of the portfolio.
    /// </summary>
    public required SimpleNavigationDto Status { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<ProjectPortfolio, ProjectPortfolioListDto>()
            .Map(dest => dest.Status, src => SimpleNavigationDto.FromEnum(src.Status));
    }
}
