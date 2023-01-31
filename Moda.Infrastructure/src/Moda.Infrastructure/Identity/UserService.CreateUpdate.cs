using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;

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
    public async Task<string> GetOrCreateFromPrincipalAsync(ClaimsPrincipal principal)
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

        return user.Id;
    }

    private async Task<ApplicationUser> CreateOrUpdateFromPrincipalAsync(ClaimsPrincipal principal)
    {
        string principalObjectId = principal.GetObjectId() ?? throw new InternalServerException("Principal ObjectId is missing or null.");

        var adUser = await _graphServiceClient.Users[principalObjectId].Request().GetAsync();
        string? email = principal.FindFirstValue(ClaimTypes.Upn) ?? adUser.Mail;
        string? username = principal.GetDisplayName() ?? adUser.GivenName;
        
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
            Guid newUserId = await GetOrCreatePersonId(principalObjectId);
            user = new ApplicationUser
            {
                Id = newUserId.ToString(),
                ObjectId = principalObjectId,
                FirstName = principal.FindFirstValue(ClaimTypes.GivenName) ?? adUser.GivenName,
                LastName = principal.FindFirstValue(ClaimTypes.Surname) ?? adUser.Surname,
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                UserName = username,
                NormalizedUserName = username.ToUpperInvariant(),
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                IsActive = true
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

    private async Task<Guid> GetOrCreatePersonId(string principalObjectId)
    {
        // get the Person Id and if not null verify no existing user with that Id
        var personId = await _sender.Send(new GetPersonIdByKeyQuery(principalObjectId));
        if (personId.HasValue)
        {
            var existingUser = await _userManager.FindByIdAsync(personId.Value.ToString());
            if (existingUser is not null)
            {
                _logger.LogError("Person with key {PrincipalObjectId} already exists.", principalObjectId);
                throw new InternalServerException($"Person with key {principalObjectId} already exists.");
            }

            return personId.Value;
        }

        // else, create new person
        var result = await _sender.Send(new CreatePersonCommand(principalObjectId!));
        if (result.IsFailure)
        {
            _logger.LogError("Create Person failed: {Error}", result.Error);
            throw new InternalServerException("Create Person failed");
        }

        return result.Value;
    }
}
