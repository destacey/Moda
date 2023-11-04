using Moda.Health.Dtos;
using Moda.Health.Queries;
using Moda.Web.Api.Models.Health;

namespace Moda.Web.Api.Controllers.Health;

[Route("api/[controller]")]
[ApiVersionNeutral]
[ApiController]
public class HealthChecksController : ControllerBase
{
    private readonly ILogger<HealthChecksController> _logger;
    private readonly ISender _sender;

    public HealthChecksController(ILogger<HealthChecksController> logger, ISender sender)
    {
        _logger = logger;
        _sender = sender;
    }

    [HttpGet("{id}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.HealthChecks)]
    [OpenApiOperation("Get a health check by id.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType(typeof(ErrorResult))]
    public async Task<ActionResult<HealthCheckDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var healthCheck = await _sender.Send(new GetHealthCheckQuery(id), cancellationToken);
        return healthCheck is not null
            ? Ok(healthCheck)
            : NotFound();
    }

    [HttpGet("health-report/{objectId}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.HealthChecks)]
    [OpenApiOperation("Get the healt report for a specific objectId.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<HealthReportDto>> GetHealthReport(Guid objectId, CancellationToken cancellationToken)
    {
        var healthReport = await _sender.Send(new GetHealthReportQuery(objectId), cancellationToken);
        return healthReport is not null
            ? Ok(healthReport)
            : NotFound();
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.HealthChecks)]
    [OpenApiOperation("Create a healt report.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.CreateReturn201Guid))]
    public async Task<ActionResult> Create([FromBody] CreateHealthCheckRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToCreateHealthCheckCommand(), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpPut("{id}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.HealthChecks)]
    [OpenApiOperation("Update a healt report.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<HealthCheckDto>> Update(Guid id, [FromBody] UpdateHealthCheckRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest();

        var result = await _sender.Send(request.ToUpdateHealthCheckCommand(), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error);
    }

    [HttpGet("contexts")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.HealthChecks)]
    [OpenApiOperation("Get a list of health check contexts.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<HealthCheckContextDto>>> GetContexts(CancellationToken cancellationToken)
    {
        var items = await _sender.Send(new GetHealthCheckContextsQuery(), cancellationToken);
        return Ok(items.OrderBy(c => c.Order));
    }

    [HttpGet("statuses")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.HealthChecks)]
    [OpenApiOperation("Get a list of health check statuses.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<HealthStatusDto>>> GetStatuses(CancellationToken cancellationToken)
    {
        var items = await _sender.Send(new GetHealthStatusesQuery(), cancellationToken);
        return Ok(items.OrderBy(c => c.Order));
    }
}
