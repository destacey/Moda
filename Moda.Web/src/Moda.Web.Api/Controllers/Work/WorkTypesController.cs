using Moda.Web.Api.Models.Work.WorkTypes;
using Moda.Work.Application.WorkTypes.Dtos;
using Moda.Work.Application.WorkTypes.Queries;

namespace Moda.Web.Api.Controllers.Work;

public class WorkTypesController : VersionNeutralApiController
{
    private readonly ILogger<WorkTypesController> _logger;
    private readonly ISender _sender;

    public WorkTypesController(ILogger<WorkTypesController> logger, ISender sender)
    {
        _logger = logger;
        _sender = sender;
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.WorkTypes)]
    [OpenApiOperation("Get a list of all work types.", "")]
    public async Task<ActionResult<IReadOnlyList<WorkTypeDto>>> GetList(CancellationToken cancellationToken, bool includeInactive = false)
    {
        var workTypes = await _sender.Send(new GetWorkTypesQuery(includeInactive), cancellationToken);
        return Ok(workTypes.OrderBy(s => s.Name));
    }

    [HttpGet("{id}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.WorkTypes)]
    [OpenApiOperation("Get work type details using the id.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkTypeDto>> GetById(int id)
    {
        var workType = await _sender.Send(new GetWorkTypeQuery(id));

        return workType is not null
            ? Ok(workType)
            : NotFound();
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.WorkTypes)]
    [OpenApiOperation("Create a work type.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.Create))]
    public async Task<ActionResult> Create(CreateWorkTypeRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToCreateWorkTypeCommand(), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpPut("{id}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.WorkTypes)]
    [OpenApiOperation("Update a work type.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Update(int id, UpdateWorkTypeRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest();

        var result = await _sender.Send(request.ToUpdateWorkTypeCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.Error);
    }

    //[HttpDelete("{id}")]
    //[MustHavePermission(ApplicationAction.Delete, ApplicationResource.WorkTypes)]
    //[OpenApiOperation("Delete a work type.", "")]
    //public async Task<ActionResult> Delete(int id)
    //{
    //    throw new NotImplementedException();
    //}
}
