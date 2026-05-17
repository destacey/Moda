using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wayd.Common.Application.Identity.OidcProviders;

namespace Wayd.Infrastructure.Auth.Oidc;

/// <summary>
/// Singleton, in-memory cache over the <c>OidcProviders</c> table. The token
/// validator is on a hot path — every API call from an Entra-authenticated user
/// triggers an exchange in some flows — so reading the row from SQL every time
/// is wasteful. A short TTL trades a small staleness window for a much smaller
/// query rate, and admin writes invalidate explicitly so latency is bounded by
/// the explicit invalidation, not the TTL.
/// </summary>
/// <remarks>
/// Singleton lifetime is required so the cache persists across requests and so
/// the same instance is shared with the <c>OidcTokenValidator</c>'s per-Authority
/// <c>ConfigurationManager</c> dictionary (which similarly must outlive a request
/// to keep its JWKS cache warm). DB access goes through <see cref="IServiceScopeFactory"/>
/// because <c>WaydDbContext</c> is scoped.
/// </remarks>
internal sealed class OidcProviderRegistry : IOidcProviderRegistry
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;

    private readonly ConcurrentDictionary<string, CacheEntry<OidcProvider?>> _byName =
        new(StringComparer.OrdinalIgnoreCase);
    private CacheEntry<IReadOnlyList<OidcProvider>>? _allEnabled;
    private readonly object _allEnabledLock = new();

    public OidcProviderRegistry(IServiceScopeFactory scopeFactory, TimeProvider timeProvider)
    {
        _scopeFactory = scopeFactory;
        _timeProvider = timeProvider;
    }

    public async Task<OidcProvider?> GetByName(string name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;

        if (_byName.TryGetValue(name, out var cached) && !IsExpired(cached))
        {
            return cached.Value;
        }

        // Tracking is unnecessary for read-only validator/listing paths and
        // hangs onto entities for the lifetime of the request. Detach with
        // AsNoTracking so the singleton-held cache doesn't drag the EF
        // change-tracker graph along with it.
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WaydDbContext>();
        var provider = await db.OidcProviders
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Name == name, cancellationToken);

        _byName[name] = new CacheEntry<OidcProvider?>(provider, _timeProvider.GetUtcNow());
        return provider;
    }

    public async Task<IReadOnlyList<OidcProvider>> GetEnabled(CancellationToken cancellationToken)
    {
        // Fast path: unsynchronized read covers the common case (cache warm).
        // On a miss, multiple concurrent callers may each run the DB query — the
        // lock only protects the write so the last writer wins cleanly. Redundant
        // queries on concurrent cold-start are acceptable; the TTL keeps them rare.
        if (_allEnabled is { } current && !IsExpired(current))
        {
            return current.Value;
        }

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WaydDbContext>();
        var providers = await db.OidcProviders
            .AsNoTracking()
            .Where(p => p.IsEnabled)
            .OrderBy(p => p.DisplayName)
            .ToListAsync(cancellationToken);

        lock (_allEnabledLock)
        {
            _allEnabled = new CacheEntry<IReadOnlyList<OidcProvider>>(providers, _timeProvider.GetUtcNow());
        }
        return providers;
    }

    public void Invalidate(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        _byName.TryRemove(name, out _);
        // The enabled-listing reflects which providers are returned; updating
        // an individual provider can change whether it appears in the list
        // (IsEnabled flipped) or its DisplayName ordering. Cheaper to invalidate
        // the list along with the entry than to reason about whether this
        // particular edit affected the listing.
        InvalidateAll();
    }

    public void InvalidateAll()
    {
        lock (_allEnabledLock)
        {
            _allEnabled = null;
        }
    }

    private bool IsExpired<T>(CacheEntry<T> entry)
        => _timeProvider.GetUtcNow() - entry.CachedAt > CacheTtl;

    private readonly record struct CacheEntry<T>(T Value, DateTimeOffset CachedAt);
}
