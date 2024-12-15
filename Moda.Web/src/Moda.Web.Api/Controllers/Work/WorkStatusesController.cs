using Moda.Web.Api.Extensions;
using Moda.Web.Api.Models.Work.WorkStatuses;
using Moda.Work.Application.WorkStatuses.Dtos;
using Moda.Work.Application.WorkStatuses.Queries;

namespace Moda.Web.Api.Controllers.Work;

[Route("api/work/work-statuses")]
[ApiVersionNeutral]
[ApiController]
public class WorkStatusesController : ControllerBase
{
    private readonly ILogger<WorkStatusesController> _logger;
    private readonly ISender _sender;

    public WorkStatusesController(ILogger<WorkStatusesController> logger, ISender sender)
    {
        _logger = logger;
        _sender = sender;
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.WorkStatuses)]
    [OpenApiOperation("Get a list of all work statuss.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<WorkStatusDto>>> GetList(CancellationToken cancellationToken, bool includeInactive = false)
    {
        var workStatuses = await _sender.Send(new GetWorkStatusesQuery(includeInactive), cancellationToken);
        return Ok(workStatuses.OrderBy(s => s.Name));
    }

    [HttpGet("{id}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.WorkStatuses)]
    [OpenApiOperation("Get work status details using the id.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkStatusDto>> GetById(int id)
    {
        var workStatus = await _sender.Send(new GetWorkStatusQuery(id));

        return workStatus is not null
            ? Ok(workStatus)
            : NotFound();
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.WorkStatuses)]
    [OpenApiOperation("Create a work status.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.CreateReturn201Int))]
    public async Task<ActionResult> Create(CreateWorkStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToCreateWorkStatusCommand(), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPut("{id}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.WorkStatuses)]
    [OpenApiOperation("Update a work status.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Update(int id, UpdateWorkStatusRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(HttpContext));

        var result = await _sender.Send(request.ToUpdateWorkStatusCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    //[HttpDelete("{id}")]
    //[MustHavePermission(ApplicationAction.Delete, ApplicationResource.WorkStatuses)]
    //[OpenApiOperation("Delete a work status.", "")]
    //public async Task<ActionResult> Delete(int id)
    //{
    //    throw new NotImplementedException();
    //}
}
