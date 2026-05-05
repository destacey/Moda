using System.Security.Claims;
using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Wayd.Common.Application.Employees.Queries;
using Wayd.Common.Application.Identity;
using Wayd.Common.Extensions;
using NotFoundException = Wayd.Common.Application.Exceptions.NotFoundException;

namespace Wayd.Infrastructure.Identity;

internal partial class UserService
{
    /// <summary>
    /// Used when authenticating with Microsoft Entra ID.
    ///
    /// Resolves the user via the <c>UserIdentity</c> table, keyed by
    /// (Provider, ProviderTenantId, ProviderSubject) = (MicrosoftEntraId, tid, oid).
    /// If no active identity matches and exactly one identity matches (MicrosoftEntraId, NULL, oid),
    /// that row's ProviderTenantId is populated from the token — the one-time upgrade
    /// path for users backfilled before tenant was persisted.
    /// If still no match and a user has a staged tenant migration whose target matches
    /// the token's tenant, that user's active identity is rebound to (tid, oid) inside
    /// a transaction (see <see cref="TryApplyPendingTenantMigration"/>).
    /// If still no match, the user is created and a new UserIdentity row is inserted.
    /// </summary>
    public async Task<(string Id, string? EmployeeId)> GetOrCreateFromPrincipalAsync(ClaimsPrincipal principal)
    {
        string? objectId = principal.GetObjectId();
        if (string.IsNullOrWhiteSpace(objectId))
        {
            _logger.LogError("Invalid principal objectId");
            throw new InternalServerException("Invalid objectId");
        }

        string? tenantId = principal.GetTenantId();
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            _logger.LogError("Invalid principal tenantId");
            throw new InternalServerException("Invalid tenantId");
        }

        // UPN is the signed-in-token identifier we trust for matching a staged
        // migration. Falls back to the email claim if absent (some tokens don't
        // emit UPN). Null means no migration rebind will be attempted.
        string? upn = principal.FindFirstValue(ClaimTypes.Upn) ?? principal.FindFirstValue(ClaimTypes.Email);

        var isFirstUser = !await _userManager.Users.AnyAsync();

        var user = await ResolveUserByEntraIdentityAsync(tenantId, objectId, upn)
            ?? await CreateOrUpdateFromPrincipalAsync(principal, isFirstUser);

