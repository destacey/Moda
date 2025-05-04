using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moda.Goals.Application.Persistence;
using Moda.Health;
using Moda.Links;
using Moda.Planning.Application.Persistence;
using Moda.ProjectPortfolioManagement.Application;
using Moda.StrategicManagement.Application;
using Moda.Work.Application.Persistence;
using Neo4j.Driver;
using Neo4jClient;
using Serilog;

namespace Moda.Infrastructure.Persistence;

internal static class ConfigureServices
{
    private static readonly Serilog.ILogger _logger = Log.ForContext(typeof(ConfigureServices));

    internal static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration config)
    {
        // TODO: there must be a cleaner way to do IOptions validation...
        var databaseSettings = config.GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>();
        if (databaseSettings is null)
            throw new InvalidOperationException("DatabaseSettings is not configured.");

        var neo4jSettings = config.GetSection(nameof(Neo4jSettings)).Get<Neo4jSettings>();
        if (neo4jSettings is null)
            throw new InvalidOperationException("Neo4jSettings is not configured.");

        string? rootConnectionString = databaseSettings.ConnectionString;
        if (string.IsNullOrWhiteSpace(rootConnectionString))
            throw new InvalidOperationException("DB ConnectionString is not configured.");

        string? dbProvider = databaseSettings.DBProvider;
        if (string.IsNullOrWhiteSpace(dbProvider))
            throw new InvalidOperationException("DB Provider is not configured.");

        _logger.Information($"Current DB Provider : {dbProvider}");

        return services
            .Configure<DatabaseSettings>(config.GetSection(nameof(DatabaseSettings)))
            .Configure<Neo4jSettings>(config.GetSection(nameof(Neo4jSettings)))

            .AddDbContext<ModaDbContext>(m => m.UseDatabase(dbProvider, rootConnectionString))
            .AddDomainDbContexts()

            .AddTransient<IDatabaseInitializer, DatabaseInitializer>()
            .AddTransient<ApplicationDbInitializer>()
            .AddTransient<ApplicationDbSeeder>()
            .AddServices(typeof(ICustomSeeder), ServiceLifetime.Transient)
            .AddTransient<CustomSeederRunner>()

            .AddTransient<IConnectionStringSecurer, ConnectionStringSecurer>()
            .AddTransient<IConnectionStringValidator, ConnectionStringValidator>()

            .AddSingleton<IGraphClient>((sp) =>
            {
                var client = new GraphClient(new Uri(neo4jSettings.Database), neo4jSettings.User, neo4jSettings.Password);
                var connectTask = client.ConnectAsync();
                connectTask.Wait();
                if (connectTask.IsFaulted)
                {
                    _logger.Error("Failed to connect to Neo4j database.");
                    throw new InvalidOperationException("Failed to connect to Neo4j database.");
                }
                return client;
            });
    }

    internal static DbContextOptionsBuilder UseDatabase(this DbContextOptionsBuilder builder, string dbProvider, string connectionString)
    {
        switch (dbProvider.ToLowerInvariant())
        {
            case DbProviderKeys.SqlServer:
                return builder.UseSqlServer(connectionString, options =>
                {
                    options.MigrationsAssembly("Moda.Infrastructure.Migrators.MSSQL");
                    options.UseNodaTime();
                });

            //case DbProviderKeys.Npgsql:
            //    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            //    return builder.UseNpgsql(connectionString, options =>
            //    {
            //        options.MigrationsAssembly("Moda.Infrastructure.Migrators.PostgreSQL");
            //        options.UseNodaTime();
            //    });

            default:
                throw new InvalidOperationException($"DB Provider {dbProvider} is not supported.");
        }
    }

    private static IServiceCollection AddDomainDbContexts(this IServiceCollection services)
    {
        services.AddScoped<IModaDbContext, ModaDbContext>();
        services.AddScoped<IAppIntegrationDbContext, ModaDbContext>();
        services.AddScoped<IGoalsDbContext, ModaDbContext>();
        services.AddScoped<IHealthDbContext, ModaDbContext>();
        services.AddScoped<ILinksDbContext, ModaDbContext>();
        services.AddScoped<IOrganizationDbContext, ModaDbContext>();
        services.AddScoped<IPlanningDbContext, ModaDbContext>();
        services.AddScoped<IProjectPortfolioManagementDbContext, ModaDbContext>();
        services.AddScoped<IStrategicManagementDbContext, ModaDbContext>();
        services.AddScoped<IWorkDbContext, ModaDbContext>();

        return services;
    }
}