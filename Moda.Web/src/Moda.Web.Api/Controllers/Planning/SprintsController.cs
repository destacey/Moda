using Moda.Planning.Application.Iterations.Dtos;
using Moda.Planning.Application.Iterations.Queries;
using Moda.Work.Application.WorkItems.Dtos;
using Moda.Work.Application.WorkItems.Queries;

namespace Moda.Web.Api.Controllers.Planning;

[Route("api/planning/[controller]")]
[ApiVersionNeutral]
[ApiController]
public class SprintsController(ILogger<SprintsController> logger, ISender sender) : ControllerBase
{
    private readonly ILogger<SprintsController> _logger = logger;
    private readonly ISender _sender = sender;

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Iterations)]
    [OpenApiOperation("Get a list of sprints.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<SprintListDto>>> GetSprints([FromQuery] Guid? teamId, CancellationToken cancellationToken)
    {
        var sprints = await _sender.Send(new GetSprintsQuery(teamId), cancellationToken);

        return Ok(sprints);
    }

    [HttpGet("{idOrKey}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Iterations)]
    [OpenApiOperation("Get sprint details.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SprintDetailsDto>> GetSprint(string idOrKey, CancellationToken cancellationToken)
    {
        var sprint = await _sender.Send(new GetSprintQuery(idOrKey), cancellationToken);

        return sprint is not null
            ? Ok(sprint)
            : NotFound();
    }

    // get backlog
    [HttpGet("{idOrKey}/backlog")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Iterations)]
    [OpenApiOperation("Get sprint backlog items.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<SprintBacklogItemDto>>> GetSprintBacklog(string idOrKey, CancellationToken cancellationToken)
    {
        var backlogItems = await _sender.Send(new GetSprintBacklogQuery(idOrKey), cancellationToken);

        return backlogItems is not null
            ? Ok(backlogItems)
            : NotFound();
    }
}
