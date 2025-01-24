using Moda.Common.Application.Models;
using Moda.StrategicManagement.Application.Visions.Commands;
using Moda.StrategicManagement.Application.Visions.Dtos;
using Moda.StrategicManagement.Application.Visions.Queries;
using Moda.StrategicManagement.Domain.Enums;
using Moda.Web.Api.Extensions;
using Moda.Web.Api.Models.StrategicManagement.Strategies;
using Moda.Web.Api.Models.StrategicManagement.Visions;

namespace Moda.Web.Api.Controllers.StrategicManagement;

[Route("api/strategic-management/[controller]")]
[ApiVersionNeutral]
[ApiController]
public class VisionsController : ControllerBase
{
    private readonly ILogger<VisionsController> _logger;
    private readonly ISender _sender;

    public VisionsController(ILogger<VisionsController> logger, ISender sender)
    {
        _logger = logger;
        _sender = sender;
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Visions)]
    [OpenApiOperation("Get a list of visions.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<VisionDto>>> GetVisions(CancellationToken cancellationToken, [FromQuery] int? state = null)
    {
        VisionState? filter = state.HasValue ? (VisionState)state.Value : null;

        var visions = await _sender.Send(new GetVisionsQuery(filter), cancellationToken);

        return Ok(visions);
    }

    [HttpGet("{idOrKey}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Visions)]
    [OpenApiOperation("Get vision details.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VisionDto>> GetVision(string idOrKey, CancellationToken cancellationToken)
    {
        var vision = await _sender.Send(new GetVisionQuery(idOrKey), cancellationToken);

        return vision is not null
            ? Ok(vision)
            : NotFound();
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.Visions)]
    [OpenApiOperation("Create a vision.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.CreateReturn201IdAndKey))]
    public async Task<ActionResult<ObjectIdAndKey>> Create([FromBody] CreateVisionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToCreateVisionCommand(), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetVision), new { idOrKey = result.Value.Id.ToString() }, result.Value)
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPut("{id}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Visions)]
    [OpenApiOperation("Update a vision.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateVisionRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(HttpContext));

        var result = await _sender.Send(request.ToUpdateVisionCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPost("{id}/activate")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Visions)]
    [OpenApiOperation("Activate a vision.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ActivateVisionCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPost("{id}/archive")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Visions)]
    [OpenApiOperation("Archive a vision.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Archive(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ArchiveVisionCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpDelete("{id}")]
    [MustHavePermission(ApplicationAction.Delete, ApplicationResource.Visions)]
    [OpenApiOperation("Delete a vision.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteVisionCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpGet("states")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Visions)]
    [OpenApiOperation("Get a list of all vision states.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<VisionStateDto>>> GetStateOptions(CancellationToken cancellationToken)
    {
        var items = await _sender.Send(new GetVisionStatesQuery(), cancellationToken);
        return Ok(items.OrderBy(s => s.Order));
    }
}
