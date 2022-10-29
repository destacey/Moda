using Microsoft.Extensions.DependencyInjection;

namespace Moda.Infrastructure.Persistence.Initialization;

internal class CustomSeederRunner
{
    private readonly ICustomSeeder[] _seeders;

    public CustomSeederRunner(IServiceProvider serviceProvider) =>
        _seeders = serviceProvider.GetServices<ICustomSeeder>().ToArray();

    public async Task RunSeeders(CancellationToken cancellationToken)
    {
        foreach (var seeder in _seeders)
        {
            await seeder.Initialize(cancellationToken);
        }
    }
}