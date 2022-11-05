namespace Moda.Web.Api.Controllers.Identity;

public class RolesController : VersionNeutralApiController
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    { 
        _roleService = roleService; 
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Roles)]
    [OpenApiOperation("Get a list of all roles.", "")]
    public async Task<List<RoleDto>> GetList(CancellationToken cancellationToken)
    {
        return await _roleService.GetListAsync(cancellationToken);
    }

    [HttpGet("{id}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Roles)]
    [OpenApiOperation("Get role details.", "")]
    public async Task<RoleDto> GetById(string id)
    {
        return await _roleService.GetByIdAsync(id);
    }

    [HttpGet("{id}/permissions")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.RoleClaims)]
    [OpenApiOperation("Get role details with its permissions.", "")]
    public async Task<RoleDto> GetByIdWithPermissions(string id, CancellationToken cancellationToken)
    {
        return await _roleService.GetByIdWithPermissionsAsync(id, cancellationToken);
    }

    [HttpPut("{id}/permissions")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.RoleClaims)]
    [OpenApiOperation("Update a role's permissions.", "")]
    public async Task<ActionResult<string>> UpdatePermissions(string id, UpdateRolePermissionsRequest request, CancellationToken cancellationToken)
    {
        return id != request.RoleId 
            ? (ActionResult<string>)BadRequest() 
            : Ok(await _roleService.UpdatePermissionsAsync(request.ToUpdateRolePermissionsCommand(), cancellationToken));
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.Roles)]
    [OpenApiOperation("Create or update a role.", "")]
    public async Task<ActionResult<string>> RegisterRole(CreateOrUpdateRoleRequest request)
    {
        return await _roleService.CreateOrUpdateAsync(request.ToCreateOrUpdateRoleCommand());
    }

    [HttpDelete("{id}")]
    [MustHavePermission(ApplicationAction.Delete, ApplicationResource.Roles)]
    [OpenApiOperation("Delete a role.", "")]
    public async Task<string> Delete(string id)
    {
        return await _roleService.DeleteAsync(id);
    }
}