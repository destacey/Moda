﻿using Moda.Common.Application.Requests.WorkManagement;
using Moda.Web.Api.Extensions;
using Moda.Work.Application.WorkProcesses.Commands;
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
    [OpenApiOperation("Get a list of work processes.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<WorkProcessListDto>>> GetList(CancellationToken cancellationToken, bool includeInactive = false)
    {
        var workProcesses = await _sender.Send(new GetWorkProcessesQuery(includeInactive), cancellationToken);
        return Ok(workProcesses.OrderBy(s => s.Name));
    }

    [HttpGet("{idOrKey}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.WorkProcesses)]
    [OpenApiOperation("Get work process details.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WorkProcessDto>> Get(string idOrKey, CancellationToken cancellationToken)
    {
        GetWorkProcessQuery query;
        if (Guid.TryParse(idOrKey, out Guid guidId))
        {
            query = new GetWorkProcessQuery(guidId);
        }
        else if (int.TryParse(idOrKey, out int intId))
        {
            query = new GetWorkProcessQuery(intId);
        }
        else
        {
            return BadRequest(ProblemDetailsExtensions.ForUnknownIdOrKeyType(HttpContext));
        }

        var result = await _sender.Send(query, cancellationToken);

        return result.IsFailure
            ? BadRequest(result.ToBadRequestObject(HttpContext))
            : result.Value is not null
                ? Ok(result.Value)
                : NotFound();
    }

    [HttpPost("{id}/activate")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.WorkProcesses)]
    [OpenApiOperation("Activate a work process.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ActivateWorkProcessCommand(id), cancellationToken);

        return result.IsSuccess 
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPost("{id}/deactivate")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.WorkProcesses)]
    [OpenApiOperation("Deactivate a work process.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateWorkProcessCommand(id), cancellationToken);

        return result.IsSuccess 
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    // get work process schemes
    [HttpGet("{id}/schemes")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.WorkProcesses)]
    [OpenApiOperation("Get work process schemes.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<WorkProcessSchemeDto>>> GetSchemes(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetWorkProcessSchemesQuery(id), cancellationToken);

        return result is not null
                ? Ok(result)
                : NotFound();
    }
}
