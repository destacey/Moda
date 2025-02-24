using Moda.ProjectPortfolioManagement.Domain.Models;

namespace Moda.ProjectPortfolioManagement.Application.Portfolios.Dtos;
public sealed record ProjectPortfolioOptionDto : IMapFrom<ProjectPortfolio>
{
    public Guid Id { get; set; }

    /// <summary>
    /// The name of the portfolio.
    /// </summary>
    public required string Name { get; set; }
}
