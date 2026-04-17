using Wayd.StrategicManagement.Domain.Models;

namespace Wayd.StrategicManagement.Application;

public interface IStrategicManagementDbContext : IWaydDbContext
{
    DbSet<Strategy> Strategies { get; }
    DbSet<StrategicTheme> StrategicThemes { get; }
    DbSet<Vision> Visions { get; }
}
