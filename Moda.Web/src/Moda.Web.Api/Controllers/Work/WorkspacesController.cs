﻿using Moda.Common.Extensions;
using Moda.Work.Application.Workspaces.Dtos;
using Moda.Work.Application.Workspaces.Queries;

namespace Moda.Web.Api.Controllers.Work;

[Route("api/work/workspaces")]
[ApiVersionNeutral]
[ApiController]
public class WorkspacesController(ILogger<WorkspacesController> logger, ISender sender) : ControllerBase
{
    private readonly ILogger<WorkspacesController> _logger = logger;
    private readonly ISender _sender = sender;

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Workspaces)]
    [OpenApiOperation("Get a list of workspaces.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<WorkspaceListDto>>> GetList(CancellationToken cancellationToken, bool includeInactive = false)
    {
        var workspaces = await _sender.Send(new GetWorkspacesQuery(includeInactive), cancellationToken);
        return Ok(workspaces);
    }

    [HttpGet("{idOrKey}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Workspaces)]
    [OpenApiOperation("Get workspace details.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WorkspaceDto>> Get(string idOrKey, CancellationToken cancellationToken)
    {
        GetWorkspaceQuery query;
        if (Guid.TryParse(idOrKey, out Guid guidId))
        {
            query = new GetWorkspaceQuery(guidId);
        }
        else if (idOrKey.IsValidWorkspaceKeyFormat())
        {
            query = new GetWorkspaceQuery(new WorkspaceKey(idOrKey));
        }
        else
        {
            return BadRequest(ErrorResult.CreateUnknownIdOrKeyTypeBadRequest("WorkspacesController.GetCalendar"));
        }

        var result = await _sender.Send(query, cancellationToken);

        return result.IsFailure
            ? BadRequest(ErrorResult.CreateBadRequest(result.Error, "WorkspacesController.Get"))
            : result.Value is not null
                ? result.Value
                : NotFound();
    }
}