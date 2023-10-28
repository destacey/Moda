using Microsoft.EntityFrameworkCore;
using Moda.Common.Application.Persistence;
using Moda.Health.Models;

namespace Moda.Health;
internal interface IHealthDbContext : IModaDbContext
{
    DbSet<HealthCheck> HealthChecks { get; }
}
