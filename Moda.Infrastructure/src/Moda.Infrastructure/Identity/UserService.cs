using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Moda.Common.Application.Dtos;

namespace Moda.Infrastructure.Identity;

internal partial class UserService(
    ILogger<UserService> logger,
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    ModaDbContext db,
    IEventPublisher events,
    GraphServiceClient graphServiceClient,
    ISender sender,
    IDateTimeProvider dateTimeProvider) : IUserService
{
    private readonly ILogger<UserService> _logger = logger;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly ModaDbContext _db = db;
    private readonly IEventPublisher _events = events;
    private readonly GraphServiceClient _graphServiceClient = graphServiceClient;
    private readonly ISender _sender = sender;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

    public async Task<IReadOnlyList<UserDetailsDto>> SearchAsync(UserListFilter filter, CancellationToken cancellationToken)
    {
        var userDtos = await _db.Users
            .Where(u => u.IsActive == filter.IsActive)
            .Select(u => new UserDetailsDto
            {
                Id = u.Id,
                UserName = u.UserName,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                IsActive = u.IsActive,
                PhoneNumber = u.PhoneNumber,
                LastActivityAt = u.LastActivityAt,
                Employee = u.Employee == null ? null : NavigationDto.Create(u.Employee.Id, u.Employee.Key, u.Employee.Name.FullName),
                Roles = u.UserRoles
                    .Join(_db.Roles,
                        ur => ur.RoleId,
                        r => r.Id,
                        (ur, r) => new RoleListDto
                        {
                            Id = r.Id,
                            Name = r.Name!,
                            Description = r.Description
                        })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return userDtos;
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
        var userDtos = await _db.Users
            .Select(u => new UserDetailsDto
            {
                Id = u.Id,
                UserName = u.UserName,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                IsActive = u.IsActive,
                PhoneNumber = u.PhoneNumber,
                LastActivityAt = u.LastActivityAt,
                Employee = u.Employee == null ? null : NavigationDto.Create(u.Employee.Id, u.Employee.Key, u.Employee.Name.FullName),
                Roles = u.UserRoles
                    .Join(_db.Roles,
                        ur => ur.RoleId,
                        r => r.Id,
                        (ur, r) => new RoleListDto
                        {
                            Id = r.Id,
                            Name = r.Name!,
                            Description = r.Description
                        })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return userDtos;
    }

    public Task<int> GetCountAsync(CancellationToken cancellationToken) =>
        _userManager.Users.AsNoTracking().CountAsync(cancellationToken);

    public async Task<UserDetailsDto?> GetAsync(string userId, CancellationToken cancellationToken)
    {
        var userDto = await _db.Users
            .Where(u => u.Id == userId)
            .Select(u => new UserDetailsDto
            {
                Id = u.Id,
                UserName = u.UserName,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                IsActive = u.IsActive,
                PhoneNumber = u.PhoneNumber,
                LastActivityAt = u.LastActivityAt,
                Employee = u.Employee == null ? null : NavigationDto.Create(u.Employee.Id, u.Employee.Key, u.Employee.Name.FullName),
                Roles = u.UserRoles
                    .Join(_db.Roles,
                        ur => ur.RoleId,
                        r => r.Id,
                        (ur, r) => new RoleListDto
                        {
                            Id = r.Id,
                            Name = r.Name!,
                            Description = r.Description
                        })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        return userDto;
    }

    public async Task<List<UserDetailsDto>> GetUsersWithRole(string roleId, CancellationToken cancellationToken)
    {
        var userDtos = await _db.Users
            .Where(u => u.UserRoles.Any(ur => ur.RoleId == roleId))
            .Select(u => new UserDetailsDto
            {
                Id = u.Id,
                UserName = u.UserName,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                IsActive = u.IsActive,
                PhoneNumber = u.PhoneNumber,
                LastActivityAt = u.LastActivityAt,
                Employee = u.Employee == null ? null : NavigationDto.Create(u.Employee.Id, u.Employee.Key, u.Employee.Name.FullName),
                Roles = u.UserRoles
                    .Join(_db.Roles,
                        ur => ur.RoleId,
                        r => r.Id,
                        (ur, r) => new RoleListDto
                        {
                            Id = r.Id,
                            Name = r.Name!,
                            Description = r.Description
                        })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return userDtos;
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