using System.Security.Claims;
using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Moda.Organization.Application.Employees.Queries;
using NotFoundException = Moda.Common.Application.Exceptions.NotFoundException;

namespace Moda.Infrastructure.Identity;

internal partial class UserService
{
    /// <summary>
    /// This is used when authenticating with AzureAd.
    /// The local user is retrieved using the objectidentifier claim present in the ClaimsPrincipal.
    /// If no such claim is found, an InternalServerException is thrown.
    /// If no user is found with that ObjectId, a new one is created and populated with the values from the ClaimsPrincipal.
    /// If a role claim is present in the principal, and the user is not yet in that roll, then the user is added to that role.
    /// </summary>
    public async Task<(string Id, string? EmployeeId)> GetOrCreateFromPrincipalAsync(ClaimsPrincipal principal)
    {
        string? objectId = principal.GetObjectId();
        if (string.IsNullOrWhiteSpace(objectId))
        {
            _logger.LogError("Invalid principal objectId");
            throw new InternalServerException("Invalid objectId");
        }

        var user = await _userManager.Users.Where(u => u.ObjectId == objectId).FirstOrDefaultAsync()
            ?? await CreateOrUpdateFromPrincipalAsync(principal);

        if (principal.FindFirstValue(ClaimTypes.Role) is string role &&
            await _roleManager.RoleExistsAsync(role) &&
            !await _userManager.IsInRoleAsync(user, role))
        {
            await _userManager.AddToRoleAsync(user, role);
            await _events.PublishAsync(new ApplicationUserUpdatedEvent(user.Id, _dateTimeService.Now, true));
        }

        return (user.Id, user.EmployeeId?.ToString());
    }

    private async Task<ApplicationUser> CreateOrUpdateFromPrincipalAsync(ClaimsPrincipal principal)
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

        IdentityResult? result;
        if (user is not null)
        {
            user.ObjectId = principal.GetObjectId();
            result = await _userManager.UpdateAsync(user);

            await _events.PublishAsync(new ApplicationUserUpdatedEvent(user.Id, _dateTimeService.Now));
        }
        else
        {
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
                EmployeeId = await GetEmployeeId(principalObjectId)
            };
            result = await _userManager.CreateAsync(user);

            await _events.PublishAsync(new ApplicationUserCreatedEvent(user.Id, _dateTimeService.Now));
        }

        if (!result.Succeeded)
        {
            _logger.LogError("Error creating user from principal: {Errors}", result.Errors.Select(e => e.Description));
            throw new InternalServerException("Validation Errors Occurred.");
        }

        return user;
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

        string? phoneNumber = await _userManager.GetPhoneNumberAsync(user);
        if (command.PhoneNumber != phoneNumber)
            await _userManager.SetPhoneNumberAsync(user, command.PhoneNumber);

        var result = await _userManager.UpdateAsync(user);

        await _signInManager.RefreshSignInAsync(user);

        await _events.PublishAsync(new ApplicationUserUpdatedEvent(user.Id, _dateTimeService.Now));

        if (!result.Succeeded)
        {
            _logger.LogError("Error updating user: {Errors}", result.Errors.Select(e => e.Description));
            throw new InternalServerException("Update profile failed");
        }
    }

    public async Task<Result> UpdateMissingEmployeeIds(CancellationToken cancellationToken)
    {
        var users = await _userManager.Users.Where(u => !u.EmployeeId.HasValue).ToListAsync(cancellationToken);

        if (users.Any())
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
                    await _events.PublishAsync(new ApplicationUserUpdatedEvent(user.Id, _dateTimeService.Now));
                }
                else
                {
                    _logger.LogError("Error updating employeeId on user {UserId}: {Errors}", user.Id, result.Errors.Select(e => e.Description));
                }
            }
        }

        return Result.Success();
    }

    private async Task<Guid?> GetEmployeeId(string principalObjectId)
    {
        // get the Person Id and if not null verify no existing user with that Id
        var employeeId = await _sender.Send(new GetEmployeeByEmployeeNumberQuery(principalObjectId));
        if (employeeId is null)
            _logger.LogWarning("Employee with EmployeeNumber {EmployeeNumber} not found.", principalObjectId);

        return employeeId;
    }
}
