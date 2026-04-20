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

        var isFirstUser = !await _userManager.Users.AnyAsync();

        var user = await ResolveUserByEntraIdentityAsync(tenantId, objectId)
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

    private async Task<ApplicationUser?> ResolveUserByEntraIdentityAsync(string tenantId, string objectId)
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

        if (pending.Count == 0)
        {
            return null;
        }

        if (pending.Count > 1)
        {
            _logger.LogError(
                "Ambiguous tenant upgrade for Entra subject {ObjectId}: {Count} active rows with NULL tenant. Manual resolution required.",
                objectId, pending.Count);
            throw new InternalServerException("Identity resolution ambiguous. Contact an administrator.");
        }

        var row = pending[0];
        row.ProviderTenantId = tenantId;
        await _userIdentityStore.Update(row);
        return row.User;
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
        if (user is not null && !string.IsNullOrWhiteSpace(user.ObjectId))
        {
            _logger.LogError("Username or Email {Username} not valid", username);
            throw new InternalServerException($"Username {username} is already taken.");
        }

        if (user is null)
        {
            user = await _userManager.FindByEmailAsync(email);
            if (user is not null && !string.IsNullOrWhiteSpace(user.ObjectId))
            {
                _logger.LogError("Email {email} is already taken.", email);
                throw new InternalServerException($"Email {email} is already taken.");
            }
        }

        string principalTenantId = principal.GetTenantId() ?? throw new InternalServerException("Principal TenantId is missing or null.");

        IdentityResult? result;
        if (user is not null)
        {
            user.ObjectId = principal.GetObjectId();
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
                ObjectId = principalObjectId,
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
        // Idempotent: inserts an active Entra UserIdentity row if one doesn't already
        // exist for this user. Covers both new-user creation and the "existing local
        // user is being linked to Entra" path.
        var exists = await _userIdentityStore.ExistsActive(userId, LoginProviders.MicrosoftEntraId);
        if (exists)
        {
            return;
        }

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

        IdentityResult result = command.LoginProvider == LoginProviders.Wayd
            ? await _userManager.CreateAsync(user, command.Password!)
            : await _userManager.CreateAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Error creating user: {Errors}", errors);
            return Result.Failure<string>(errors);
        }

        await _userManager.AddToRoleAsync(user, ApplicationRoles.Basic);
        await _events.PublishAsync(new ApplicationUserCreatedEvent(user.Id, _dateTimeProvider.Now));

        // Wayd (local) users get an identity row immediately, keyed by the stable
        // ApplicationUser.Id (usernames are mutable). Entra users created by an admin
        // have no oid/tid yet — the row is inserted on their first SSO login via
        // EnsureEntraIdentityRowAsync.
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
            }, cancellationToken);
        }

        _logger.LogInformation("User {UserId} created successfully.", user.Id);
        return Result.Success(user.Id);
    }

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
            foreach (var user in users)
            {
                var employeeId = employees.Where(e => e.EmployeeNumber == user.ObjectId).Select(e => (Guid?)e.Id ?? null).FirstOrDefault();
                if (!employeeId.HasValue)
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
        var users = await _userManager.Users.Where(u => u.ObjectId != null).ToListAsync(cancellationToken);

        _logger.LogDebug("{UserCount} users found with EmployeeId.", users.Count);
        if (users.Count == 0)
            return Result.Success();

        foreach (var user in users)
        {
            var employee = externalEmployees.FirstOrDefault(e => e.EmployeeNumber == user.ObjectId);
            if (employee is null)
            {
                _logger.LogWarning("Employee with Id {EmployeeId} not found.", user.EmployeeId);
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
