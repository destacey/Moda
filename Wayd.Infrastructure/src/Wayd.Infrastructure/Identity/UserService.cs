using CSharpFunctionalExtensions;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Wayd.Common.Application.Identity;

namespace Wayd.Infrastructure.Identity;

internal partial class UserService(
    ILogger<UserService> logger,
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    WaydDbContext db,
    IEventPublisher events,
    GraphServiceClient graphServiceClient,
    ISender sender,
    IDateTimeProvider dateTimeProvider,
    ICurrentUser currentUser,
    IUserIdentityStore userIdentityStore) : IUserService
{
    private readonly ILogger<UserService> _logger = logger;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly WaydDbContext _db = db;
    private readonly IEventPublisher _events = events;
    private readonly GraphServiceClient _graphServiceClient = graphServiceClient;
    private readonly ISender _sender = sender;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly IUserIdentityStore _userIdentityStore = userIdentityStore;

    public async Task<IReadOnlyList<UserDetailsDto>> SearchAsync(UserListFilter filter, CancellationToken cancellationToken)
    {
        return await _db.Users
            .Where(u => u.IsActive == filter.IsActive)
            .ProjectToType<UserDetailsDto>()
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsWithNameAsync(string name)
    {
        return await _userManager.FindByNameAsync(name) is not null;
    }

    public async Task<bool> ExistsWithEmailAsync(string email, string? exceptId = null)
    {
        return await _userManager.FindByEmailAsync(email.Normalize()) is ApplicationUser user && user.Id != exceptId;
    }

    public async Task<bool> ExistsWithPhoneNumberAsync(string phoneNumber, string? exceptId = null)
    {
        return await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber) is ApplicationUser user && user.Id != exceptId;
    }

    public async Task<List<UserDetailsDto>> GetListAsync(CancellationToken cancellationToken)
    {
        return await _db.Users
            .ProjectToType<UserDetailsDto>()
            .ToListAsync(cancellationToken);
    }

    public Task<int> GetCountAsync(CancellationToken cancellationToken) =>
        _userManager.Users.AsNoTracking().CountAsync(cancellationToken);

    public async Task<UserDetailsDto?> GetAsync(string userId, CancellationToken cancellationToken)
    {
        return await _db.Users
            .Where(u => u.Id == userId)
            .ProjectToType<UserDetailsDto>()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<UserDetailsDto>> GetUsersWithRole(string roleId, CancellationToken cancellationToken)
    {
        return await _db.Users
            .Where(u => u.UserRoles.Any(ur => ur.RoleId == roleId))
            .ProjectToType<UserDetailsDto>()
            .ToListAsync(cancellationToken);
    }

    public Task<int> GetUsersWithRoleCount(string roleId, CancellationToken cancellationToken)
    {
        return _db.Users
            .Where(u => u.UserRoles.Any(ur => ur.RoleId == roleId))
            .CountAsync(cancellationToken);
    }

    public async Task<string?> GetEmailAsync(string userId)
    {
        var user = await _userManager.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
        {
            _logger.LogError("UserId {UserId} not found", userId);
            throw new NotFoundException("User Not Found.");
        }

        return user.Email;
    }

    public async Task<Result> ActivateUserAsync(ActivateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users.Where(u => u.Id == command.UserId).FirstOrDefaultAsync(cancellationToken);
        if (user is null)
            throw new NotFoundException("User Not Found.");

        if (user.IsActive)
            return Result.Failure("User is already active.");

        user.IsActive = true;
        await _userManager.UpdateAsync(user);
        await _events.PublishAsync(new ApplicationUserActivatedEvent(user.Id, _dateTimeProvider.Now));

        _logger.LogInformation("User {UserId} activated.", command.UserId);
        return Result.Success();
    }

    public async Task<Result> DeactivateUserAsync(DeactivateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users.Where(u => u.Id == command.UserId).FirstOrDefaultAsync(cancellationToken);
        if (user is null)
            throw new NotFoundException("User Not Found.");

        if (!user.IsActive)
            return Result.Failure("User is already inactive.");

        if (command.UserId == _currentUser.GetUserId())
            return Result.Failure("You cannot deactivate your own account.");

        user.IsActive = false;
        await _userManager.UpdateAsync(user);
        await _events.PublishAsync(new ApplicationUserDeactivatedEvent(user.Id, _dateTimeProvider.Now));

        _logger.LogInformation("User {UserId} deactivated.", command.UserId);
        return Result.Success();
    }

    public async Task<Result> StageTenantMigration(StageTenantMigrationCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);
        if (user is null)
            throw new NotFoundException("User Not Found.");

        if (user.LoginProvider != LoginProviders.MicrosoftEntraId)
            return Result.Failure("Tenant migration is only available for Microsoft Entra ID users.");

        if (!await _userIdentityStore.ExistsActive(user.Id, LoginProviders.MicrosoftEntraId, cancellationToken))
            return Result.Failure("User has no active Microsoft Entra ID identity to migrate.");

        // Last-write-wins. Re-staging silently overwrites the previous target — admin's
        // most recent decision is the one we honor, matching the spec's stated semantics.
        user.PendingMigrationTenantId = command.TargetTenantId;
        await _userManager.UpdateAsync(user);

        await _events.PublishAsync(new ApplicationUserUpdatedEvent(user.Id, _dateTimeProvider.Now));

        _logger.LogInformation(
            "Tenant migration staged for user {UserId}: target tenant {TenantId}.",
            user.Id, command.TargetTenantId);
        return Result.Success();
    }

    public async Task<Result> CancelTenantMigration(string userId, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
            throw new NotFoundException("User Not Found.");

        if (user.PendingMigrationTenantId is null)
        {
            // Idempotent: clearing a flag that's already clear is a no-op. Returning
            // success keeps the admin UI simple — they can click Cancel without
            // worrying about whether the migration just completed in another tab.
            return Result.Success();
        }

        user.PendingMigrationTenantId = null;
        await _userManager.UpdateAsync(user);

        await _events.PublishAsync(new ApplicationUserUpdatedEvent(user.Id, _dateTimeProvider.Now));

        _logger.LogInformation("Tenant migration canceled for user {UserId}.", user.Id);
        return Result.Success();
    }

    public async Task<List<UserIdentityDto>> GetIdentityHistory(string userId, CancellationToken cancellationToken)
    {
        var userExists = await _userManager.Users.AnyAsync(u => u.Id == userId, cancellationToken);
        if (!userExists)
            throw new NotFoundException("User Not Found.");

        return await _db.UserIdentities
            .AsNoTracking()
            .Where(ui => ui.UserId == userId)
            .OrderByDescending(ui => ui.IsActive)
            .ThenByDescending(ui => ui.LinkedAt)
            .ProjectToType<UserIdentityDto>()
            .ToListAsync(cancellationToken);
    }
}