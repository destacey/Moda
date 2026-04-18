using Wayd.ProjectPortfolioManagement.Domain.Models;

namespace Wayd.ProjectPortfolioManagement.Application.Portfolios.Dtos;

public sealed record ProjectPortfolioOptionDto : IMapFrom<ProjectPortfolio>
{
    public Guid Id { get; set; }

    /// <summary>
    /// The name of the portfolio.
    /// </summary>
    public required string Name { get; set; }
}
