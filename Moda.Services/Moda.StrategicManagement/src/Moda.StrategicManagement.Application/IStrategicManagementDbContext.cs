using Moda.StrategicManagement.Domain.Models;

namespace Moda.StrategicManagement.Application;

public interface IStrategicManagementDbContext : IModaDbContext
{
    DbSet<Strategy> Strategies { get; }
    DbSet<StrategicTheme> StrategicThemes { get; }
    DbSet<Vision> Visions { get; }
}
