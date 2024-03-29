using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Moda.Common.Application.BackgroundJobs;

namespace Moda.Infrastructure.Identity;

internal partial class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ModaDbContext _db;
    private readonly IJobService _jobService;
    private readonly IEventPublisher _events;
    private readonly GraphServiceClient _graphServiceClient;
    private readonly ISender _sender;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UserService(
        ILogger<UserService> logger,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ModaDbContext db,
        IJobService jobService,
        IEventPublisher events,
        GraphServiceClient graphServiceClient,
        ISender sender,
        IDateTimeProvider dateTimeProvider)
    {
        _logger = logger;
        _signInManager = signInManager;
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
        _jobService = jobService;
        _events = events;
        _graphServiceClient = graphServiceClient;
        _sender = sender;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<IReadOnlyList<UserDetailsDto>> SearchAsync(UserListFilter filter, CancellationToken cancellationToken)
    {
        var users = await _userManager.Users
            .Include(u => u.Employee)
            .Where(u => u.IsActive == filter.IsActive)
            .ProjectToType<UserDetailsDto>()
            .ToListAsync(cancellationToken);

        return users;
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

    public async Task<List<UserDetailsDto>> GetListAsync(CancellationToken cancellationToken) =>
        (await _userManager.Users
            .Include(u => u.Employee)
            .AsNoTracking()
            .ToListAsync(cancellationToken))
            .Adapt<List<UserDetailsDto>>();

    public Task<int> GetCountAsync(CancellationToken cancellationToken) =>
        _userManager.Users.AsNoTracking().CountAsync(cancellationToken);

    public async Task<UserDetailsDto?> GetAsync(string userId, CancellationToken cancellationToken)
    {
        return await _userManager.Users
            .Include(u => u.Employee)
            .AsNoTracking()
            .ProjectToType<UserDetailsDto>()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
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

    public async Task ToggleStatusAsync(ToggleUserStatusCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users.Where(u => u.Id == command.UserId).FirstOrDefaultAsync(cancellationToken);
        if (user is null)
        {
            _logger.LogError("UserId {UserId} not found", command.UserId);
            throw new NotFoundException("User Not Found.");
        }

        bool isAdmin = await _userManager.IsInRoleAsync(user, ApplicationRoles.Admin);
        if (isAdmin)
        {
            _logger.LogError("Administrators Profile's Status cannot be toggled");
            throw new ConflictException("Administrators Profile's Status cannot be toggled");
        }

        user.IsActive = command.ActivateUser;

        await _userManager.UpdateAsync(user);

        await _events.PublishAsync(new ApplicationUserUpdatedEvent(user.Id, _dateTimeProvider.Now));
    }
}