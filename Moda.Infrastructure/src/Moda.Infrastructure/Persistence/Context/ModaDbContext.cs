using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moda.Planning.Application.Persistence;
using Moda.Planning.Domain.Models;
using Moda.Work.Domain.Models;

namespace Moda.Infrastructure.Persistence.Context;

public class ModaDbContext : BaseDbContext, IAppIntegrationDbContext, IOrganizationDbContext, IPlanningDbContext, IWorkDbContext
{
    public ModaDbContext(DbContextOptions options, ICurrentUser currentUser, IDateTimeService dateTimeService, ISerializerService serializer, IOptions<DatabaseSettings> dbSettings, IEventPublisher events)
        : base(options, currentUser, dateTimeService, serializer, dbSettings, events)
    {
    }

    #region IAppIntegration

    public DbSet<Connection> Connections => Set<Connection>();
    public DbSet<AzureDevOpsBoardsConnection> AzureDevOpsBoardsConnections => Set<AzureDevOpsBoardsConnection>();

    #endregion IAppIntegration

    #region IOrganization

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<BaseTeam> BaseTeams => Set<BaseTeam>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamOfTeams> TeamOfTeams => Set<TeamOfTeams>();

    #endregion IOrganization

    #region IPlanning

    public DbSet<ProgramIncrement> ProgramIncrements => Set<ProgramIncrement>();
    public DbSet<Risk> Risks => Set<Risk>();
    public DbSet<PlanningTeam> PlanningTeams => Set<PlanningTeam>();

    #endregion IPlanning

    #region IWork

    public DbSet<BacklogLevelScheme> BacklogLevelSchemes => Set<BacklogLevelScheme>();
    public DbSet<WorkState> WorkStates => Set<WorkState>();
    public DbSet<WorkType> WorkTypes => Set<WorkType>();

    #endregion IWork

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(SchemaNames.Work);
    }
}