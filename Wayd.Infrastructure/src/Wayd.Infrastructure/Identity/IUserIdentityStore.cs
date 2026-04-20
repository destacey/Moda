namespace Wayd.Infrastructure.Identity;

/// <summary>
/// Narrow seam over <see cref="UserIdentity"/> persistence. Introduced so identity-
/// write paths in <see cref="UserService"/> and <see cref="Auth.Local.TokenService"/>
/// can be unit-tested without spinning up a full <c>WaydDbContext</c>.
/// </summary>
internal interface IUserIdentityStore : IScopedService
{
    Task<UserIdentity?> FindActive(string provider, string? tenantId, string subject, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns up to two active rows matching (provider, subject) with NULL tenant.
    /// Used by the Entra null-tid upgrade path to detect ambiguous matches.
    /// </summary>
    Task<IReadOnlyList<UserIdentity>> FindActiveByNullTenant(string provider, string subject, CancellationToken cancellationToken = default);

    Task<bool> ExistsActive(string userId, string provider, CancellationToken cancellationToken = default);

    Task Add(UserIdentity identity, CancellationToken cancellationToken = default);

    Task Update(UserIdentity identity, CancellationToken cancellationToken = default);
}
