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
using Moda.Organization.Application.Teams.Models;
using Moda.Planning.Application.Persistence;
using Moda.Planning.Domain.Models;
using Moda.Planning.Domain.Models.Roadmaps;
using Moda.StrategicManagement.Application;
using Moda.StrategicManagement.Domain.Models;
using Moda.Work.Domain.Models;

namespace Moda.Infrastructure.Persistence.Context;

public class ModaDbContext : BaseDbContext, IAppIntegrationDbContext, IGoalsDbContext, IHealthDbContext, ILinksDbContext, IOrganizationDbContext, IPlanningDbContext, IStrategicManagementDbContext, IWorkDbContext
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

    #endregion IPlanning

    #region IStrategicManagement

    public DbSet<Strategy> Strategies => Set<Strategy>();
    public DbSet<StrategicTheme> StrategicThemes => Set<StrategicTheme>();
    public DbSet<Vision> Visions => Set<Vision>();

    #endregion IStrategicManagement

    #region IWork

    public DbSet<WorkTypeHierarchy> WorkTypeHierarchies => Set<WorkTypeHierarchy>();
    public DbSet<Workflow> Workflows => Set<Workflow>();
    public DbSet<WorkItemReference> WorkItemReferences => Set<WorkItemReference>();
    public DbSet<WorkItem> WorkItems => Set<WorkItem>();
    public DbSet<WorkItemLink> WorkItemLinks => Set<WorkItemLink>();
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

    #region Graph Table Sync

    // TODO: Find a better way to sync graph tables

    public async Task<int> UpsertTeamNode(TeamNode teamNode, CancellationToken cancellationToken)
    {
        return await Database.ExecuteSqlInterpolatedAsync($@"
            MERGE INTO [Organization].[TeamNodes] AS target
            USING (SELECT {teamNode.Id} AS Id) AS source (Id)
            ON target.Id = source.Id
            WHEN MATCHED THEN
                UPDATE SET
                    [Key] = {teamNode.Key},
                    [Name] = {teamNode.Name},
                    [Code] = {teamNode.Code},
                    [Type] = {teamNode.Type.ToString()},  -- ToString() is required
                    [IsActive] = {teamNode.IsActive},
                    [ActiveDate] = {teamNode.ActiveDate},
                    [InactiveDate] = {teamNode.InactiveDate}
            WHEN NOT MATCHED THEN
                INSERT (
                    [Id],
                    [Key],
                    [Name],
                    [Code],
                    [Type],
                    [IsActive],
                    [ActiveDate],
                    [InactiveDate]
                )
                VALUES (
                    {teamNode.Id},
                    {teamNode.Key},
                    {teamNode.Name},
                    {teamNode.Code},
                    {teamNode.Type.ToString()}, -- ToString() is required
                    {teamNode.IsActive},
                    {teamNode.ActiveDate},
                    {teamNode.InactiveDate}
                );
        ", cancellationToken);
    }

    public async Task<int> UpsertTeamMembershipEdge(TeamMembershipEdge membershipEdge, CancellationToken cancellationToken)
    {
        return await Database.ExecuteSqlInterpolatedAsync($@"
           MERGE INTO [Organization].[TeamMembershipEdges] AS target
           USING (SELECT {membershipEdge.Id} AS Id) AS source (Id)
           ON target.Id = source.Id
           WHEN MATCHED THEN
               UPDATE SET
                   [StartDate] = {membershipEdge.StartDate},
                   [EndDate] = {membershipEdge.EndDate}
           WHEN NOT MATCHED THEN
               INSERT (
                   [Id],
                   [StartDate],
                   [EndDate],
                   $from_id,
                   $to_id
               )
               VALUES (
                   {membershipEdge.Id},
                   {membershipEdge.StartDate},
                   {membershipEdge.EndDate},
                   (SELECT $node_id FROM [Organization].[TeamNodes] WHERE Id = {membershipEdge.FromNode.Id}),
                   (SELECT $node_id FROM [Organization].[TeamNodes] WHERE Id = {membershipEdge.ToNode.Id})
               );
        ", cancellationToken);
    }

    public async Task<int> DeleteTeamMembershipEdge(Guid id, CancellationToken cancellationToken)
    {
        return await Database.ExecuteSqlInterpolatedAsync($@"
            DELETE FROM [Organization].[TeamMembershipEdges]
            WHERE Id = {id}
        ", cancellationToken);
    }

    #endregion Graph Table Sync
}