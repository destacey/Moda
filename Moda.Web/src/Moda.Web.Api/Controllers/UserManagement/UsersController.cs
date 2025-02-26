using Moda.Web.Api.Extensions;
using Moda.Web.Api.Models.UserManagement.Users;

namespace Moda.Web.Api.Controllers.UserManagement;

[Route("api/user-management/users")]
[ApiVersionNeutral]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Users)]
    [OpenApiOperation("Get list of all users.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<List<UserDetailsDto>> GetList(CancellationToken cancellationToken)
    {
        return await _userService.GetListAsync(cancellationToken);
    }

    [HttpGet("{id}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Users)]
    [OpenApiOperation("Get a user's details.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDetailsDto>> GetById(string id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetAsync(id, cancellationToken);

        return user is null
            ? NotFound()
            : user;
    }

    [HttpGet("{id}/roles")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.UserRoles)]
    [OpenApiOperation("Get a user's roles.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<List<UserRoleDto>> GetRoles(string id, CancellationToken cancellationToken, [FromQuery] bool includeUnassigned = false)
    {
        return await _userService.GetRolesAsync(id, includeUnassigned, cancellationToken);
    }

    [HttpPost("{id}/roles")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.UserRoles)]
    [OpenApiOperation("Update a user's assigned roles.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> ManageRoles(string id, AssignUserRolesRequest request, CancellationToken cancellationToken)
    {
        if (id != request.UserId)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(nameof(id), nameof(request.UserId), HttpContext));
        
        var result = await _userService.AssignRolesAsync(request.ToAssignUserRolesRequest(), cancellationToken);

        return result.IsSuccess
            ? Ok()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPost("{id}/toggle-status")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Users)]
    [OpenApiOperation("Toggle a user's active status.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.Toggle))]
    public async Task<ActionResult> ToggleStatus(string id, ToggleUserStatusRequest request, CancellationToken cancellationToken)
    {
        if (id != request.UserId)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(nameof(id), nameof(request.UserId), HttpContext));

        await _userService.ToggleStatusAsync(request.ToToggleUserStatusCommand(), cancellationToken);
        return NoContent();
    }

    private string GetOriginFromRequest() => $"{Request.Scheme}://{Request.Host.Value}{Request.PathBase.Value}";
}