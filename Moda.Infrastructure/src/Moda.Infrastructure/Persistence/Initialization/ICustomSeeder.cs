namespace Moda.Infrastructure.Persistence.Initialization;

public interface ICustomSeeder
{
    Task Initialize(ModaDbContext dbContext, IDateTimeProvider dateTimeManager, CancellationToken cancellationToken);
}