using Moda.Common.Application.Models;
using Moda.ProjectPortfolioManagement.Application.Programs.Commands;
using Moda.ProjectPortfolioManagement.Application.Programs.Dtos;
using Moda.ProjectPortfolioManagement.Application.Programs.Queries;
using Moda.ProjectPortfolioManagement.Application.Projects.Dtos;
using Moda.ProjectPortfolioManagement.Application.Projects.Queries;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.Web.Api.Extensions;
using Moda.Web.Api.Models.Ppm.Programs;

namespace Moda.Web.Api.Controllers.Ppm;

[Route("api/ppm/[controller]")]
[ApiVersionNeutral]
[ApiController]
public class ProgramsController(ILogger<ProgramsController> logger, ISender sender) : ControllerBase
{
    private readonly ILogger<ProgramsController> _logger = logger;
    private readonly ISender _sender = sender;

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Programs)]
    [OpenApiOperation("Get a list of programs.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ProgramListDto>>> GetPrograms(CancellationToken cancellationToken, [FromQuery] int? status = null)
    {
        ProgramStatus? filter = status.HasValue ? (ProgramStatus)status.Value : null;

        var programs = await _sender.Send(new GetProgramsQuery(StatusFilter: filter), cancellationToken);

        return Ok(programs);
    }

    [HttpGet("{idOrKey}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Programs)]
    [OpenApiOperation("Get program details.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProgramDetailsDto>> GetProgram(string idOrKey, CancellationToken cancellationToken)
    {
        var program = await _sender.Send(new GetProgramQuery(idOrKey), cancellationToken);

        return program is not null
            ? Ok(program)
            : NotFound();
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.Programs)]
    [OpenApiOperation("Create a program.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.CreateReturn201IdAndKey))]
    public async Task<ActionResult<ObjectIdAndKey>> Create([FromBody] CreateProgramRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToCreateProgramCommand(), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetProgram), new { idOrKey = result.Value.Id.ToString() }, result.Value)
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPut("{id}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Programs)]
    [OpenApiOperation("Update a program.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateProgramRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(HttpContext));

        var result = await _sender.Send(request.ToUpdateProgramCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPost("{id}/activate")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Programs)]
    [OpenApiOperation("Activate a program.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ActivateProgramCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPost("{id}/complete")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Programs)]
    [OpenApiOperation("Complete a program.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Complete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CompleteProgramCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPost("{id}/cancel")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Programs)]
    [OpenApiOperation("Cancel a program.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CancelProgramCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpDelete("{id}")]
    [MustHavePermission(ApplicationAction.Delete, ApplicationResource.Programs)]
    [OpenApiOperation("Delete a program.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteProgramCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }



    [HttpGet("{idOrKey}/projects")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Programs)]
    [OpenApiOperation("Get a list of projects.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ProjectListDto>>> GetProjects(string idOrKey, CancellationToken cancellationToken, [FromQuery] int? status = null)
    {
        ProjectStatus? filter = status.HasValue ? (ProjectStatus)status.Value : null;

        var projects = await _sender.Send(new GetProjectsQuery(StatusFilter: filter, ProgramIdOrKey: idOrKey), cancellationToken);

        return Ok(projects);
    }
}
