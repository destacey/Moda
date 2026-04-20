using Microsoft.EntityFrameworkCore;

namespace Wayd.Infrastructure.Identity;

internal sealed class UserIdentityStore(WaydDbContext db) : IUserIdentityStore
{
    private readonly WaydDbContext _db = db;

    public Task<UserIdentity?> FindActive(string provider, string? tenantId, string subject, CancellationToken cancellationToken = default)
    {
        return _db.UserIdentities
            .Include(ui => ui.User)
            .FirstOrDefaultAsync(ui =>
                ui.IsActive &&
                ui.Provider == provider &&
                ui.ProviderTenantId == tenantId &&
                ui.ProviderSubject == subject,
                cancellationToken);
    }

    public async Task<IReadOnlyList<UserIdentity>> FindActiveByNullTenant(string provider, string subject, CancellationToken cancellationToken = default)
    {
        return await _db.UserIdentities
            .Include(ui => ui.User)
            .Where(ui =>
                ui.IsActive &&
                ui.Provider == provider &&
                ui.ProviderTenantId == null &&
                ui.ProviderSubject == subject)
            .Take(2)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsActive(string userId, string provider, CancellationToken cancellationToken = default)
    {
        return _db.UserIdentities.AnyAsync(ui =>
            ui.UserId == userId &&
            ui.IsActive &&
            ui.Provider == provider,
            cancellationToken);
    }

    public async Task Add(UserIdentity identity, CancellationToken cancellationToken = default)
    {
        _db.UserIdentities.Add(identity);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task Update(UserIdentity identity, CancellationToken cancellationToken = default)
    {
        _db.UserIdentities.Update(identity);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
