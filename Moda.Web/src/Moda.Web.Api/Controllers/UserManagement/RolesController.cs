namespace Moda.Web.Api.Controllers.UserManagement;

[Route("api/user-management/roles")]
[ApiVersionNeutral]
[ApiController]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Roles)]
    [OpenApiOperation("Get a list of all roles.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<RoleListDto>>> GetList(CancellationToken cancellationToken)
    {
        var roles = await _roleService.GetListAsync(cancellationToken);

        return Ok(roles.OrderBy(r => r.Name));
    }

    [HttpGet("{id}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Roles)]
    [OpenApiOperation("Get role details.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleDto>> GetById(string id)
    {
        var role = await _roleService.GetByIdAsync(id);

        return role is not null
            ? Ok(role)
            : NotFound();
    }

    [HttpGet("{id}/permissions")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.RoleClaims)]
    [OpenApiOperation("Get role details with its permissions.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleDto>> GetByIdWithPermissions(string id, CancellationToken cancellationToken)
    {
        var role = await _roleService.GetByIdWithPermissionsAsync(id, cancellationToken);

        return role is not null
            ? Ok(role)
            : NotFound();
    }

    [HttpPut("{id}/permissions")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.RoleClaims)]
    [OpenApiOperation("Update a role's permissions.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<string>> UpdatePermissions(string id, UpdateRolePermissionsRequest request, CancellationToken cancellationToken)
    {
        return id != request.RoleId
            ? BadRequest()
            : Ok(await _roleService.UpdatePermissionsAsync(request.ToUpdateRolePermissionsCommand(), cancellationToken));
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.Roles)]
    [OpenApiOperation("Create or update a role.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.Create))]
    public async Task<ActionResult<string>> Create(CreateOrUpdateRoleRequest request)
    {
        return await _roleService.CreateOrUpdateAsync(request.ToCreateOrUpdateRoleCommand());
    }

    [HttpDelete("{id}")]
    [MustHavePermission(ApplicationAction.Delete, ApplicationResource.Roles)]
    [OpenApiOperation("Delete a role.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Delete(string id)
    {
        await _roleService.DeleteAsync(id);

        return NoContent();
    }
}