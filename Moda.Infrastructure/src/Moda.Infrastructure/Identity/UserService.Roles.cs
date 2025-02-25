using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Moda.Infrastructure.Identity;

internal partial class UserService
{
    public async Task<List<UserRoleDto>> GetRolesAsync(string userId, bool includeUnassigned, CancellationToken cancellationToken)
    {
        var userRoles = new List<UserRoleDto>();

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return [];

        var roles = await _roleManager.Roles.AsNoTracking().ToListAsync(cancellationToken);
        foreach (var role in roles)
        {
            userRoles.Add(new UserRoleDto
            {
                RoleId = role.Id,
                RoleName = role.Name,
                Description = role.Description,
                Enabled = await _userManager.IsInRoleAsync(user, role.Name!)
            });
        }

        return includeUnassigned ? userRoles : [.. userRoles.Where(r => r.Enabled)];
    }

    public async Task<Result> AssignRolesAsync(AssignUserRolesCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));

        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);

        _ = user ?? throw new NotFoundException("User Not Found.");

        var userCurrentRoles = await _userManager.GetRolesAsync(user);

        // REMOVE ROLES
        var rolesToRemove = userCurrentRoles.Except(command.RoleNames);

        if (rolesToRemove.Contains(ApplicationRoles.Admin))
        {
            var adminCount = (await _userManager.GetUsersInRoleAsync(ApplicationRoles.Admin)).Count;
            if (adminCount <= 1)
            {
                _logger.LogWarning("Moda should have at least 1 Admin.");
                throw new ConflictException("Moda should have at least 1 Admin.");
            }
        }

        var result = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
        if (!result.Succeeded)
        {
            _logger.LogError("Failed to remove roles from user.");
            throw new InternalServerException("Failed to remove roles from user.");
        }

        // ADD ROLES
        var rolesToAdd = command.RoleNames.Except(userCurrentRoles);

        result = await _userManager.AddToRolesAsync(user, rolesToAdd);
        if (!result.Succeeded)
        {
            _logger.LogError("Failed to add roles to user.");
            throw new InternalServerException("Failed to add roles to user.");
        }

        await _events.PublishAsync(new ApplicationUserUpdatedEvent(user.Id, _dateTimeProvider.Now, true));

        return Result.Success();
    }
}