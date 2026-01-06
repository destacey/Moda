using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using Moda.Common.Domain.Models.ProjectPortfolioManagement;
using Moda.ProjectPortfolioManagement.Application.Projects.Queries;
using Moda.ProjectPortfolioManagement.Application.ProjectTasks.Commands;
using Moda.ProjectPortfolioManagement.Application.ProjectTasks.Dtos;
using Moda.ProjectPortfolioManagement.Application.ProjectTasks.Models;
using Moda.ProjectPortfolioManagement.Application.ProjectTasks.Queries;
using Moda.Web.Api.Extensions;
using Moda.Web.Api.Models.Ppm.ProjectTasks;
using TaskStatus = Moda.ProjectPortfolioManagement.Domain.Enums.TaskStatus;

namespace Moda.Web.Api.Controllers.Ppm;

[Route("api/ppm/projects/{projectIdOrKey}/tasks")]
[ApiVersionNeutral]
[ApiController]
public class ProjectTasksController(ILogger<ProjectTasksController> logger, ISender sender) : ControllerBase
{
    private readonly ILogger<ProjectTasksController> _logger = logger;
    private readonly ISender _sender = sender;

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Projects)]
    [OpenApiOperation("Get a list of project tasks.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ProjectTaskListDto>>> GetProjectTasks(
        string projectIdOrKey,
        CancellationToken cancellationToken,
        [FromQuery] int? status = null,
        [FromQuery] Guid? parentId = null)
    {
        TaskStatus? statusFilter = status.HasValue ? (TaskStatus)status.Value : null;

        var tasks = await _sender.Send(
            new GetProjectTasksQuery(projectIdOrKey, statusFilter, parentId),
            cancellationToken);

        return Ok(tasks);
    }

    [HttpGet("tree")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Projects)]
    [OpenApiOperation("Get a hierarchical tree of project tasks with WBS codes.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ProjectTaskTreeDto>>> GetProjectTaskTree(
        string projectIdOrKey,
        CancellationToken cancellationToken)
    {
        var tasks = await _sender.Send(new GetProjectTaskTreeQuery(projectIdOrKey), cancellationToken);

        return Ok(tasks);
    }

    [HttpGet("{taskIdOrKey}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Projects)]
    [OpenApiOperation("Get project task details.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectTaskDto>> GetProjectTask(
        string projectIdOrKey,
        string taskIdOrKey,
        CancellationToken cancellationToken)
    {
        var task = await _sender.Send(new GetProjectTaskQuery(taskIdOrKey), cancellationToken);

        return task is not null
            ? Ok(task)
            : NotFound();
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.Projects)]
    [OpenApiOperation("Create a project task.", "")]
    [ProducesResponseType(typeof(ProjectTaskIdAndKey), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ProjectTaskIdAndKey>> CreateProjectTask(
        string projectIdOrKey,
        [FromBody] CreateProjectTaskRequest request,
        CancellationToken cancellationToken)
    {
        // Resolve project ID from IdOrKey
        var projectId = await ResolveProjectId(projectIdOrKey, cancellationToken);
        if (projectId is null)
            return NotFound($"Project with identifier '{projectIdOrKey}' not found.");

        var result = await _sender.Send(request.ToCreateProjectTaskCommand(projectId.Value), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(
                nameof(GetProjectTask),
                new { projectIdOrKey, taskIdOrKey = result.Value.Key },
                result.Value)
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPut("{id}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Projects)]
    [OpenApiOperation("Update a project task.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> UpdateProjectTask(
        string projectIdOrKey,
        Guid id,
        [FromBody] UpdateProjectTaskRequest request,
        CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(HttpContext));

        var result = await _sender.Send(request.ToUpdateProjectTaskCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPatch("{id}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Projects)]
    [OpenApiOperation("Partially update a project task using JSON Patch (RFC 6902).", "Applies a JSON Patch document to update specific fields.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> PatchProjectTask(
        string projectIdOrKey,
        Guid id,
        [FromBody] JsonPatchDocument<UpdateProjectTaskRequest> patchDocument,
        CancellationToken cancellationToken)
    {
        if (patchDocument == null)
            return BadRequest("Patch document cannot be null.");

        // Get the current task state
        var taskDto = await _sender.Send(new GetProjectTaskQuery(id.ToString()), cancellationToken);
        if (taskDto is null)
            return NotFound($"Project task with ID '{id}' not found.");

        // Convert DTO to UpdateProjectTaskRequest
        var updateRequest = UpdateProjectTaskRequest.FromDto(taskDto);

        // Apply the patch document to the update request
        patchDocument.ApplyTo(updateRequest, error =>
        {
            ModelState.AddModelError(error.AffectedObject.GetType().Name, error.ErrorMessage);
        });

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var result = await _sender.Send(updateRequest.ToUpdateProjectTaskCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpDelete("{id}")]
    [MustHavePermission(ApplicationAction.Delete, ApplicationResource.Projects)]
    [OpenApiOperation("Delete a project task.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteProjectTask(
        string projectIdOrKey,
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteProjectTaskCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPut("{id}/placement")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Projects)]
    [OpenApiOperation("Update a project task's placement within a parent.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> UpdateProjectTaskPlacement(
        string projectIdOrKey,
        Guid id,
        [FromBody] UpdateProjectTaskPlacementRequest request,
        CancellationToken cancellationToken)
    {
        if (id != request.TaskId)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(HttpContext));

        var projectId = await ResolveProjectId(projectIdOrKey, cancellationToken);
        if (projectId is null)
            return NotFound($"Project with identifier '{projectIdOrKey}' not found.");

        var result = await _sender.Send(request.ToUpdateProjectTaskPlacementCommand(projectId.Value), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpGet("critical-path")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Projects)]
    [OpenApiOperation("Get the critical path for the project.", "Returns an ordered list of task IDs on the critical path.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<Guid>>> GetCriticalPath(
        string projectIdOrKey,
        CancellationToken cancellationToken)
    {
        var criticalPath = await _sender.Send(new GetCriticalPathQuery(projectIdOrKey), cancellationToken);

        return Ok(criticalPath);
    }

    [HttpPost("{id}/dependencies")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Projects)]
    [OpenApiOperation("Add a dependency to a task.", "Creates a finish-to-start dependency where the specified task is the predecessor.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> AddTaskDependency(
        string projectIdOrKey,
        Guid id,
        [FromBody] AddTaskDependencyRequest request,
        CancellationToken cancellationToken)
    {
        if (id != request.PredecessorId)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(HttpContext));

        var projectId = await ResolveProjectId(projectIdOrKey, cancellationToken);
        if (projectId is null)
            return NotFound($"Project with identifier '{projectIdOrKey}' not found.");

        var result = await _sender.Send(new AddProjectTaskDependencyCommand(projectId.Value, request.PredecessorId, request.SuccessorId), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpDelete("{id}/dependencies/{successorId}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Projects)]
    [OpenApiOperation("Remove a dependency from a task.", "Removes the finish-to-start dependency between the predecessor and successor tasks.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> RemoveTaskDependency(
        string projectIdOrKey,
        Guid id,
        Guid successorId,
        CancellationToken cancellationToken)
    {

        var projectId = await ResolveProjectId(projectIdOrKey, cancellationToken);
        if (projectId is null)
            return NotFound($"Project with identifier '{projectIdOrKey}' not found.");

        var result = await _sender.Send(new RemoveProjectTaskDependencyCommand(projectId.Value, id, successorId), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpGet("/api/ppm/projects/tasks/types")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Projects)]
    [OpenApiOperation("Get a list of all task types.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ProjectTaskTypeDto>>> GetTaskTypes(CancellationToken cancellationToken)
    {
        var items = await _sender.Send(new GetProjectTaskTypesQuery(), cancellationToken);
        return Ok(items.OrderBy(c => c.Order));
    }

    [HttpGet("/api/ppm/projects/tasks/statuses")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Projects)]
    [OpenApiOperation("Get a list of all task statuses.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<TaskStatusDto>>> GetTaskStatuses(CancellationToken cancellationToken)
    {
        var items = await _sender.Send(new GetProjectTaskStatusesQuery(), cancellationToken);
        return Ok(items.OrderBy(c => c.Order));
    }

    [HttpGet("/api/ppm/projects/tasks/priorities")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Projects)]
    [OpenApiOperation("Get a list of all task priorities.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<TaskPriorityDto>>> GetTaskPriorities(CancellationToken cancellationToken)
    {
        var items = await _sender.Send(new GetProjectTaskPrioritiesQuery(), cancellationToken);
        return Ok(items.OrderBy(c => c.Order));
    }

    private async Task<Guid?> ResolveProjectId(string projectIdOrKey, CancellationToken cancellationToken)
    {
        if (Guid.TryParse(projectIdOrKey, out var id))
        {
            return id;
        }

        try
        {
            var key = new ProjectKey(projectIdOrKey);
            return await _sender.Send(new GetProjectIdQuery(key), cancellationToken);
        }
        catch
        {
            return null;
        }
    }
}
