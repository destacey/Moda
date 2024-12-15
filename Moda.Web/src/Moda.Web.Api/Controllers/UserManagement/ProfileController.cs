using System.Security.Claims;
using Moda.Common.Application.Interfaces;
using Moda.Web.Api.Models.UserManagement.Profiles;

namespace Moda.Web.Api.Controllers.UserManagement;

[Route("api/user-management/profiles")]
[ApiVersionNeutral]
[ApiController]
public class ProfileController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ISender _sender;
    private readonly ICurrentUser _currentUser;

    public ProfileController(IUserService userService, ISender sender, ICurrentUser currentUser)
    {
        _userService = userService;
        _sender = sender;
        _currentUser = currentUser;
    }

    [HttpGet]
    [OpenApiOperation("Get profile details of currently logged in user.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDetailsDto>> Get(CancellationToken cancellationToken)
    {
        return User.GetUserId() is not { } userId || string.IsNullOrEmpty(userId)
            ? Unauthorized()
            : Ok(await _userService.GetAsync(userId, cancellationToken));
    }

    [HttpPut]
    [OpenApiOperation("Update profile details of currently logged in user.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Update(UpdateProfileRequest request)
    {
        if (User.GetUserId() is not { } userId || string.IsNullOrEmpty(userId))
            return Unauthorized();

        await _userService.UpdateAsync(request.ToUpdateUserCommand(), userId);
        return NoContent();
    }

    [HttpGet("permissions")]
    [OpenApiOperation("Get permissions of currently logged in user.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<string>>> GetPermissions(CancellationToken cancellationToken)
    {
        return User.GetUserId() is not { } userId || string.IsNullOrEmpty(userId)
            ? Unauthorized()
            : Ok(await _userService.GetPermissionsAsync(userId, cancellationToken));
    }

    [HttpGet("internal-employee-id")]
    [OpenApiOperation("Get internal employee id of currently logged in user.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<ActionResult<string>> GetInternalEmployeeId()
    {
        // TODO: move this to a claim
        return Task.FromResult<ActionResult<string>>(Ok(_currentUser.GetEmployeeId()));
    }

    [HttpGet("logs")]
    [OpenApiOperation("Get audit logs of currently logged in user.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<List<AuditDto>> GetLogs(CancellationToken cancellationToken)
    {
        return _sender.Send(new GetMyAuditLogsQuery(), cancellationToken);
    }
}