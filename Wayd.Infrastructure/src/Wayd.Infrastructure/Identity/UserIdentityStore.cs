using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Wayd.Infrastructure.Identity;

internal sealed class UserIdentityStore(WaydDbContext db) : IUserIdentityStore
{
    // SQL Server error numbers for unique-constraint / duplicate-key violations.
    // 2627 = unique constraint, 2601 = unique index. We treat either as "another
    // concurrent caller inserted the same logical row first" — the caller's goal
    // (ensure this row exists) is satisfied either way.
    private const int SqlUniqueConstraintErrorNumber = 2627;
    private const int SqlUniqueIndexErrorNumber = 2601;

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

    public async Task<int> DeactivateAllActive(string userId, Instant unlinkedAt, string unlinkReason, CancellationToken cancellationToken = default)
    {
        var active = await _db.UserIdentities
            .Where(ui => ui.UserId == userId && ui.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var row in active)
        {
            row.IsActive = false;
            row.UnlinkedAt = unlinkedAt;
            row.UnlinkReason = unlinkReason;
        }

        if (active.Count > 0)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }

        return active.Count;
    }

    public async Task<bool> TryPopulateTenant(Guid identityId, string tenantId, CancellationToken cancellationToken = default)
    {
        // Conditional UPDATE: only succeeds if the row still has a NULL tenant.
        // Two concurrent logins could both see the backfilled NULL-tenant row; the
        // one whose UPDATE lands first wins, the other gets 0 rows affected and must
        // re-resolve (their tenantId might match the now-populated row, or might not).
        var rowsAffected = await _db.UserIdentities
            .Where(ui => ui.Id == identityId && ui.ProviderTenantId == null)
            .ExecuteUpdateAsync(
                updates => updates.SetProperty(ui => ui.ProviderTenantId, tenantId),
                cancellationToken);

        return rowsAffected == 1;
    }

    public async Task Add(UserIdentity identity, CancellationToken cancellationToken = default)
    {
        _db.UserIdentities.Add(identity);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            // A concurrent caller inserted the same (provider, tenant, subject) row
            // between our caller's ExistsActive check and this insert. The logical
            // outcome ("a row exists") is achieved, so we swallow and detach the
            // rejected entity so the context stays usable.
            _db.Entry(identity).State = EntityState.Detached;
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException is SqlException sql &&
               (sql.Number == SqlUniqueConstraintErrorNumber || sql.Number == SqlUniqueIndexErrorNumber);
    }

    public async Task Update(UserIdentity identity, CancellationToken cancellationToken = default)
    {
        _db.UserIdentities.Update(identity);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task ExecuteInTransaction(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await action(cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
