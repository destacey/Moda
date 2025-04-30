﻿using Moda.Common.Extensions;
using Moda.Web.Api.Extensions;
using Moda.Web.Api.Models.Work.Workspaces;
using Moda.Work.Application.WorkItems.Commands;
using Moda.Work.Application.WorkItems.Dtos;
using Moda.Work.Application.WorkItems.Queries;
using Moda.Work.Application.Workspaces.Commands;
using Moda.Work.Application.Workspaces.Dtos;
using Moda.Work.Application.Workspaces.Queries;
using Moda.Work.Domain.Models;

namespace Moda.Web.Api.Controllers.Work;

[Route("api/work/workspaces")]
[ApiVersionNeutral]
[ApiController]
public class WorkspacesController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Workspaces)]
    [OpenApiOperation("Get a list of workspaces.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<WorkspaceListDto>>> GetList(CancellationToken cancellationToken, bool includeInactive = false)
    {
        var workspaces = await _sender.Send(new GetWorkspacesQuery(includeInactive), cancellationToken);
        return Ok(workspaces);
    }

    [HttpGet("{idOrKey}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Workspaces)]
    [OpenApiOperation("Get workspace details.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
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
            return BadRequest(ProblemDetailsExtensions.ForUnknownIdOrKeyType(HttpContext));
        }

        var result = await _sender.Send(query, cancellationToken);

        return result.IsFailure
            ? BadRequest(result.ToBadRequestObject(HttpContext))
            : result.Value is not null
                ? result.Value
                : NotFound();
    }

    [HttpPut("{id}/external-url-templates")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Workspaces)]
    [OpenApiOperation("Set the external view work item URL template for a workspace.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> SetExternalUrlTemplates(Guid id, [FromBody] SetExternalUrlTemplatesRequest dto, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new SetExternalViewWorkItemUrlTemplateCommand(id, dto.ExternalViewWorkItemUrlTemplate), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    #region Work Items

    [HttpGet("{idOrKey}/work-items")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.WorkItems)]
    [OpenApiOperation("Get work items for a workspace.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<WorkItemListDto>>> GetWorkItems(string idOrKey, CancellationToken cancellationToken)
    {
        GetWorkItemsQuery query;
        if (Guid.TryParse(idOrKey, out Guid guidId))
        {
            query = new GetWorkItemsQuery(guidId);
        }
        else if (idOrKey.IsValidWorkspaceKeyFormat())
        {
               query = new GetWorkItemsQuery(new WorkspaceKey(idOrKey));
        }
        else
        {
            return BadRequest(ProblemDetailsExtensions.ForUnknownIdOrKeyType(HttpContext));
        }

        var result = await _sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value.OrderByKey(true))
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpGet("{idOrKey}/work-items/{workItemKey}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.WorkItems)]
    [OpenApiOperation("Get work item details.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WorkItemDetailsDto>> GetWorkItem(string idOrKey, string workItemKey, CancellationToken cancellationToken)
    {
        // TODO: allow work item key or id
        var key = new WorkItemKey(workItemKey);
        GetWorkItemQuery query;
        if (Guid.TryParse(idOrKey, out Guid guidId))
        {
            query = new GetWorkItemQuery(guidId, key);
        }
        else if (idOrKey.IsValidWorkspaceKeyFormat())
        {
            query = new GetWorkItemQuery(new WorkspaceKey(idOrKey), key);
        }
        else
        {
            return BadRequest(ProblemDetailsExtensions.ForUnknownIdOrKeyType(HttpContext));
        }

        var result = await _sender.Send(query, cancellationToken);

        return result.IsFailure
            ? BadRequest(result.ToBadRequestObject(HttpContext))
            : result.Value is not null
                ? result.Value
                : NotFound();
    }

    [HttpPut("{id}/work-items/{workItemKey}/update-project")]
    [MustHavePermission(ApplicationAction.ManageProjectWorkItems, ApplicationResource.Projects)]
    [OpenApiOperation("Update the project for a work item.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateProjectId(Guid id, string workItemKey, [FromBody] UpdateWorkItemProjectRequest request, CancellationToken cancellationToken)
    {
        if (workItemKey != request.WorkItemKey)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(HttpContext));

        var result = await _sender.Send(request.ToUpdateWorkItemProjectCommand(), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpGet("{idOrKey}/work-items/{workItemKey}/children")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.WorkItems)]
    [OpenApiOperation("Get a work item's child work items.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<WorkItemListDto>>> GetChildWorkItems(string idOrKey, string workItemKey, CancellationToken cancellationToken)
    {
        // TODO: allow work item key or id
        var key = new WorkItemKey(workItemKey);
        GetChildWorkItemsQuery query;
        if (Guid.TryParse(idOrKey, out Guid guidId))
        {
            query = new GetChildWorkItemsQuery(guidId, key);
        }
        else if (idOrKey.IsValidWorkspaceKeyFormat())
        {
            query = new GetChildWorkItemsQuery(new WorkspaceKey(idOrKey), key);
        }
        else
        {
            return BadRequest(ProblemDetailsExtensions.ForUnknownIdOrKeyType(HttpContext));
        }

        var result = await _sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value.OrderBy(w => w.StackRank))
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpGet("{idOrKey}/work-items/{workItemKey}/dependencies")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.WorkItems)]
    [OpenApiOperation("Get a work item's dependencies.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ScopedDependencyDto>>> GetWorkItemDependencies(string idOrKey, string workItemKey, CancellationToken cancellationToken)
    {
        var key = new WorkItemKey(workItemKey);

        var result = await _sender.Send(new GetWorkItemDependenciesQuery(idOrKey, key), cancellationToken);

        
        return result.IsFailure
            ? BadRequest(result.ToBadRequestObject(HttpContext))
            : result.Value is not null
                ? Ok(result.Value.OrderBy(w => w.CreatedOn))
                : NotFound();
    }

    [HttpGet("{idOrKey}/work-items/{workItemKey}/metrics")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.WorkItems)]
    [OpenApiOperation("Get metrics for a work item.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<WorkItemProgressDailyRollupDto>>> GetMetrics(string idOrKey, string workItemKey, CancellationToken cancellationToken)
    {
        // TODO: allow work item key or id
        var key = new WorkItemKey(workItemKey);
        GetWorkItemMetricsQuery query;
        if (Guid.TryParse(idOrKey, out Guid guidId))
        {
            query = new GetWorkItemMetricsQuery(guidId, key);
        }
        else if (idOrKey.IsValidWorkspaceKeyFormat())
        {
            query = new GetWorkItemMetricsQuery(new WorkspaceKey(idOrKey), key);
        }
        else
        {
            return BadRequest(ProblemDetailsExtensions.ForUnknownIdOrKeyType(HttpContext));
        }

        var result = await _sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpGet("work-items/search")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.WorkItems)]
    [OpenApiOperation("Search for a work item using its key or title.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<WorkItemListDto>>> SearchWorkItems(string query, CancellationToken cancellationToken, int top = 50)
    {
        var result = await _sender.Send(new SearchWorkItemsQuery(query, top), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value.OrderByKey(true))
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    #endregion Work Items
}
