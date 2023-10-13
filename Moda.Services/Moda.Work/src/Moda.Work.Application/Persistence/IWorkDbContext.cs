using Microsoft.EntityFrameworkCore;

namespace Moda.AppIntegration.Application.Persistence;
public interface IWorkDbContext : IModaDbContext
{
    DbSet<BacklogLevelScheme> BacklogLevelSchemes { get; }
    DbSet<WorkProcess> WorkProcesses { get; }
    DbSet<Workspace> Workspaces { get; }
    DbSet<WorkState> WorkStates { get; }
    DbSet<WorkType> WorkTypes { get; }
}
