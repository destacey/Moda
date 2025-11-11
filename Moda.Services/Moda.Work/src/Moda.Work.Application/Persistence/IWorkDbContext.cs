namespace Moda.Work.Application.Persistence;
public interface IWorkDbContext : IModaDbContext
{
    DbSet<WorkTypeHierarchy> WorkTypeHierarchies { get; }
    DbSet<Workflow> Workflows { get; }
    DbSet<WorkItemReference> WorkItemReferences { get; }
    DbSet<WorkItem> WorkItems { get; }
    DbSet<WorkItemDependency> WorkItemDependencies { get; }
    DbSet<WorkItemHierarchy> WorkItemHierarchies { get; }
    DbSet<WorkIteration> WorkIterations { get; }
    DbSet<WorkProcess> WorkProcesses { get; }
    DbSet<WorkProject> WorkProjects { get; }
    DbSet<Workspace> Workspaces { get; }
    DbSet<WorkStatus> WorkStatuses { get; }
    DbSet<WorkTeam> WorkTeams { get; }
    DbSet<WorkType> WorkTypes { get; }
}
