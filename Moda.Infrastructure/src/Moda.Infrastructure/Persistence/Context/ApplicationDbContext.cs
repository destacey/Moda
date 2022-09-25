using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Moda.Infrastructure.Persistence.Context;

public class ApplicationDbContext : BaseDbContext
{
    public ApplicationDbContext(DbContextOptions options, ICurrentUser currentUser, IDateTimeService dateTimeService, ISerializerService serializer, IOptions<DatabaseSettings> dbSettings, IEventPublisher events)
        : base(options, currentUser, dateTimeService, serializer, dbSettings, events)
    {
    }

    //public DbSet<Workspace> Workspaces => Set<Workspace>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(SchemaNames.Work);
    }
}