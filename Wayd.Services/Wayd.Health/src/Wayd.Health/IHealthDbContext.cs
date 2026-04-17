using Microsoft.EntityFrameworkCore;
using Wayd.Common.Application.Persistence;
using Wayd.Health.Models;

namespace Wayd.Health;

public interface IHealthDbContext : IWaydDbContext
{
    DbSet<HealthCheck> HealthChecks { get; }
}
