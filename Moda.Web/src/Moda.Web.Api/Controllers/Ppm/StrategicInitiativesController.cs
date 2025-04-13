using Moda.Common.Application.Models;
using Moda.ProjectPortfolioManagement.Application.Projects.Dtos;
using Moda.ProjectPortfolioManagement.Application.StrategicInitiatives.Commands;
using Moda.ProjectPortfolioManagement.Application.StrategicInitiatives.Commands.Kpis;
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
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
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

    #region KPIs

    [HttpGet("{id}/kpis")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.StrategicInitiatives)]
    [OpenApiOperation("Get a list of KPIs for a strategic initiative.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<StrategicInitiativeKpiListDto>>> GetKpis(string id, CancellationToken cancellationToken)
    {
        var kpis = await _sender.Send(new GetStrategicInitiativeKpisQuery(id), cancellationToken);

        // TODO: does this return null if the strategic initiative is not found?
        return kpis is not null
            ? Ok(kpis)
            : NotFound();
    }

    [HttpGet("{id}/kpis/{kpiId}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.StrategicInitiatives)]
    [OpenApiOperation("Get a KPI for a strategic initiative.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StrategicInitiativeKpiDetailsDto>> GetKpi(string id, string kpiId, CancellationToken cancellationToken)
    {
        var kpi = await _sender.Send(new GetStrategicInitiativeKpiQuery(id, kpiId), cancellationToken);

        return kpi is not null
            ? Ok(kpi)
            : NotFound();
    }

    [HttpPost("{id}/kpis")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.StrategicInitiatives)]
    [OpenApiOperation("Create a KPI for a strategic initiative.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.CreateReturn201IdAndKey))]
    public async Task<ActionResult<ObjectIdAndKey>> CreateKpi(Guid id, [FromBody] CreateStrategicInitiativeKpiRequest request, CancellationToken cancellationToken)
    {
        if (id != request.StrategicInitiativeId)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(HttpContext));

        var result = await _sender.Send(request.ToCreateStrategicInitiativeKpiCommand(), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetKpi), new { id = id, kpiId = result.Value }, result.Value)
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPut("{id}/kpis/{kpiId}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.StrategicInitiatives)]
    [OpenApiOperation("Update a KPI for a strategic initiative.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> UpdateKpi(Guid id, Guid kpiId, [FromBody] UpdateStrategicInitiativeKpiRequest request, CancellationToken cancellationToken)
    {
        if (id != request.StrategicInitiativeId || kpiId != request.KpiId)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(HttpContext));

        var result = await _sender.Send(request.ToUpdateStrategicInitiativeKpiCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpDelete("{id}/kpis/{kpiId}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.StrategicInitiatives)]
    [OpenApiOperation("Delete a KPI for a strategic initiative.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteKpi(Guid id, Guid kpiId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteStrategicInitiativeKpiCommand(id, kpiId), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPost("{id}/kpis/{kpiId}/measurements")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.StrategicInitiatives)]
    [OpenApiOperation("Add a measurement to the strategic initiative KPI.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> AddKpiMeasurement(Guid id, Guid kpiId, [FromBody] AddStrategicInitiativeKpiMeasurementRequest request, CancellationToken cancellationToken)
    {
        if (id != request.StrategicInitiativeId || kpiId != request.KpiId)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(HttpContext));

        var result = await _sender.Send(request.ToAddStrategicInitiativeKpiMeasurementCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpGet("kpi-units")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.StrategicInitiatives)]
    [OpenApiOperation("Get a list of KPI units.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<StrategicInitiativeKpiUnitDto>>> GetKpiUnits(CancellationToken cancellationToken)
    {
        var units = await _sender.Send(new GetStrategicInitiativeKpiUnitsQuery(), cancellationToken);

        return Ok(units);
    }

    [HttpGet("kpi-target-directions")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.StrategicInitiatives)]
    [OpenApiOperation("Get a list of KPI target directions.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<StrategicInitiativeKpiTargetDirectionDto>>> GetKpiTargetDirections(CancellationToken cancellationToken)
    {
        var targetDirections = await _sender.Send(new GetStrategicInitiativeKpiTargetDirectionsQuery(), cancellationToken);

        return Ok(targetDirections);
    }

    #endregion KPIs

    #region Projects

    [HttpGet("{idOrKey}/projects")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.StrategicInitiatives)]
    [OpenApiOperation("Get a list of projects for the strategic initiative.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ProjectListDto>>> GetProjects(string idOrKey, CancellationToken cancellationToken)
    {
        var projects = await _sender.Send(new GetStrategicInitiativeProjectsQuery(idOrKey), cancellationToken);

        return projects is not null
            ? Ok(projects)
            : NotFound();
    }

    [HttpPost("{id}/projects")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.StrategicInitiatives)]
    [OpenApiOperation("Manage projects for the strategic initiative.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> ManageProjects(Guid id, [FromBody] ManageStrategicInitiativeProjectsRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(HttpContext));

        var result = await _sender.Send(request.ToManageStrategicInitiativeProjectsCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    #endregion Projects
}
