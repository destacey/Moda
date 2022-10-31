using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moda.Organization.Application.Persistence;

namespace Moda.Infrastructure.Persistence.Context;

public class ModaDbContext : BaseDbContext, IOrganizationDbContext
{
    public ModaDbContext(DbContextOptions options, ICurrentUser currentUser, IDateTimeService dateTimeService, ISerializerService serializer, IOptions<DatabaseSettings> dbSettings, IEventPublisher events)
        : base(options, currentUser, dateTimeService, serializer, dbSettings, events)
    {
    }

    #region IOrganization
    
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Person> People => Set<Person>();
    
    #endregion IOrganization

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(SchemaNames.Work);
    }
}