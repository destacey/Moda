using Moda.Common.Application.Models;
using Moda.ProjectPortfolioManagement.Application.StrategicInitiatives.Commands;
using Moda.ProjectPortfolioManagement.Application.StrategicInitiatives.Dtos;
using Moda.ProjectPortfolioManagement.Application.StrategicInitiatives.Queries;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.Web.Api.Extensions;
using Moda.Web.Api.Models.Ppm.StrategicInitiatives;

namespace Moda.Web.Api.Controllers.Ppm;

[Route("api/ppm/strategic-initiatives")]
[ApiVersionNeutral]
[ApiController]
public class StrategicInitiativesController(ILogger<StrategicInitiativesController> logger, ISender sender) : ControllerBase
{
    private readonly ILogger<StrategicInitiativesController> _logger = logger;
    private readonly ISender _sender = sender;

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.StrategicInitiatives)]
    [OpenApiOperation("Get a list of strategic initiatives.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<StrategicInitiativeListDto>>> GetStrategicInitiatives(CancellationToken cancellationToken, [FromQuery] int? status = null)
    {
        StrategicInitiativeStatus? filter = status.HasValue ? (StrategicInitiativeStatus)status.Value : null;

        var initiatives = await _sender.Send(new GetStrategicInitiativesQuery(filter), cancellationToken);

        return Ok(initiatives);
    }

    [HttpGet("{idOrKey}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.StrategicInitiatives)]
    [OpenApiOperation("Get strategic initiative details.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StrategicInitiativeDetailsDto>> GetStrategicInitiative(string idOrKey, CancellationToken cancellationToken)
    {
        var initiative = await _sender.Send(new GetStrategicInitiativeQuery(idOrKey), cancellationToken);

        return initiative is not null
            ? Ok(initiative)
            : NotFound();
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.StrategicInitiatives)]
    [OpenApiOperation("Create a strategic initiative.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.CreateReturn201IdAndKey))]
    public async Task<ActionResult<ObjectIdAndKey>> Create([FromBody] CreateStrategicInitiativeRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToCreateStrategicInitiativeCommand(), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetStrategicInitiative), new { idOrKey = result.Value.Id.ToString() }, result.Value)
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPut("{id}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.StrategicInitiatives)]
    [OpenApiOperation("Update a strategic initiative.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateStrategicInitiativeRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(HttpContext));

        var result = await _sender.Send(request.ToUpdateStrategicInitiativeCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPost("{id}/approve")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.StrategicInitiatives)]
    [OpenApiOperation("Approve a strategic initiative.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ApproveStrategicInitiativeCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPost("{id}/activate")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.StrategicInitiatives)]
    [OpenApiOperation("Activate a strategic initiative.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ActivateStrategicInitiativeCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPost("{id}/complete")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.StrategicInitiatives)]
    [OpenApiOperation("Complete a strategic initiative.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Complete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CompleteStrategicInitiativeCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPost("{id}/cancel")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.StrategicInitiatives)]
    [OpenApiOperation("Cancel a strategic initiative.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CancelStrategicInitiativeCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpDelete("{id}")]
    [MustHavePermission(ApplicationAction.Delete, ApplicationResource.StrategicInitiatives)]
    [OpenApiOperation("Delete a strategic initiative.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteStrategicInitiativeCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }
}
