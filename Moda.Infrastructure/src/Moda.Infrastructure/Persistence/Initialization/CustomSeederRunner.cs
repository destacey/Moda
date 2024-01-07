using Microsoft.Extensions.DependencyInjection;

namespace Moda.Infrastructure.Persistence.Initialization;

internal class CustomSeederRunner
{
    private readonly ICustomSeeder[] _seeders;
    private readonly ModaDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CustomSeederRunner(IServiceProvider serviceProvider, ModaDbContext dbContext, IDateTimeProvider dateTimeProvider)
    {
        _seeders = serviceProvider.GetServices<ICustomSeeder>().ToArray();
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task RunSeeders(CancellationToken cancellationToken)
    {
        foreach (var seeder in _seeders)
        {
            await seeder.Initialize(_dbContext, _dateTimeProvider, cancellationToken);
        }
    }
}