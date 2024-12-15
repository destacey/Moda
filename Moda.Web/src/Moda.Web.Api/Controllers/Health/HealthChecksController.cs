using Moda.Health.Dtos;
using Moda.Health.Queries;
using Moda.Web.Api.Extensions;
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
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HealthCheckDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var healthCheck = await _sender.Send(new GetHealthCheckQuery(id), cancellationToken);

        return healthCheck is not null
            ? Ok(healthCheck)
            : NotFound();
    }

    [HttpGet("health-report/{objectId}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.HealthChecks)]
    [OpenApiOperation("Get the health report for a specific objectId.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<HealthCheckDto>>> GetHealthReport(Guid objectId, CancellationToken cancellationToken)
    {
        var healthChecks = await _sender.Send(new GetHealthReportQuery(objectId), cancellationToken);
        return healthChecks is not null
            ? Ok(healthChecks)
            : NotFound();
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.HealthChecks)]
    [OpenApiOperation("Create a health report.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.CreateReturn201Guid))]
    public async Task<ActionResult> Create([FromBody] CreateHealthCheckRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToCreateHealthCheckCommand(), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPut("{id}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.HealthChecks)]
    [OpenApiOperation("Update a health report.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<HealthCheckDto>> Update(Guid id, [FromBody] UpdateHealthCheckRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(HttpContext));

        var result = await _sender.Send(request.ToUpdateHealthCheckCommand(), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.ToBadRequestObject(HttpContext));

    }

    [HttpGet("statuses")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.HealthChecks)]
    [OpenApiOperation("Get a list of health check statuses.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<HealthStatusDto>>> GetStatuses(CancellationToken cancellationToken)
    {
        var items = await _sender.Send(new GetHealthStatusesQuery(), cancellationToken);
        return Ok(items.OrderBy(c => c.Order));
    }
}
