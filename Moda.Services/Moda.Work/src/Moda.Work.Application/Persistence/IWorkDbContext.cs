namespace Moda.AppIntegration.Application.Persistence;
public interface IWorkDbContext : IModaDbContext
{
    DbSet<WorkTypeHierarchy> WorkTypeHierarchies { get; }
    DbSet<Workflow> Workflows { get; }
    DbSet<WorkItemLink> WorkItemLinks { get; }
    DbSet<WorkItem> WorkItems { get; }
    DbSet<WorkProcess> WorkProcesses { get; }
    DbSet<Workspace> Workspaces { get; }
    DbSet<WorkStatus> WorkStatuses { get; }
    DbSet<WorkType> WorkTypes { get; }
}
