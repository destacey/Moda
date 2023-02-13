using Microsoft.Extensions.DependencyInjection;

namespace Moda.Infrastructure.Persistence.Initialization;

internal class CustomSeederRunner
{
    private readonly ICustomSeeder[] _seeders;
    private readonly ModaDbContext _dbContext;
    private readonly IDateTimeService _dateTimeService;

    public CustomSeederRunner(IServiceProvider serviceProvider, ModaDbContext dbContext, IDateTimeService dateTimeService)
    {
        _seeders = serviceProvider.GetServices<ICustomSeeder>().ToArray();
        _dbContext = dbContext;
        _dateTimeService = dateTimeService;
    }

    public async Task RunSeeders(CancellationToken cancellationToken)
    {
        foreach (var seeder in _seeders)
        {
            await seeder.Initialize(_dbContext, _dateTimeService, cancellationToken);
        }
    }
}