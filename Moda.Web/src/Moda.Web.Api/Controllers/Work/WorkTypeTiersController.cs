using Moda.Work.Application.WorkTypeTiers.Dtos;
using Moda.Work.Application.WorkTypeTiers.Queries;

namespace Moda.Web.Api.Controllers.Work;

[Route("api/work/work-type-tiers")]
[ApiVersionNeutral]
[ApiController]
public class WorkTypeTiersController : ControllerBase
{
    private readonly ISender _sender;

    public WorkTypeTiersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.WorkTypeTiers)]
    [OpenApiOperation("Get a list of all work type tiers.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<WorkTypeTierDto>>> GetList(CancellationToken cancellationToken)
    {
        var tiers = await _sender.Send(new GetWorkTypeTiersQuery(), cancellationToken);
        return Ok(tiers.OrderBy(c => c.Order));
    }
}
