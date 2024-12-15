using Moda.Common.Application.Exceptions;
using Moda.Web.Api.Extensions;

namespace Moda.Web.Api.Controllers.UserManagement;

[Route("api/user-management/roles")]
[ApiVersionNeutral]
[ApiController]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly ILogger<RolesController> _logger;

    public RolesController(IRoleService roleService, ILogger<RolesController> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Roles)]
    [OpenApiOperation("Get a list of all roles.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<RoleListDto>>> GetList(CancellationToken cancellationToken)
    {
        var roles = await _roleService.GetList(cancellationToken);

        return Ok(roles);
    }

    [HttpGet("{id}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Roles)]
    [OpenApiOperation("Get role details.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleDto>> GetById(string id, CancellationToken cancellationToken)
    {
        var role = await _roleService.GetById(id, cancellationToken);

        return role is not null
            ? Ok(role)
            : NotFound();
    }

    [HttpGet("{id}/permissions")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.RoleClaims)]
    [OpenApiOperation("Get role details with its permissions.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleDto>> GetByIdWithPermissions(string id, CancellationToken cancellationToken)
    {
        var role = await _roleService.GetByIdWithPermissions(id, cancellationToken);

        return role is not null
            ? Ok(role)
            : NotFound();
    }

    [HttpPut("{id}/permissions")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.RoleClaims)]
    [OpenApiOperation("Update a role's permissions.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdatePermissions(string id, UpdateRolePermissionsRequest request, CancellationToken cancellationToken)
    {
        if (id != request.RoleId)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(nameof(id), nameof(request.RoleId), HttpContext));

        var result = await _roleService.UpdatePermissions(request.ToUpdateRolePermissionsCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.Roles)]
    [OpenApiOperation("Create or update a role.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.CreateReturn201String))]
    public async Task<ActionResult<string>> CreateOrUpdate(CreateOrUpdateRoleRequest request)
    {
        var id = await _roleService.CreateOrUpdate(request.ToCreateOrUpdateRoleCommand());

        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpDelete("{id}")]
    [MustHavePermission(ApplicationAction.Delete, ApplicationResource.Roles)]
    [OpenApiOperation("Delete a role.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Delete(string id)
    {
        try
        {
            await _roleService.Delete(id);
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Error deleting role with id {id}", id);
            return BadRequest(ProblemDetailsExtensions.ForBadRequest(ex.Message, HttpContext));
        }

        return NoContent();
    }
}