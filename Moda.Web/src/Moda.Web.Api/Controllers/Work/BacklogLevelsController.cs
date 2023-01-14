using Moda.Web.Api.Models.Work.BacklogLevels;
using Moda.Work.Application.BacklogLevels.Dtos;
using Moda.Work.Application.BacklogLevels.Queries;

namespace Moda.Web.Api.Controllers.Work;

public class BacklogLevelsController : VersionNeutralApiController
{
    private readonly ILogger<BacklogLevelsController> _logger;
    private readonly ISender _sender;

    public BacklogLevelsController(ILogger<BacklogLevelsController> logger, ISender sender)
    {
        _logger = logger;
        _sender = sender;
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.BacklogLevels)]
    [OpenApiOperation("Get a list of all backlog levels.", "")]
    public async Task<ActionResult<IReadOnlyList<BacklogLevelDto>>> GetList(CancellationToken cancellationToken)
    {
        var backlogLevels = await _sender.Send(new GetBacklogLevelsQuery(), cancellationToken);
        
        return Ok(backlogLevels.OrderBy(l => (int)l.Category).ThenByDescending(s => s.Rank));
    }
    
    [HttpGet("{id}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.BacklogLevels)]
    [OpenApiOperation("Get backlog level details using the id.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BacklogLevelDto>> GetById(int id)
    {
        var backlogLevel = await _sender.Send(new GetBacklogLevelQuery(id));

        return backlogLevel is not null
            ? Ok(backlogLevel)
            : NotFound();
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.BacklogLevels)]
    [OpenApiOperation("Create a backlog level.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.Create))]
    public async Task<ActionResult> Create(CreateBacklogLevelRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToCreateBacklogLevelCommand(), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpPut("{id}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.BacklogLevels)]
    [OpenApiOperation("Update a backlog level.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Update(int id, UpdateBacklogLevelRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest();

        var result = await _sender.Send(request.ToUpdateBacklogLevelCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.Error);
    }

    //[HttpDelete("{id}")]
    //[MustHavePermission(ApplicationAction.Delete, ApplicationResource.BacklogLevels)]
    //[OpenApiOperation("Delete a backlog level.", "")]
    //public async Task<ActionResult> Delete(int id)
    //{
    //    throw new NotImplementedException();
    //}
}
