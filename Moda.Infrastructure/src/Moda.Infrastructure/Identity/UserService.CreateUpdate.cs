using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
        string principalObjectId = principal.GetObjectId() ?? throw new InternalServerException(string.Format("Principal ObjectId is missing or null."));

        var adUser = await _graphServiceClient.Users[principalObjectId].Request().GetAsync();
        string? email = principal.FindFirstValue(ClaimTypes.Upn) ?? adUser.Mail;
        string? username = principal.GetDisplayName() ?? adUser.GivenName;
        
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username))
        {
            throw new InternalServerException(string.Format("Username or Email not valid."));
        }

        var user = await _userManager.FindByNameAsync(username);
        if (user is not null && !string.IsNullOrWhiteSpace(user.ObjectId))
        {
            throw new InternalServerException(string.Format("Username {0} is already taken.", username));
        }

        if (user is null)
        {
            user = await _userManager.FindByEmailAsync(email);
            if (user is not null && !string.IsNullOrWhiteSpace(user.ObjectId))
                throw new InternalServerException(string.Format("Email {0} is already taken.", email));
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

        return result.Succeeded 
            ? user
            : throw new InternalServerException("Validation Errors Occurred.");
    }

    public async Task UpdateAsync(UpdateUserCommand command, string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        _ = user ?? throw new NotFoundException("User Not Found.");

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
            throw new InternalServerException("Update profile failed");
    }

    private async Task<Guid> GetOrCreatePersonId(string principalObjectId)
    {
        // get the Person Id and if not null verify no existing user with that Id
        var personId = await _sender.Send(new GetPersonIdByKeyQuery(principalObjectId));
        if (personId.HasValue)
        {
            var existingUser = await _userManager.FindByIdAsync(personId.Value.ToString());

            return existingUser is not null
                ? throw new InternalServerException(string.Format("Person with key {0} already exists.", principalObjectId))
                : personId.Value;
        }

        // else, create new person
        var result = await _sender.Send(new CreatePersonCommand(principalObjectId!));

        return result.IsSuccess 
            ? result.Value 
            : throw new InternalServerException("Create Person failed");
    }
}
