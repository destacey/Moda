using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Options;
using Moda.Infrastructure.Persistence.Extensions;

namespace Moda.Infrastructure.Persistence.Context;

public abstract class BaseDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string, IdentityUserClaim<string>, IdentityUserRole<string>, IdentityUserLogin<string>, ApplicationRoleClaim, IdentityUserToken<string>>
{
    protected readonly ICurrentUser _currentUser;
    protected readonly IDateTimeProvider _dateTimeManager;
    private readonly ISerializerService _serializer;
    private readonly DatabaseSettings _dbSettings;
    private readonly IEventPublisher _events;

    protected BaseDbContext(DbContextOptions options, ICurrentUser currentUser, IDateTimeProvider dateTimeManager, ISerializerService serializer, IOptions<DatabaseSettings> dbSettings, IEventPublisher events)
        : base(options)
    {
        _currentUser = currentUser;
        _dateTimeManager = dateTimeManager;
        _serializer = serializer;
        _dbSettings = dbSettings.Value;
        _events = events;

        // this is need so that the owned entities are soft deleted correctly
        ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;
        ChangeTracker.DeleteOrphansTiming = CascadeTiming.OnSaveChanges;
    }

    // Used by Dapper
    public IDbConnection Connection => Database.GetDbConnection();

    public DbSet<Trail> AuditTrails => Set<Trail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // QueryFilters need to be applied before base.OnModelCreating
        modelBuilder.AppendGlobalQueryFilter<ISoftDelete>(s => !s.IsDeleted);

        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // TODO: We want this only for development probably... maybe better make it configurable in logger.json config?
        optionsBuilder.EnableSensitiveDataLogging();

        // If you want to see the sql queries that efcore executes:

        // Uncomment the next line to see them in the output window of visual studio
        // optionsBuilder.LogTo(m => Debug.WriteLine(m), LogLevel.Information);

        // Or uncomment the next line if you want to see them in the console
        // optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);

        optionsBuilder.UseDatabase(_dbSettings.DBProvider!, _dbSettings.ConnectionString!);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        var auditEntries = HandleAuditingBeforeSaveChanges(_currentUser.GetUserId());

        int result = await base.SaveChangesAsync(cancellationToken);

        await HandleAuditingAfterSaveChangesAsync(auditEntries, cancellationToken);

        await SendDomainEventsAsync();

        return result;
    }

    private List<AuditTrail> HandleAuditingBeforeSaveChanges(Guid userId)
    {
        foreach (var entry in ChangeTracker.Entries<IAuditable>().ToList())
        {
            var timestamp = _dateTimeManager.Now;

            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedBy = userId;
                entry.Entity.Created = timestamp;
            }

            if (entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.HasChangedOwnedEntities())
            {
                entry.Entity.LastModifiedBy = userId;
                entry.Entity.LastModified = timestamp;
            }

            if (entry.State == EntityState.Deleted && entry.Entity is ISoftDelete softDelete)
            {
                softDelete.IsDeleted = true;
                softDelete.DeletedBy = userId;
                softDelete.Deleted = timestamp;
                entry.State = EntityState.Modified;
            }
        }

        ChangeTracker.DetectChanges();

        var correlationId = Guid.NewGuid();

        var trailEntries = new List<AuditTrail>();
        foreach (var entry in ChangeTracker.Entries<IAuditable>()
            .Where(e => e.State is EntityState.Added or EntityState.Deleted or EntityState.Modified || e.HasChangedOwnedEntities())
            .ToList())
        {
            var trailEntry = new AuditTrail(entry, _serializer, _dateTimeManager)
            {
                SchemaName = entry.Metadata.GetSchema(),
                TableName = entry.Entity.GetType().Name,
                UserId = userId,
                CorrelationId = correlationId
            };
            trailEntries.Add(trailEntry);
            foreach (var property in entry.Properties)
            {
                if (property.IsTemporary)
                {
                    trailEntry.TemporaryProperties.Add(property);
                    continue;
                }

                string propertyName = property.Metadata.Name;
                if (property.Metadata.IsPrimaryKey())
                {
                    trailEntry.KeyValues[propertyName] = property.CurrentValue;
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        trailEntry.TrailType = TrailType.Create;
                        trailEntry.NewValues[propertyName] = property.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        trailEntry.TrailType = TrailType.Delete;
                        trailEntry.OldValues[propertyName] = property.OriginalValue;
                        break;

                    case EntityState.Modified:
                        // TODO: IsModified appears to always be true
                        if (property.IsModified && ((property.OriginalValue is null && property.CurrentValue is not null) || property.OriginalValue?.Equals(property.CurrentValue) == false))
                        {
                            trailEntry.ChangedColumns.Add(propertyName);
                            trailEntry.TrailType = TrailType.Update;
                            trailEntry.OldValues[propertyName] = property.OriginalValue;
                            trailEntry.NewValues[propertyName] = property.CurrentValue;
                        }
                        break;
                }

            }

            // set trailtype to SoftDelete if the entity is soft deleted
            if (entry.State == EntityState.Modified && entry.Entity is ISoftDelete softDeleteEntity && softDeleteEntity.IsDeleted
                    && trailEntry.OldValues.TryGetValue("IsDeleted", out var oldIsDeletedValue) && oldIsDeletedValue is not null && (bool)oldIsDeletedValue == false)
            {
                trailEntry.TrailType = TrailType.SoftDelete;
            }
        }

        foreach (var auditEntry in trailEntries.Where(e => !e.HasTemporaryProperties))
        {
            AuditTrails.Add(auditEntry.ToAuditTrail());
        }

        return trailEntries.Where(e => e.HasTemporaryProperties).ToList();
    }

    private Task HandleAuditingAfterSaveChangesAsync(List<AuditTrail> trailEntries, CancellationToken cancellationToken = new())
    {
        if (trailEntries == null || trailEntries.Count == 0)
        {
            return Task.CompletedTask;
        }

        foreach (var entry in trailEntries)
        {
            foreach (var prop in entry.TemporaryProperties)
            {
                if (prop.Metadata.IsPrimaryKey())
                {
                    entry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                }
                else
                {
                    entry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                }
            }

            AuditTrails.Add(entry.ToAuditTrail());
        }

        return SaveChangesAsync(cancellationToken);
    }

    private async Task SendDomainEventsAsync()
    {
        var entitiesWithEvents = ChangeTracker.Entries<IEntity>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Count > 0)
            .ToArray();

        foreach (var entity in entitiesWithEvents)
        {
            var domainEvents = entity.DomainEvents.ToArray();
            entity.ClearDomainEvents();
            foreach (var domainEvent in domainEvents)
            {
                await _events.PublishAsync(domainEvent);
            }
        }
    }
}