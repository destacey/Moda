using Wayd.Organization.Application.TeamTypes.Dtos;
using Wayd.Organization.Application.TeamTypes.Queries;

namespace Wayd.Web.Api.Controllers.Work;

[Route("api/organization/team-types")]
[ApiVersionNeutral]
[ApiController]
public class TeamTypesController : ControllerBase
{
    private readonly ISender _sender;

    public TeamTypesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.WorkTypeTiers)]
    [OpenApiOperation("Get a list of all team types.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<TeamTypeDto>>> GetList(CancellationToken cancellationToken)
    {
        var categories = await _sender.Send(new GetTeamTypesQuery(), cancellationToken);
        return Ok(categories.OrderBy(c => c.Order));
    }
}
