using Moda.Web.Api.Models.Work.WorkStates;
using Moda.Work.Application.WorkStates.Dtos;
using Moda.Work.Application.WorkStates.Queries;

namespace Moda.Web.Api.Controllers.Work;

[Route("api/work/work-states")]
[ApiVersionNeutral]
[ApiController]
public class WorkStatesController : ControllerBase
{
    private readonly ILogger<WorkStatesController> _logger;
    private readonly ISender _sender;

    public WorkStatesController(ILogger<WorkStatesController> logger, ISender sender)
    {
        _logger = logger;
        _sender = sender;
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.WorkStates)]
    [OpenApiOperation("Get a list of all work states.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<WorkStateDto>>> GetList(CancellationToken cancellationToken, bool includeInactive = false)
    {
        var workStates = await _sender.Send(new GetWorkStatesQuery(includeInactive), cancellationToken);
        return Ok(workStates.OrderBy(s => s.Name));
    }

    [HttpGet("{id}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.WorkStates)]
    [OpenApiOperation("Get work state details using the id.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkStateDto>> GetById(int id)
    {
        var workState = await _sender.Send(new GetWorkStateQuery(id));

        return workState is not null
            ? Ok(workState)
            : NotFound();
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.WorkStates)]
    [OpenApiOperation("Create a work state.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.Create))]
    public async Task<ActionResult> Create(CreateWorkStateRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToCreateWorkStateCommand(), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpPut("{id}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.WorkStates)]
    [OpenApiOperation("Update a work state.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Update(int id, UpdateWorkStateRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest();

        var result = await _sender.Send(request.ToUpdateWorkStateCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.Error);
    }

    //[HttpDelete("{id}")]
    //[MustHavePermission(ApplicationAction.Delete, ApplicationResource.WorkStates)]
    //[OpenApiOperation("Delete a work state.", "")]
    //public async Task<ActionResult> Delete(int id)
    //{
    //    throw new NotImplementedException();
    //}
}
