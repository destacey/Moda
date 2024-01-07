using Microsoft.EntityFrameworkCore;
using Moda.Work.Domain.Models;
using NodaTime;

namespace Moda.Infrastructure.Persistence.Initialization;
public class WorkSeeder : ICustomSeeder
{
    public async Task Initialize(ModaDbContext dbContext, IDateTimeProvider dateTimeManager, CancellationToken cancellationToken)
    {
        await SeedBacklogLevelScheme(dbContext, dateTimeManager, cancellationToken);
    }

    public async Task SeedBacklogLevelScheme(ModaDbContext dbContext, IDateTimeProvider dateTimeManager, CancellationToken cancellationToken)
    {
        Instant timestamp = dateTimeManager.Now;

        if (await dbContext.BacklogLevelSchemes.AnyAsync(cancellationToken))
        {
            BacklogLevelScheme scheme = await dbContext.BacklogLevelSchemes
                .Include(s => s.BacklogLevels)
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
            BacklogLevelScheme scheme = BacklogLevelScheme.Initialize(timestamp);
            dbContext.BacklogLevelSchemes.Add(scheme);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
