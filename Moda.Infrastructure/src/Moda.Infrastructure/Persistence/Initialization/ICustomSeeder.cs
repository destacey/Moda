namespace Moda.Infrastructure.Persistence.Initialization;

public interface ICustomSeeder
{
    Task Initialize(ModaDbContext dbContext, IDateTimeService dateTimeService, CancellationToken cancellationToken);
}