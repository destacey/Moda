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
    [OpenApiOperation(nameof(GetList), "Get a list of all roles.", "")]
    public async Task<IEnumerable<RoleListDto>> GetList(CancellationToken cancellationToken)
    {
        var roles = await _roleService.GetListAsync(cancellationToken);

        return roles.OrderBy(r => r.Name);
    }

    [HttpGet("{id}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Roles)]
    [OpenApiOperation(nameof(GetById), "Get role details.", "")]
    public async Task<RoleDto> GetById(string id)
    {
        return await _roleService.GetByIdAsync(id);
    }

    [HttpGet("{id}/permissions")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.RoleClaims)]
    [OpenApiOperation(nameof(GetByIdWithPermissions), "Get role details with its permissions.", "")]
    public async Task<RoleDto> GetByIdWithPermissions(string id, CancellationToken cancellationToken)
    {
        return await _roleService.GetByIdWithPermissionsAsync(id, cancellationToken);
    }

    [HttpPut("{id}/permissions")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.RoleClaims)]
    [OpenApiOperation(nameof(UpdatePermissions), "Update a role's permissions.", "")]
    public async Task<ActionResult<string>> UpdatePermissions(string id, UpdateRolePermissionsRequest request, CancellationToken cancellationToken)
    {
        return id != request.RoleId
            ? BadRequest()
            : Ok(await _roleService.UpdatePermissionsAsync(request.ToUpdateRolePermissionsCommand(), cancellationToken));
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.Roles)]
    [OpenApiOperation(nameof(Create), "Create or update a role.", "")]
    public async Task<ActionResult<string>> Create(CreateOrUpdateRoleRequest request)
    {
        return await _roleService.CreateOrUpdateAsync(request.ToCreateOrUpdateRoleCommand());
    }

    [HttpDelete("{id}")]
    [MustHavePermission(ApplicationAction.Delete, ApplicationResource.Roles)]
    [OpenApiOperation(nameof(Delete), "Delete a role.", "")]
    public async Task<string> Delete(string id)
    {
        return await _roleService.DeleteAsync(id);
    }
}