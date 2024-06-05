using Microsoft.EntityFrameworkCore;
using Moda.Work.Domain.Models;
using NodaTime;

namespace Moda.Infrastructure.Persistence.Initialization;
public class WorkSeeder : ICustomSeeder
{
    public async Task Initialize(ModaDbContext dbContext, IDateTimeProvider dateTimeProvider, CancellationToken cancellationToken)
    {
        await SeedWorkTypeLevelScheme(dbContext, dateTimeProvider, cancellationToken);
    }

    public static async Task SeedWorkTypeLevelScheme(ModaDbContext dbContext, IDateTimeProvider dateTimeProvider, CancellationToken cancellationToken)
    {
        Instant timestamp = dateTimeProvider.Now;

        if (await dbContext.WorkTypeHierarchies.AnyAsync(cancellationToken))
        {
            WorkTypeHierarchy scheme = await dbContext.WorkTypeHierarchies
                .Include(s => s.Levels)
                .SingleAsync(cancellationToken);
            var result = scheme.Reinitialize(timestamp);
            if (result.IsFailure)
            {
                throw new InvalidOperationException(result.Error);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
        else
        {
            // TODO: This will never be true since the migration adds the default Other work type level.
            WorkTypeHierarchy scheme = WorkTypeHierarchy.Initialize(timestamp);
            dbContext.WorkTypeHierarchies.Add(scheme);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