        if (isFirstUser)
        {
            await _userManager.AddToRoleAsync(user, "Admin");
            await _events.PublishAsync(new ApplicationUserUpdatedEvent(user.Id, _dateTimeProvider.Now, true));
        }
        else
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (roles is null || !roles.Any())
            {
                await _userManager.AddToRoleAsync(user, "Basic");
                await _events.PublishAsync(new ApplicationUserUpdatedEvent(user.Id, _dateTimeProvider.Now, true));
            }
        }

        return (user.Id, user.EmployeeId?.ToString());
    }

    private async Task<ApplicationUser?> ResolveUserByEntraIdentityAsync(string tenantId, string objectId, string? upn)
    {
        var identity = await _userIdentityStore.FindActive(LoginProviders.MicrosoftEntraId, tenantId, objectId);
        if (identity is not null)
        {
            return identity.User;
        }

        // One-time upgrade path: rows backfilled from ApplicationUser.ObjectId have
        // ProviderTenantId = NULL. On the user's next login, populate tenant from the
        // token. If more than one such row matches the subject (extremely unlikely
        // oid collision across tenants), bail out — do not auto-link.
        var pending = await _userIdentityStore.FindActiveByNullTenant(LoginProviders.MicrosoftEntraId, objectId);

        if (pending.Count > 1)
        {
            _logger.LogError(
                "Ambiguous tenant upgrade for Entra subject {ObjectId}: {Count} active rows with NULL tenant. Manual resolution required.",
                objectId, pending.Count);
            throw new InternalServerException("Identity resolution ambiguous. Contact an administrator.");
        }

        if (pending.Count == 1)
        {
            var row = pending[0];

            // Conditional UPDATE guards against a concurrent login racing us. If another
            // login populated the tenant first, TryPopulateTenant returns false and we
            // re-resolve — the other login may have set a tenant that matches ours (fine,
            // both users converge on the same row) or a different one (our token has no
            // matching active row, so we fall into the create path).
            var won = await _userIdentityStore.TryPopulateTenant(row.Id, tenantId);
            if (!won)
            {
                _logger.LogInformation(
                    "Tenant upgrade for Entra subject {ObjectId} lost the race to a concurrent login; re-resolving.",
                    objectId);
                return await _userIdentityStore.FindActive(LoginProviders.MicrosoftEntraId, tenantId, objectId)
                    is { } winnerIdentity
                    ? winnerIdentity.User
                    : null;
            }

            return row.User;
        }

        // No active identity for (tid, oid) and no NULL-tenant backfill row to upgrade.
        // Last resort before falling through to create: an admin may have staged a
        // tenant migration for this user, in which case we rebind their existing
        // identity to (tid, oid) instead of creating a duplicate user.
        return await TryApplyPendingTenantMigration(tenantId, objectId, upn);
    }

    /// <summary>
    /// If a user has <c>PendingMigrationTenantId == tenantId</c> and a UPN matching the
    /// signed-in token, atomically:
    ///   1. Deactivate their active <see cref="UserIdentity"/> row (UnlinkReason = TenantMigration).
    ///   2. Insert a new active row with (provider, tenantId, objectId).
    ///   3. Clear PendingMigrationTenantId.
    /// Returns the user on success; null if no migration was staged or no match found.
    /// The user's <c>Id</c> is preserved, so all downstream FKs remain valid.
    /// </summary>
    private async Task<ApplicationUser?> TryApplyPendingTenantMigration(string tenantId, string objectId, string? upn)
    {
        if (string.IsNullOrWhiteSpace(upn))
        {
            return null;
        }

        // Match on UPN (case-insensitive) to scope the lookup. A bare email match
        // would risk rebinding the wrong user if two ApplicationUser rows share an
        // email — match by both UserName (UPN-shaped) and NormalizedEmail to cover
        // tokens whose UPN is the email and accounts whose Email isn't UPN-shaped.
        var normalized = upn.ToUpperInvariant();
        var candidate = await _userManager.Users
            .FirstOrDefaultAsync(u =>
                u.PendingMigrationTenantId == tenantId &&
                u.LoginProvider == LoginProviders.MicrosoftEntraId &&
                (u.NormalizedUserName == normalized || u.NormalizedEmail == normalized));

        if (candidate is null)
        {
            return null;
        }

        // Run the deactivate + insert + flag-clear together. Failure of any step
        // rolls back the others — without the transaction we could deactivate the
        // old identity then fail to insert the new one, locking the user out.
        await _userIdentityStore.ExecuteInTransaction(async ct =>
        {
            await _userIdentityStore.DeactivateAllActive(candidate.Id, _dateTimeProvider.Now, UserIdentityUnlinkReasons.TenantMigration, ct);

            await _userIdentityStore.Add(new UserIdentity
            {
                Id = Guid.NewGuid(),
                UserId = candidate.Id,
                Provider = LoginProviders.MicrosoftEntraId,
                ProviderTenantId = tenantId,
                ProviderSubject = objectId,
                IsActive = true,
                LinkedAt = _dateTimeProvider.Now,
            }, ct);

            candidate.PendingMigrationTenantId = null;
            await _userManager.UpdateAsync(candidate);
        });

        _logger.LogInformation(
            "Tenant migration completed for user {UserId}: rebound to tenant {TenantId} (subject {ObjectId}).",
            candidate.Id, tenantId, objectId);

        await _events.PublishAsync(new ApplicationUserUpdatedEvent(candidate.Id, _dateTimeProvider.Now));

        return candidate;
    }

    private async Task<ApplicationUser> CreateOrUpdateFromPrincipalAsync(ClaimsPrincipal principal, bool isFirstUser)
    {
        string principalObjectId = principal.GetObjectId() ?? throw new InternalServerException("Principal ObjectId is missing or null.");

        var adUser = await _graphServiceClient.Users[principalObjectId].GetAsync();
        string? email = principal.FindFirstValue(ClaimTypes.Upn) ?? adUser?.Mail;
        string? username = principal.GetDisplayName() ?? adUser?.GivenName;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username))
        {
            _logger.LogError("Username {Username} or Email {Email} not valid", username, email);
            throw new InternalServerException("Username or Email not valid.");
        }

        var user = await _userManager.FindByNameAsync(username);
        if (user is not null && await _userIdentityStore.ExistsActive(user.Id, LoginProviders.MicrosoftEntraId))
        {
            _logger.LogError("Username or Email {Username} not valid", username);
            throw new InternalServerException($"Username {username} is already taken.");
        }

        if (user is null)
        {
            user = await _userManager.FindByEmailAsync(email);
            if (user is not null && await _userIdentityStore.ExistsActive(user.Id, LoginProviders.MicrosoftEntraId))
            {
                _logger.LogError("Email {email} is already taken.", email);
                throw new InternalServerException($"Email {email} is already taken.");
            }
        }

        string principalTenantId = principal.GetTenantId() ?? throw new InternalServerException("Principal TenantId is missing or null.");

        IdentityResult? result;
        if (user is not null)
        {
            // Existing user — linking them to Entra. The UserIdentity row is inserted
            // downstream by EnsureEntraIdentityRowAsync, which also deactivates any
            // prior active identity (e.g., a Wayd local identity being migrated to SSO).
            result = await _userManager.UpdateAsync(user);

            await _events.PublishAsync(new ApplicationUserUpdatedEvent(user.Id, _dateTimeProvider.Now));
        }
        else
        {
            var employeeId = await GetEmployeeId(principalObjectId);

            if (!employeeId.HasValue && !isFirstUser)
            {
                _logger.LogWarning("Registration denied for user {Username} (ObjectId: {ObjectId}). No matching employee record found.",
                    username, principalObjectId);
                throw new ForbiddenException("Registration is restricted to users with an employee record in Wayd.");
            }

            user = new ApplicationUser
            {
                FirstName = principal.FindFirstValue(ClaimTypes.GivenName) ?? Guard.Against.NullOrWhiteSpace(adUser?.GivenName),
                LastName = principal.FindFirstValue(ClaimTypes.Surname) ?? Guard.Against.NullOrWhiteSpace(adUser?.Surname),
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                UserName = username,
                NormalizedUserName = username.ToUpperInvariant(),
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                IsActive = true,
                EmployeeId = employeeId,
                LoginProvider = LoginProviders.MicrosoftEntraId,
            };
            result = await _userManager.CreateAsync(user);

            await _events.PublishAsync(new ApplicationUserCreatedEvent(user.Id, _dateTimeProvider.Now));
        }

        if (!result.Succeeded)
        {
            _logger.LogError("Error creating user from principal: {Errors}", result.Errors.Select(e => e.Description));
            throw new InternalServerException("Validation Errors Occurred.");
        }

        await EnsureEntraIdentityRowAsync(user.Id, principalTenantId, principalObjectId);

        return user;
    }

    private async Task EnsureEntraIdentityRowAsync(string userId, string tenantId, string objectId)
    {
        // Idempotent for the new-user case, and enforces the "exactly one active
        // identity per user" invariant when an existing user is being linked to
        // Entra (e.g., a Wayd-local user moving to SSO). Any prior active row —
        // including a Wayd identity — is marked inactive with reason ProviderRelinked.
        var exists = await _userIdentityStore.ExistsActive(userId, LoginProviders.MicrosoftEntraId);
        if (exists)
        {
            return;
        }

        await _userIdentityStore.DeactivateAllActive(userId, _dateTimeProvider.Now, UserIdentityUnlinkReasons.ProviderRelinked);

        await _userIdentityStore.Add(new UserIdentity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Provider = LoginProviders.MicrosoftEntraId,
            ProviderTenantId = tenantId,
            ProviderSubject = objectId,
            IsActive = true,
            LinkedAt = _dateTimeProvider.Now,
        });
    }

    public async Task<Result<string>> CreateAsync(CreateUserCommand command, CancellationToken cancellationToken)
    {
        // Use email as the username for admin-created users
        var user = new ApplicationUser
        {
            FirstName = command.FirstName,
            LastName = command.LastName,
            Email = command.Email,
            NormalizedEmail = command.Email.ToUpperInvariant(),
            UserName = command.Email,
            NormalizedUserName = command.Email.ToUpperInvariant(),
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
            IsActive = true,
            LockoutEnabled = true,
            EmployeeId = command.EmployeeId,
            PhoneNumber = command.PhoneNumber,
            LoginProvider = command.LoginProvider,
            MustChangePassword = command.LoginProvider == LoginProviders.Wayd,
        };

        // User creation, role assignment, and the Wayd identity row must land
        // together. Partial failure — user exists with no active Wayd identity —
        // would leave a local user who cannot log in (TokenService gates on the
        // identity row). Run the whole thing in a transaction so any failure rolls
        // back the user.
        IdentityResult? result = null;
        try
        {
            await _userIdentityStore.ExecuteInTransaction(async ct =>
            {
                result = command.LoginProvider == LoginProviders.Wayd
                    ? await _userManager.CreateAsync(user, command.Password!)
                    : await _userManager.CreateAsync(user);

                if (!result.Succeeded)
                {
                    // Throw to force rollback. Outer catch swallows this sentinel;
                    // `result` carries the validation errors back to the caller.
                    throw new UserCreationRollbackException();
                }

                await _userManager.AddToRoleAsync(user, ApplicationRoles.Basic);

                // Wayd (local) users get an identity row immediately, keyed by the
                // stable ApplicationUser.Id (usernames are mutable). Entra users
                // created by an admin have no oid/tid yet — the row is inserted on
                // their first SSO login via EnsureEntraIdentityRowAsync.
                if (command.LoginProvider == LoginProviders.Wayd)
                {
                    await _userIdentityStore.Add(new UserIdentity
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        Provider = LoginProviders.Wayd,
                        ProviderTenantId = null,
                        ProviderSubject = user.Id,
                        IsActive = true,
                        LinkedAt = _dateTimeProvider.Now,
                    }, ct);
                }
            }, cancellationToken);
        }
        catch (UserCreationRollbackException)
        {
            // Expected when UserManager.CreateAsync returned a failing IdentityResult
            // inside the transaction. Fall through — the block below formats the
            // error response from `result`.
        }

        if (result is null || !result.Succeeded)
        {
            var errors = result is null
                ? "User creation failed."
                : string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Error creating user: {Errors}", errors);
            return Result.Failure<string>(errors);
        }

        // Publish after commit so subscribers don't see events for rolled-back users.
        await _events.PublishAsync(new ApplicationUserCreatedEvent(user.Id, _dateTimeProvider.Now));

        _logger.LogInformation("User {UserId} created successfully.", user.Id);
        return Result.Success(user.Id);
    }

    private sealed class UserCreationRollbackException : Exception { }

    public async Task UpdateAsync(UpdateUserCommand command, string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            _logger.LogError("User with id {UserId} not found.", userId);
            throw new NotFoundException("User Not Found.");
        }

        user.FirstName = command.FirstName;
        user.LastName = command.LastName;
        user.PhoneNumber = command.PhoneNumber;
        user.EmployeeId = command.EmployeeId;

        if (!string.Equals(user.Email, command.Email, StringComparison.OrdinalIgnoreCase))
        {
            user.Email = command.Email;
            user.NormalizedEmail = command.Email.ToUpperInvariant();
            user.UserName = command.Email;
            user.NormalizedUserName = command.Email.ToUpperInvariant();
        }

        string? phoneNumber = await _userManager.GetPhoneNumberAsync(user);
        if (command.PhoneNumber != phoneNumber)
            await _userManager.SetPhoneNumberAsync(user, command.PhoneNumber);

        var result = await _userManager.UpdateAsync(user);

        await _signInManager.RefreshSignInAsync(user);

        await _events.PublishAsync(new ApplicationUserUpdatedEvent(user.Id, _dateTimeProvider.Now));

        if (!result.Succeeded)
        {
            _logger.LogError("Error updating user: {Errors}", result.Errors.Select(e => e.Description));
            throw new InternalServerException("Update profile failed");
        }
    }

    public async Task<Result> ChangePasswordAsync(string userId, ChangePasswordCommand command)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            _logger.LogError("User with id {UserId} not found.", userId);
            throw new NotFoundException("User Not Found.");
        }

        if (user.LoginProvider != LoginProviders.Wayd)
        {
            return Result.Failure("Password change is only available for local accounts.");
        }

        var result = await _userManager.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Password change failed for user {UserId}: {Errors}", userId, errors);
            return Result.Failure(errors);
        }

        if (user.MustChangePassword)
        {
            user.MustChangePassword = false;
            await _userManager.UpdateAsync(user);
        }

        _logger.LogInformation("Password changed successfully for user {UserId}.", userId);
        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordCommand command)
    {
        var user = await _userManager.FindByIdAsync(command.UserId);
        if (user is null)
        {
            _logger.LogError("User with id {UserId} not found.", command.UserId);
            throw new NotFoundException("User Not Found.");
        }

        if (user.LoginProvider != LoginProviders.Wayd)
        {
            return Result.Failure("Password reset is only available for local accounts.");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, command.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Password reset failed for user {UserId}: {Errors}", command.UserId, errors);
            return Result.Failure(errors);
        }

        user.MustChangePassword = true;
        await _userManager.UpdateAsync(user);

        if (await _userManager.IsLockedOutAsync(user))
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.ResetAccessFailedCountAsync(user);
            _logger.LogInformation("Lockout cleared for user {UserId} during password reset.", command.UserId);
        }

        _logger.LogInformation("Password reset successfully for user {UserId}.", command.UserId);
        return Result.Success();
    }

    public async Task<Result> UnlockUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            _logger.LogError("User with id {UserId} not found.", userId);
            throw new NotFoundException("User Not Found.");
        }

        if (!await _userManager.IsLockedOutAsync(user))
        {
            return Result.Failure("User is not currently locked out.");
        }

        await _userManager.SetLockoutEndDateAsync(user, null);
        await _userManager.ResetAccessFailedCountAsync(user);

        _logger.LogInformation("User {UserId} unlocked by admin.", userId);
        return Result.Success();
    }

    public async Task<Result> UpdateMissingEmployeeIds(CancellationToken cancellationToken)
    {
        var users = await _userManager.Users.Where(u => !u.EmployeeId.HasValue).ToListAsync(cancellationToken);

        if (users.Count != 0)
        {
            var employees = await _sender.Send(new GetEmployeeNumberMapQuery(), cancellationToken);
            // External employee records are keyed by the Entra oid, which lives in
            // the user's active MicrosoftEntraId UserIdentity row.
            var entraSubjectsByUserId = await _userIdentityStore.GetActiveSubjectsByProvider(LoginProviders.MicrosoftEntraId, cancellationToken);
            // Pre-index by EmployeeNumber so per-user lookup is O(1) instead of scanning
            // the full employees list for every user. Last-wins on duplicate numbers —
            // matches the old FirstOrDefault behavior since duplicates would have been
            // unreachable anyway.
            var employeeIdByNumber = employees
                .GroupBy(e => e.EmployeeNumber)
                .ToDictionary(g => g.Key, g => g.Last().Id);

            foreach (var user in users)
            {
                if (!entraSubjectsByUserId.TryGetValue(user.Id, out var entraSubject))
                    continue;

                if (!employeeIdByNumber.TryGetValue(entraSubject, out var employeeId))
                    continue;

                user.EmployeeId = employeeId;
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    await _events.PublishAsync(new ApplicationUserUpdatedEvent(user.Id, _dateTimeProvider.Now));
                }
                else
                {
                    _logger.LogError("Error updating employeeId on user {UserId}: {Errors}", user.Id, result.Errors.Select(e => e.Description));
                }
            }
        }

        return Result.Success();
    }

    public async Task<Result> SyncUsersFromEmployeeRecords(List<IExternalEmployee> externalEmployees, CancellationToken cancellationToken)
    {
        // Only sync users who have an active Entra identity — that's what pairs them
        // with an external employee record (keyed by Entra oid). Filter server-side
        // via an EXISTS subquery against UserIdentities so we don't have to round-
        // trip user IDs in an IN (...) list, which hits the 2100-parameter SQL
        // Server limit past a few thousand users.
        var users = await _userManager.Users
            .Where(u => _db.UserIdentities.Any(ui =>
                ui.UserId == u.Id &&
                ui.IsActive &&
                ui.Provider == LoginProviders.MicrosoftEntraId))
            .ToListAsync(cancellationToken);

        _logger.LogDebug("{UserCount} users found with an active Entra identity.", users.Count);
        if (users.Count == 0)
            return Result.Success();

        // Second query — just the subject map for the users we actually need to
        // correlate. Still cheap because it's filtered by active + provider.
        var entraSubjectsByUserId = await _userIdentityStore.GetActiveSubjectsByProvider(LoginProviders.MicrosoftEntraId, cancellationToken);

        // Pre-index by EmployeeNumber so per-user correlation is O(1) instead of
        // scanning the full externalEmployees list for every user.
        var employeesByNumber = externalEmployees
            .GroupBy(e => e.EmployeeNumber)
            .ToDictionary(g => g.Key, g => g.Last());

        foreach (var user in users)
        {
            if (!entraSubjectsByUserId.TryGetValue(user.Id, out var entraSubject))
            {
                // Edge case: user's identity was deactivated between the two queries.
                continue;
            }

            var employee = employeesByNumber.GetValueOrDefault(entraSubject);
            if (employee is null)
            {
                _logger.LogWarning(
                    "No external employee record matched Entra subject {EntraSubject} for user {UserId}.",
                    entraSubject, user.Id);
                continue;
            }

            user.FirstName = employee.Name.FirstName;
            user.LastName = employee.Name.LastName;
            user.Email = employee.Email;
            user.IsActive = employee.IsActive;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("User {UserId} updated.", user.Id);
                await _events.PublishAsync(new ApplicationUserUpdatedEvent(user.Id, _dateTimeProvider.Now));
            }
            else
            {
                _logger.LogError("Error updating user {UserId}: {Errors}", user.Id, result.Errors.Select(e => e.Description));
            }
        }

        return Result.Success();
    }

    private async Task<Guid?> GetEmployeeId(string principalObjectId)
    {
        // get the Person Id and if not null verify no existing user with that Id
        var employeeId = await _sender.Send(new GetEmployeeByEmployeeNumberQuery(principalObjectId));
        if (employeeId.IsNullEmptyOrDefault())
        {
            employeeId = null;
            _logger.LogWarning("Employee with EmployeeNumber {EmployeeNumber} not found.", principalObjectId);
        }

        return employeeId;
    }
}
