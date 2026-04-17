using Wayd.StrategicManagement.Domain.Models;

namespace Wayd.StrategicManagement.Application;

public interface IStrategicManagementDbContext : IModaDbContext
{
    DbSet<Strategy> Strategies { get; }
    DbSet<StrategicTheme> StrategicThemes { get; }
    DbSet<Vision> Visions { get; }
}
