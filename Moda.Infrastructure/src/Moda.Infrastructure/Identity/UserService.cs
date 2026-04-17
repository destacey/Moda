using CSharpFunctionalExtensions;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace Wayd.Infrastructure.Identity;

internal partial class UserService(
    ILogger<UserService> logger,
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    ModaDbContext db,
    IEventPublisher events,
    GraphServiceClient graphServiceClient,
    ISender sender,
    IDateTimeProvider dateTimeProvider,
    ICurrentUser currentUser) : IUserService
{
    private readonly ILogger<UserService> _logger = logger;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly ModaDbContext _db = db;
    private readonly IEventPublisher _events = events;
    private readonly GraphServiceClient _graphServiceClient = graphServiceClient;
    private readonly ISender _sender = sender;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

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
}