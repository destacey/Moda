using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moda.Common.Domain.Employees;
using Moda.Goals.Application.Persistence;
using Moda.Goals.Domain.Models;
using Moda.Health;
using Moda.Health.Models;
using Moda.Infrastructure.Common.Services;
using Moda.Links;
using Moda.Links.Models;
using Moda.Planning.Application.Persistence;
using Moda.Planning.Domain.Models;
using Moda.Work.Domain.Models;

namespace Moda.Infrastructure.Persistence.Context;

public class ModaDbContext : BaseDbContext, IAppIntegrationDbContext, IGoalsDbContext, IHealthDbContext, ILinksDbContext, IOrganizationDbContext, IPlanningDbContext, IWorkDbContext
{
    public ModaDbContext(DbContextOptions options, ICurrentUser currentUser, IDateTimeProvider dateTimeProvider, ISerializerService serializer, IOptions<DatabaseSettings> dbSettings, IEventPublisher events, IRequestCorrelationIdProvider requestCorrelationIdProvider)
        : base(options, currentUser, dateTimeProvider, serializer, dbSettings, events, requestCorrelationIdProvider)
    {
    }

    #region Common
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<ExternalEmployeeBlacklistItem> ExternalEmployeeBlacklistItems => Set<ExternalEmployeeBlacklistItem>();

    #endregion Common

    #region IAppIntegration

    public DbSet<Connection> Connections => Set<Connection>();
    public DbSet<AzureDevOpsBoardsConnection> AzureDevOpsBoardsConnections => Set<AzureDevOpsBoardsConnection>();

    #endregion IAppIntegration

    #region IGoals

    public DbSet<Objective> Objectives => Set<Objective>();

    #endregion IGoals

    #region IHealth

    public DbSet<HealthCheck> HealthChecks => Set<HealthCheck>();

    #endregion IHealth

    #region ILinks

    public DbSet<Link> Links => Set<Link>();

    #endregion ILinks

    #region IOrganization

    public DbSet<BaseTeam> BaseTeams => Set<BaseTeam>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamOfTeams> TeamOfTeams => Set<TeamOfTeams>();

    #endregion IOrganization

    #region IPlanning

    public DbSet<PlanningInterval> PlanningIntervals => Set<PlanningInterval>();
    public DbSet<Risk> Risks => Set<Risk>();
    public DbSet<PlanningTeam> PlanningTeams => Set<PlanningTeam>();
    public DbSet<SimpleHealthCheck> PlanningHealthChecks => Set<SimpleHealthCheck>();
    public DbSet<Roadmap> Roadmaps => Set<Roadmap>();
    public DbSet<RoadmapLink> RoadmapLinks => Set<RoadmapLink>();

    #endregion IPlanning

    #region IWork

    public DbSet<WorkTypeHierarchy> WorkTypeHierarchies => Set<WorkTypeHierarchy>();
    public DbSet<Workflow> Workflows => Set<Workflow>();
    public DbSet<WorkItemLink> WorkItemLinks => Set<WorkItemLink>();
    public DbSet<WorkItem> WorkItems => Set<WorkItem>();
    public DbSet<WorkProcess> WorkProcesses => Set<WorkProcess>();
    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<WorkStatus> WorkStatuses => Set<WorkStatus>();
    public DbSet<WorkTeam> WorkTeams => Set<WorkTeam>();
    public DbSet<WorkType> WorkTypes => Set<WorkType>();

    #endregion IWork

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(SchemaNames.Work);
    }
}