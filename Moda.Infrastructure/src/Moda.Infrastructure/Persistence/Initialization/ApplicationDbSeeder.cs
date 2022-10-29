using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Moda.Infrastructure.Persistence.Initialization;

internal class ApplicationDbSeeder
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly CustomSeederRunner _seederRunner;
    private readonly ILogger<ApplicationDbSeeder> _logger;

    public ApplicationDbSeeder(RoleManager<ApplicationRole> roleManager, CustomSeederRunner seederRunner, ILogger<ApplicationDbSeeder> logger)
    {
        _roleManager = roleManager;
        _seederRunner = seederRunner;
        _logger = logger;
    }

    public async Task SeedDatabase(ModaDbContext dbContext, CancellationToken cancellationToken)
    {
        await SeedRoles(dbContext);
        await _seederRunner.RunSeeders(cancellationToken);
    }

    private async Task SeedRoles(ModaDbContext dbContext)
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
                await AssignPermissionsToRole(dbContext, ApplicationPermissions.Basic, role);
            }
            else if (roleName == ApplicationRoles.Admin)
            {
                await AssignPermissionsToRole(dbContext, ApplicationPermissions.Admin, role);
            }
        }
    }

    private async Task AssignPermissionsToRole(ModaDbContext dbContext, IReadOnlyList<ApplicationPermission> permissions, ApplicationRole role)
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
}