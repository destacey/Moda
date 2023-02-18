using Moda.Work.Application.BacklogCategories.Dtos;
using Moda.Work.Application.BacklogCategories.Queries;

namespace Moda.Web.Api.Controllers.Work;

public class TeamTypesController : VersionNeutralApiController
{
    private readonly ISender _sender;

    public TeamTypesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.BacklogCategories)]
    [OpenApiOperation("Get a list of all team types.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TeamTypeDto>>> GetList(CancellationToken cancellationToken)
    {
        var categories = await _sender.Send(new GetTeamTypesQuery(), cancellationToken);
        return Ok(categories.OrderBy(c => c.Order));
    }
}
