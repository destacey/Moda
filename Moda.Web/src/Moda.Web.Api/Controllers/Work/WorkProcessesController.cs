using Moda.Work.Application.WorkProcesses.Dtos;
using Moda.Work.Application.WorkProcesses.Queries;

namespace Moda.Web.Api.Controllers.Work;

[Route("api/work/work-processes")]
[ApiVersionNeutral]
[ApiController]
public class WorkProcessesController(ILogger<WorkProcessesController> logger, ISender sender) : ControllerBase
{
    private readonly ILogger<WorkProcessesController> _logger = logger;
    private readonly ISender _sender = sender;

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.WorkProcesses)]
    [OpenApiOperation("Get a list of all work processes.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<WorkProcessListDto>>> GetList(CancellationToken cancellationToken, bool includeInactive = false)
    {
        var workTypes = await _sender.Send(new GetWorkProcessesQuery(includeInactive), cancellationToken);
        return Ok(workTypes.OrderBy(s => s.Name));
    }
}
