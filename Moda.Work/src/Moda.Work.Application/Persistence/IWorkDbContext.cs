using Microsoft.EntityFrameworkCore;

namespace Moda.AppIntegration.Application.Persistence;
public interface IWorkDbContext : IModaDbContext
{
    DbSet<WorkState> WorkStates { get; }
    DbSet<WorkType> WorkTypes { get; }
}
