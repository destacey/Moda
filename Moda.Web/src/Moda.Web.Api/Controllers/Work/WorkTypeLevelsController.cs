using Moda.Common.Application.Requests.WorkManagement;
using Moda.Web.Api.Models.Work.WorkTypeLevels;
using Moda.Work.Application.WorkTypeLevels.Dtos;
using Moda.Work.Application.WorkTypeLevels.Queries;

namespace Moda.Web.Api.Controllers.Work;

[Route("api/work/work-type-levels")]
[ApiVersionNeutral]
[ApiController]
public class WorkTypeLevelsController : ControllerBase
{
    private readonly ILogger<WorkTypeLevelsController> _logger;
    private readonly ISender _sender;

    public WorkTypeLevelsController(ILogger<WorkTypeLevelsController> logger, ISender sender)
    {
        _logger = logger;
        _sender = sender;
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.WorkTypeLevels)]
    [OpenApiOperation("Get a list of all work type levels.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<WorkTypeLevelDto>>> GetList(CancellationToken cancellationToken)
    {
        var levels = await _sender.Send(new GetWorkTypeLevelsQuery(), cancellationToken);

        return Ok(levels.OrderBy(l => l.Tier.Id).ThenByDescending(s => s.Order));
    }

    [HttpGet("{id}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.WorkTypeLevels)]
    [OpenApiOperation("Get work type level details using the id.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkTypeLevelDto>> GetById(int id)
    {
        var backlogLevel = await _sender.Send(new GetWorkTypeLevelQuery(id));

        return backlogLevel is not null
            ? Ok(backlogLevel)
            : NotFound();
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.WorkTypeLevels)]
    [OpenApiOperation("Create a work type level.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.CreateReturn201Int))]
    public async Task<ActionResult> Create(CreateWorkTypeLevelRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToCreateWorkTypeLevelCommand(), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpPut("{id}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.WorkTypeLevels)]
    [OpenApiOperation("Update a work type level.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Update(int id, UpdateWorkTypeLevelRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest();

        var result = await _sender.Send(request.ToUpdateWorkTypeLevelCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.Error);
    }

    //[HttpDelete("{id}")]
    //[MustHavePermission(ApplicationAction.Delete, ApplicationResource.WorkTypeLevels)]
    //[OpenApiOperation("Delete a work type level.", "")]
    //public async Task<ActionResult> Delete(int id)
    //{
    //    throw new NotImplementedException();
    //}
}
