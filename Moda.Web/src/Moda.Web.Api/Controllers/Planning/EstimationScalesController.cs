using Moda.Planning.Application.EstimationScales.Commands;
using Moda.Planning.Application.EstimationScales.Dtos;
using Moda.Planning.Application.EstimationScales.Queries;
using Moda.Web.Api.Extensions;
using Moda.Web.Api.Models.Planning.EstimationScales;

namespace Moda.Web.Api.Controllers.Planning;

[Route("api/planning/estimation-scales")]
[ApiVersionNeutral]
[ApiController]
public class EstimationScalesController : ControllerBase
{
    private readonly ISender _sender;

    public EstimationScalesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.EstimationScales)]
    [OpenApiOperation("Get a list of estimation scales.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<EstimationScaleListDto>>> GetList(CancellationToken cancellationToken)
    {
        var scales = await _sender.Send(new GetEstimationScalesQuery(), cancellationToken);
        return Ok(scales);
    }

    [HttpGet("{id:int}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.EstimationScales)]
    [OpenApiOperation("Get estimation scale details.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EstimationScaleDetailsDto>> GetScale(int id, CancellationToken cancellationToken)
    {
        var scale = await _sender.Send(new GetEstimationScaleQuery(id), cancellationToken);
        return scale is not null
            ? Ok(scale)
            : NotFound();
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.EstimationScales)]
    [OpenApiOperation("Create a custom estimation scale.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.CreateReturn201Int))]
    public async Task<ActionResult<int>> Create([FromBody] CreateEstimationScaleRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToCreateEstimationScaleCommand(), cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetScale), new { id = result.Value }, result.Value)
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPut("{id:int}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.EstimationScales)]
    [OpenApiOperation("Update a custom estimation scale.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateEstimationScaleRequest request, CancellationToken cancellationToken)
    {
        if (id != request.EstimationScaleId)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(nameof(id), nameof(request.EstimationScaleId), HttpContext));

        var result = await _sender.Send(request.ToUpdateEstimationScaleCommand(), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPut("{id:int}/active-status")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.EstimationScales)]
    [OpenApiOperation("Set the active status of an estimation scale.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> SetActiveStatus(int id, [FromBody] SetEstimationScaleActiveStatusRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(nameof(id), nameof(request.Id), HttpContext));

        var result = await _sender.Send(new SetEstimationScaleActiveStatusCommand(id, request.IsActive), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpDelete("{id:int}")]
    [MustHavePermission(ApplicationAction.Delete, ApplicationResource.EstimationScales)]
    [OpenApiOperation("Delete a custom estimation scale.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteEstimationScaleCommand(id), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }
}
