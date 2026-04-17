namespace Wayd.Infrastructure.Persistence.Initialization;

public interface ICustomSeeder
{
    Task Initialize(ModaDbContext dbContext, IDateTimeProvider dateTimeProvider, CancellationToken cancellationToken);
}