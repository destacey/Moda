using FluentValidation.Results;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Moda.Infrastructure.Identity;

internal class RoleService : IRoleService
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ModaDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IEventPublisher _events;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RoleService(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        ModaDbContext db,
        ICurrentUser currentUser,
        IEventPublisher events,
        IDateTimeProvider dateTimeProvider)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _db = db;
        _currentUser = currentUser;
        _events = events;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<List<RoleListDto>> GetList(CancellationToken cancellationToken)
        => await _roleManager.Roles.ProjectToType<RoleListDto>().ToListAsync(cancellationToken);

    public async Task<int> GetCount(CancellationToken cancellationToken) =>
        await _roleManager.Roles.CountAsync(cancellationToken);

    public async Task<bool> Exists(string roleName, string? excludeId) =>
        await _roleManager.FindByNameAsync(roleName)
            is ApplicationRole existingRole
            && existingRole.Id != excludeId;

    public async Task<RoleDto?> GetById(string id, CancellationToken cancellationToken)
    {
        var role = await _db.Roles.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        return role?.Adapt<RoleDto>();
    }

    public async Task<RoleDto?> GetByIdWithPermissions(string roleId, CancellationToken cancellationToken)
    {
        var role = await GetById(roleId, cancellationToken);
        if (role == null)
        {
            return null;
        }

        role.Permissions = await _db.RoleClaims
            .Where(c => c.RoleId == roleId && c.ClaimType == ApplicationClaims.Permission)
            .Select(c => c.ClaimValue!)
            .ToListAsync(cancellationToken);

        return role;
    }

    public async Task<string> CreateOrUpdate(CreateOrUpdateRoleCommand request)
    {
        if (string.IsNullOrEmpty(request.Id))
        {
            // Create a new role.
            var role = new ApplicationRole(request.Name, request.Description);
            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                HandleValidationErrors(result);

                throw new InternalServerException("Register role failed");
            }

            await _events.PublishAsync(new ApplicationRoleCreatedEvent(role.Id, role.Name!, _dateTimeProvider.Now));

            return role.Id;
        }
        else
        {
            // Update an existing role.
            var role = await _roleManager.FindByIdAsync(request.Id);

            _ = role ?? throw new NotFoundException("Role Not Found");

            if (ApplicationRoles.IsDefault(role.Name!))
            {
                throw new ConflictException(string.Format("Not allowed to modify {0} Role.", role.Name));
            }

            role.Update(request.Name, request.Description);
            var result = await _roleManager.UpdateAsync(role);

            if (!result.Succeeded)
            {
                HandleValidationErrors(result);

                throw new InternalServerException("Update role failed");
            }

            await _events.PublishAsync(new ApplicationRoleUpdatedEvent(role.Id, role.Name!, _dateTimeProvider.Now));

            return role.Id;
        }
    }

    public async Task<string> UpdatePermissions(UpdateRolePermissionsCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(request.RoleId);
        _ = role ?? throw new NotFoundException("Role Not Found");
        if (role.Name == ApplicationRoles.Admin)
        {
            throw new ConflictException("Not allowed to modify Permissions for this Role.");
        }

        //if (_currentTenant.Id != MultitenancyConstants.Root.Id)
        //{
        //    // Remove Root Permissions if the Role is not created for Root Tenant.
        //    request.Permissions.RemoveAll(u => u.StartsWith("Permissions.Root."));
        //}

        var currentClaims = await _roleManager.GetClaimsAsync(role);

        // Remove permissions that were previously selected
        foreach (var claim in currentClaims.Where(c => !request.Permissions.Any(p => p == c.Value)))
        {
            var removeResult = await _roleManager.RemoveClaimAsync(role, claim);
            if (!removeResult.Succeeded)
            {
                throw new InternalServerException("Update permissions failed.");
            }
        }

        // Add all permissions that were not previously selected
        foreach (string permission in request.Permissions.Where(c => !currentClaims.Any(p => p.Value == c)))
        {
            if (!string.IsNullOrEmpty(permission))
            {
                _db.RoleClaims.Add(new ApplicationRoleClaim
                {
                    RoleId = role.Id,
                    ClaimType = ApplicationClaims.Permission,
                    ClaimValue = permission,
                    CreatedBy = _currentUser.GetUserId().ToString()
                });
                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        await _events.PublishAsync(new ApplicationRoleUpdatedEvent(role.Id, role.Name!, _dateTimeProvider.Now, true));

        return "Permissions Updated.";
    }

    public async Task<string> Delete(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);

        _ = role ?? throw new NotFoundException("Role Not Found");

        if (ApplicationRoles.IsDefault(role.Name!))
        {
            throw new ConflictException(string.Format("Not allowed to delete the {0} Role.", role.Name));
        }

        if ((await _userManager.GetUsersInRoleAsync(role.Name!)).Count > 0)
        {
            throw new ConflictException(string.Format("Not allowed to delete the {0} Role as it is being used.", role.Name));
        }

        await _roleManager.DeleteAsync(role);

        await _events.PublishAsync(new ApplicationRoleDeletedEvent(role.Id, role.Name!, _dateTimeProvider.Now));

        return string.Format("Role {0} Deleted.", role.Name);
    }

    /// <summary>Handles specific validation errors if they exist.</summary>
    /// <param name="result">The result.</param>
    /// <exception cref="Moda.Common.Application.Exceptions.ValidationException"></exception>
    private void HandleValidationErrors(IdentityResult result)
    {
        if (result.Errors.Any(e => e.Code == "DuplicateRoleName"))
        {
            var duplicateRoleName = result.Errors.First(e => e.Code == "DuplicateRoleName");
            var failures = new List<ValidationFailure>()
            {
                new ValidationFailure("Name", duplicateRoleName.Description)
            };

            throw new ValidationException(failures);
        }
    }
}