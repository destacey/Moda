using Microsoft.Extensions.DependencyInjection;

namespace Moda.Infrastructure.Persistence.Initialization;

internal class CustomSeederRunner
{
    private readonly ICustomSeeder[] _seeders;
    private readonly ModaDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeManager;

    public CustomSeederRunner(IServiceProvider serviceProvider, ModaDbContext dbContext, IDateTimeProvider dateTimeManager)
    {
        _seeders = serviceProvider.GetServices<ICustomSeeder>().ToArray();
        _dbContext = dbContext;
        _dateTimeManager = dateTimeManager;
    }

    public async Task RunSeeders(CancellationToken cancellationToken)
    {
        foreach (var seeder in _seeders)
        {
            await seeder.Initialize(_dbContext, _dateTimeManager, cancellationToken);
        }
    }
}