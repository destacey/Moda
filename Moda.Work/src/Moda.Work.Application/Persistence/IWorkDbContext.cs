using Microsoft.EntityFrameworkCore;

namespace Moda.AppIntegration.Application.Persistence;
public interface IWorkDbContext : IModaDbContext
{
    DbSet<BacklogLevel> BacklogLevels { get; }
    DbSet<WorkState> WorkStates { get; }
    DbSet<WorkType> WorkTypes { get; }
}
