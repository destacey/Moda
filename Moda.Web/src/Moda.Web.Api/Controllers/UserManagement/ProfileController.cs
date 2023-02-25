using System.Security.Claims;
using Moda.Web.Api.Models.Profiles;

namespace Moda.Web.Api.Controllers.UserManagement;

[Route("api/user-management/profiles")]
[ApiVersionNeutral]
[ApiController]
public class ProfileController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ISender _mediator;

    public ProfileController(IUserService userService, ISender mediator)
    {
        _userService = userService;
        _mediator = mediator;
    }

    [HttpGet]
    [OpenApiOperation("Get profile details of currently logged in user.", "")]
    public async Task<ActionResult<UserDetailsDto>> Get(CancellationToken cancellationToken)
    {
        return User.GetUserId() is not { } userId || string.IsNullOrEmpty(userId)
            ? Unauthorized()
            : Ok(await _userService.GetAsync(userId, cancellationToken));
    }

    [HttpPut]
    [OpenApiOperation("Update profile details of currently logged in user.", "")]
    public async Task<ActionResult<bool>> Update(UpdateProfileRequest request)
    {
        if (User.GetUserId() is not { } userId || string.IsNullOrEmpty(userId))
            return Unauthorized();

        await _userService.UpdateAsync(request.ToUpdateUserCommand(), userId);
        return Ok(true);
    }

    [HttpGet("permissions")]
    [OpenApiOperation("Get permissions of currently logged in user.", "")]
    public async Task<ActionResult<List<string>>> GetPermissions(CancellationToken cancellationToken)
    {
        return User.GetUserId() is not { } userId || string.IsNullOrEmpty(userId)
            ? Unauthorized()
            : Ok(await _userService.GetPermissionsAsync(userId, cancellationToken));
    }

    [HttpGet("logs")]
    [OpenApiOperation("Get audit logs of currently logged in user.", "")]
    public Task<List<AuditDto>> GetLogs()
    {
        return _mediator.Send(new GetMyAuditLogsQuery());
    }
}