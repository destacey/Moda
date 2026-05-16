using Wayd.Common.Domain.Identity;

namespace Wayd.Common.Application.Identity.OidcProviders;

/// <summary>
/// Resolves <see cref="OidcProvider"/> rows from the database for use by the
/// token-exchange flow and the public providers-discovery endpoint. Caches reads
/// for a short window so the validator hot path doesn't hammer the DB on every
/// token exchange. Admin write paths must call <see cref="Invalidate"/> after a
/// successful save so the cache doesn't keep serving stale configuration.
/// </summary>
public interface IOidcProviderRegistry
{
    /// <summary>
    /// Returns the provider with the given <see cref="OidcProvider.Name"/>, or
    /// <c>null</c> if none exists. Disabled providers ARE returned — callers
    /// decide whether to honor or reject them. (The token-exchange flow rejects
    /// disabled providers; the admin UI needs to show them.)
    /// </summary>
    Task<OidcProvider?> GetByName(string name, CancellationToken cancellationToken);

    /// <summary>
    /// Returns all enabled providers. Used by the public /api/auth/providers
    /// endpoint to render login options. Disabled and pending providers are
    /// hidden from this list — they don't appear on the login page and exchanges
    /// against them are rejected.
    /// </summary>
    Task<IReadOnlyList<OidcProvider>> GetEnabled(CancellationToken cancellationToken);

    /// <summary>
    /// Drops the cache entry for one provider so the next read goes to the DB.
    /// Called by admin CRUD handlers after a successful write so changes take
    /// effect immediately instead of waiting out the TTL.
    /// </summary>
    void Invalidate(string name);

    /// <summary>
    /// Drops the cache for the "all enabled providers" listing. Used when a
    /// provider's enabled flag changes or a provider is created/deleted, since
    /// either changes the contents of the list itself, not just one entry.
    /// </summary>
    void InvalidateAll();
}
