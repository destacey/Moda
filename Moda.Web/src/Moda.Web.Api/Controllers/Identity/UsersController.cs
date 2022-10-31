namespace Moda.Web.Api.Controllers.Identity;

public class UsersController : VersionNeutralApiController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Users)]
    [OpenApiOperation("Get list of all users.", "")]
    public async Task<List<UserDetailsDto>> GetList(CancellationToken cancellationToken)
    {
        return await _userService.GetListAsync(cancellationToken);
    }

    [HttpGet("{id}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Users)]
    [OpenApiOperation("Get a user's details.", "")]
    public async Task<UserDetailsDto> GetById(string id, CancellationToken cancellationToken)
    {
        return await _userService.GetAsync(id, cancellationToken);
    }

    [HttpGet("{id}/roles")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.UserRoles)]
    [OpenApiOperation("Get a user's roles.", "")]
    public async Task<List<UserRoleDto>> GetRoles(string id, CancellationToken cancellationToken)
    {
        return await _userService.GetRolesAsync(id, cancellationToken);
    }

    [HttpPost("{id}/roles")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.Register))]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.UserRoles)]
    [OpenApiOperation("Update a user's assigned roles.", "")]
    public async Task<string> AssignRoles(string id, UserRolesRequest request, CancellationToken cancellationToken)
    {
        return await _userService.AssignRolesAsync(id, request, cancellationToken);
    }

    [HttpPost("{id}/toggle-status")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Users)]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.Register))]
    [OpenApiOperation("Toggle a user's active status.", "")]
    public async Task<ActionResult> ToggleStatus(string id, ToggleUserStatusRequest request, CancellationToken cancellationToken)
    {
        if (id != request.UserId)
        {
            return BadRequest();
        }

        await _userService.ToggleStatusAsync(request, cancellationToken);
        return Ok();
    }

    private string GetOriginFromRequest() => $"{Request.Scheme}://{Request.Host.Value}{Request.PathBase.Value}";
}