using Moda.ProjectPortfolioManagement.Domain.Models;

namespace Moda.ProjectPortfolioManagement.Application;
public interface IProjectPortfolioManagementDbContext : IModaDbContext
{
    DbSet<ProjectPortfolio> Portfolios { get; }
    DbSet<Program> Programs { get; }
    DbSet<Project> Projects { get; }
    DbSet<StrategicTheme> PpmStrategicThemes { get; }
}
