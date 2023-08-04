namespace Moda.Web.Api.Controllers.UserManagement;

[Route("api/user-management/permissions")]
[ApiVersionNeutral]
[ApiController]
public class PermissionsController: ControllerBase
{
    public PermissionsController()
    {
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Permissions)]
    [OpenApiOperation("Get a list of all permissions.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public ActionResult<IEnumerable<ApplicationPermission>> GetList()
    {
        var permissions = ApplicationPermissions.All;

        return Ok(permissions);
    }
}

