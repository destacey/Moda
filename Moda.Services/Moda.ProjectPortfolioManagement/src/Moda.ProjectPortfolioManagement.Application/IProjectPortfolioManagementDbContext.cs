using Moda.ProjectPortfolioManagement.Domain.Models;
using Moda.ProjectPortfolioManagement.Domain.Models.StrategicInitiatives;

namespace Moda.ProjectPortfolioManagement.Application;
public interface IProjectPortfolioManagementDbContext : IModaDbContext
{
    DbSet<ExpenditureCategory> ExpenditureCategories { get; }
    DbSet<ProjectPortfolio> Portfolios { get; }
    DbSet<Program> Programs { get; }
    DbSet<Project> Projects { get; }
    DbSet<StrategicTheme> PpmStrategicThemes { get; }
    DbSet<StrategicInitiative> StrategicInitiatives { get; }
}
