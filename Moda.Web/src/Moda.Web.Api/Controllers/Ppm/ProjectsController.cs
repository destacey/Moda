﻿using Moda.Common.Application.Models;
using Moda.ProjectPortfolioManagement.Application.Projects.Commands;
using Moda.ProjectPortfolioManagement.Application.Projects.Dtos;
using Moda.ProjectPortfolioManagement.Application.Projects.Queries;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.Web.Api.Extensions;
using Moda.Web.Api.Models.Ppm.Projects;

namespace Moda.Web.Api.Controllers.Ppm;

[Route("api/ppm/[controller]")]
[ApiVersionNeutral]
[ApiController]
public class ProjectsController(ILogger<ProjectsController> logger, ISender sender) : ControllerBase
{
    private readonly ILogger<ProjectsController> _logger = logger;
    private readonly ISender _sender = sender;

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Projects)]
    [OpenApiOperation("Get a list of projects.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ProjectListDto>>> GetProjects(CancellationToken cancellationToken, [FromQuery] int? status = null)
    {
        ProjectStatus? filter = status.HasValue ? (ProjectStatus)status.Value : null;

        var portfolios = await _sender.Send(new GetProjectsQuery(filter), cancellationToken);

        return Ok(portfolios);
    }

    [HttpGet("{idOrKey}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Projects)]
    [OpenApiOperation("Get project details.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDetailsDto>> GetProject(string idOrKey, CancellationToken cancellationToken)
    {
        var portfolio = await _sender.Send(new GetProjectQuery(idOrKey), cancellationToken);

        return portfolio is not null
            ? Ok(portfolio)
            : NotFound();
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.Projects)]
    [OpenApiOperation("Create a portfolio.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.CreateReturn201IdAndKey))]
    public async Task<ActionResult<ObjectIdAndKey>> Create([FromBody] CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToCreateProjectCommand(), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetProject), new { idOrKey = result.Value.Id.ToString() }, result.Value)
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPut("{id}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Projects)]
    [OpenApiOperation("Update a portfolio.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateProjectRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(HttpContext));

        var result = await _sender.Send(request.ToUpdateProjectCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPost("{id}/activate")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Projects)]
    [OpenApiOperation("Activate a project.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ActivateProjectCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPost("{id}/complete")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Projects)]
    [OpenApiOperation("Complete a project.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Close(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CompleteProjectCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPost("{id}/cancel")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Projects)]
    [OpenApiOperation("Cancel a project.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CancelProjectCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpDelete("{id}")]
    [MustHavePermission(ApplicationAction.Delete, ApplicationResource.Projects)]
    [OpenApiOperation("Delete a portfolio.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteProjectCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }
}
