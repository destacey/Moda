using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Moda.Common.Application.FeatureManagement;
using Moda.Common.Domain.FeatureManagement;

namespace Moda.Infrastructure.FeatureManagement;

internal sealed class DatabaseFeatureDefinitionProvider(
    IServiceScopeFactory scopeFactory,
    IMemoryCache cache,
    ILogger<DatabaseFeatureDefinitionProvider> logger) : IFeatureDefinitionProvider, IFeatureFlagCacheInvalidator
{
    private const string CacheKey = "FeatureManagement:AllDefinitions";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);

    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly IMemoryCache _cache = cache;
    private readonly ILogger<DatabaseFeatureDefinitionProvider> _logger = logger;

    public async Task<FeatureDefinition?> GetFeatureDefinitionAsync(string featureName)
    {
        var definitions = await GetCachedDefinitions();
        definitions.TryGetValue(featureName, out var definition);
        return definition;
    }

    public async IAsyncEnumerable<FeatureDefinition> GetAllFeatureDefinitionsAsync()
    {
        var definitions = await GetCachedDefinitions();
        foreach (var definition in definitions.Values)
        {
            yield return definition;
        }
    }

    public void InvalidateCache()
    {
        _cache.Remove(CacheKey);
        _logger.LogDebug("Feature flag cache invalidated.");
    }

    private async Task<Dictionary<string, FeatureDefinition>> GetCachedDefinitions()
    {
        if (_cache.TryGetValue<Dictionary<string, FeatureDefinition>>(CacheKey, out var cached) && cached is not null)
            return cached;

        var definitions = await LoadDefinitionsFromDatabase();

        _cache.Set(CacheKey, definitions, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheExpiration
        });

        return definitions;
    }

    private async Task<Dictionary<string, FeatureDefinition>> LoadDefinitionsFromDatabase()
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IFeatureManagementDbContext>();

            var flags = await dbContext.FeatureFlags
                .AsNoTracking()
                .Where(f => !f.IsArchived)
                .ToListAsync();

            var definitions = new Dictionary<string, FeatureDefinition>(StringComparer.OrdinalIgnoreCase);
            foreach (var flag in flags)
            {
                definitions[flag.Name] = ToFeatureDefinition(flag);
            }

            _logger.LogDebug("Loaded {Count} feature flag definitions from database.", definitions.Count);
            return definitions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load feature flag definitions from database. Returning empty set.");
            return new Dictionary<string, FeatureDefinition>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static FeatureDefinition ToFeatureDefinition(FeatureFlag flag)
    {
        var definition = new FeatureDefinition
        {
            Name = flag.Name,
            Telemetry = new TelemetryConfiguration { Enabled = true }
        };

        if (flag.IsEnabled)
        {
            definition.EnabledFor =
            [
                new FeatureFilterConfiguration { Name = "AlwaysOn" }
            ];
        }
        else
        {
            definition.EnabledFor = [];
        }

        return definition;
    }
}
