﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Moda.Infrastructure.Persistence.Initialization;

internal class ApplicationDbSeeder
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly CustomSeederRunner _seederRunner;
    private readonly ILogger<ApplicationDbSeeder> _logger;

    public ApplicationDbSeeder(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, CustomSeederRunner seederRunner, ILogger<ApplicationDbSeeder> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _seederRunner = seederRunner;
        _logger = logger;
    }

    public async Task SeedDatabaseAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        await SeedRolesAsync(dbContext);
        //await SeedAdminUserAsync();
        await _seederRunner.RunSeedersAsync(cancellationToken);
    }

    private async Task SeedRolesAsync(ApplicationDbContext dbContext)
    {
        foreach (string roleName in ApplicationRoles.DefaultRoles)
        {
            if (await _roleManager.Roles.SingleOrDefaultAsync(r => r.Name == roleName)
                is not ApplicationRole role)
            {
                // Create the role
                _logger.LogInformation("Seeding {role} Role.", roleName);
                role = new ApplicationRole(roleName, $"{roleName} Role");
                await _roleManager.CreateAsync(role);
            }

            // Assign permissions
            if (roleName == ApplicationRoles.Basic)
            {
                await AssignPermissionsToRoleAsync(dbContext, ApplicationPermissions.Basic, role);
            }
            else if (roleName == ApplicationRoles.Admin)
            {
                await AssignPermissionsToRoleAsync(dbContext, ApplicationPermissions.Admin, role);
            }
        }
    }

    private async Task AssignPermissionsToRoleAsync(ApplicationDbContext dbContext, IReadOnlyList<ApplicationPermission> permissions, ApplicationRole role)
    {
        var currentClaims = await _roleManager.GetClaimsAsync(role);
        foreach (var permission in permissions)
        {
            if (!currentClaims.Any(c => c.Type == ApplicationClaims.Permission && c.Value == permission.Name))
            {
                _logger.LogInformation("Seeding {role} Permission '{permission}", role.Name, permission.Name);
                dbContext.RoleClaims.Add(new ApplicationRoleClaim
                {
                    RoleId = role.Id,
                    ClaimType = ApplicationClaims.Permission,
                    ClaimValue = permission.Name,
                    CreatedBy = "ApplicationDbSeeder"
                });
                await dbContext.SaveChangesAsync();
            }
        }
    }

    //private async Task SeedAdminUserAsync()
    //{
    //    if (await _userManager.Users.FirstOrDefaultAsync(u => u.Email == _currentTenant.AdminEmail)
    //        is not ApplicationUser adminUser)
    //    {
    //        string adminUserName = $"{_currentTenant.Id.Trim()}.{ApplicationRoles.Admin}".ToLowerInvariant();
    //        adminUser = new ApplicationUser
    //        {
    //            FirstName = _currentTenant.Id.Trim().ToLowerInvariant(),
    //            LastName = ApplicationRoles.Admin,
    //            Email = _currentTenant.AdminEmail,
    //            UserName = adminUserName,
    //            EmailConfirmed = true,
    //            PhoneNumberConfirmed = true,
    //            NormalizedEmail = _currentTenant.AdminEmail?.ToUpperInvariant(),
    //            NormalizedUserName = adminUserName.ToUpperInvariant(),
    //            IsActive = true
    //        };

    //        _logger.LogInformation("Seeding Default Admin User.");
    //        var password = new PasswordHasher<ApplicationUser>();
    //        adminUser.PasswordHash = password.HashPassword(adminUser, MultitenancyConstants.DefaultPassword);
    //        await _userManager.CreateAsync(adminUser);
    //    }

    //    // Assign role to user
    //    if (!await _userManager.IsInRoleAsync(adminUser, ApplicationRoles.Admin))
    //    {
    //        _logger.LogInformation("Assigning Admin Role to Admin User.");
    //        await _userManager.AddToRoleAsync(adminUser, ApplicationRoles.Admin);
    //    }
    //}
}