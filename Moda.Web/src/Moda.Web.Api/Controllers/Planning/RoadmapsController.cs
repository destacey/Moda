using System.Linq;
using Ardalis.GuardClauses;
using Moda.Common.Application.Interfaces;
using Moda.Common.Application.Models;
using Moda.Common.Application.Requests;
using Moda.Planning.Application.Roadmaps.Commands;
using Moda.Planning.Application.Roadmaps.Dtos;
using Moda.Planning.Application.Roadmaps.Queries;
using Moda.Web.Api.Models.Planning.Roadmaps;

namespace Moda.Web.Api.Controllers.Planning;

[Route("api/planning/[controller]")]
[ApiVersionNeutral]
[ApiController]
public class RoadmapsController : ControllerBase
{
    private readonly ILogger<RoadmapsController> _logger;
    private readonly ISender _sender;
    private readonly ICurrentUser _currentUser;

    public RoadmapsController(ILogger<RoadmapsController> logger, ISender sender, ICurrentUser currentUser)
    {
        _logger = logger;
        _sender = sender;
        _currentUser = currentUser;
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Roadmaps)]
    [OpenApiOperation("Get a list of roadmaps.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<RoadmapListDto>>> GetRoadmaps(CancellationToken cancellationToken)
    {
        var roadmaps = await _sender.Send(new GetRoadmapsQuery(), cancellationToken);
        return Ok(roadmaps);
    }

    [HttpGet("{idOrKey}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Roadmaps)]
    [OpenApiOperation("Get roadmap details.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType(typeof(ErrorResult))]
    public async Task<ActionResult<RoadmapDetailsDto>> GetRoadmap(string idOrKey, CancellationToken cancellationToken)
    {
        var roadmap = await _sender.Send(new GetRoadmapQuery(idOrKey), cancellationToken);

        return roadmap is not null
            ? Ok(roadmap)
            : NotFound();
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.Roadmaps)]
    [OpenApiOperation("Create a roadmap.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.CreateReturn201IdAndKey))]
    public async Task<ActionResult<ObjectIdAndKey>> Create([FromBody] CreateRoadmapRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToCreateRoadmapCommand(), cancellationToken);
        if (result.IsFailure)
            return BadRequest(ErrorResult.CreateBadRequest(result.Error, "RoadmapsController.Create"));

        return CreatedAtAction(nameof(GetRoadmap), new { idOrKey = result.Value.Id.ToString() }, result.Value);
    }

    [HttpPut("{id}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Roadmaps)]
    [OpenApiOperation("Update a roadmap.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateRoadmapRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest();

        //var currentUserEmployeeId = Guard.Against.NullOrEmpty(_currentUser.GetEmployeeId());
        //if (!request.RoadmapManagerIds.Contains(currentUserEmployeeId))
        //{
        //    ModelState.AddModelError(nameof(request.RoadmapManagerIds), "The current user is not listed as a roadmap manager of the roadmap.");
        //    return ValidationProblem();
        //}

        var result = await _sender.Send(request.ToUpdateRoadmapCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(ErrorResult.CreateBadRequest(result.Error, "RoadmapsController.Update"));
    }

    [HttpDelete("{id}")]
    [MustHavePermission(ApplicationAction.Delete, ApplicationResource.Roadmaps)]
    [OpenApiOperation("Delete a roadmap.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteRoadmapCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(ErrorResult.CreateBadRequest(result.Error, "RoadmapsController.Delete"));
    }

    [HttpPost("children")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Roadmaps)]
    [OpenApiOperation("Retrieve child roadmaps for specified roadmap Ids.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<RoadmapChildrenDto>>> GetChildren([FromBody] List<Guid> roadmapIds, CancellationToken cancellationToken)
    {
        var roadmaps = await _sender.Send(new GetRoadmapChildrenQuery(roadmapIds), cancellationToken);
        return Ok(roadmaps);
    }

    [HttpPost("{id}/children/order")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Roadmaps)]
    [OpenApiOperation("Update the order of child roadmaps.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateChildrenOrder(Guid id, [FromBody] UpdateRoadmapChildrenOrderRequest request, CancellationToken cancellationToken)
    {
        if (id != request.RoadmapId)
            return BadRequest();

        var result = await _sender.Send(new UpdateRoadmapChildrenOrderCommand(request.RoadmapId, request.ChildrenOrder), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(ErrorResult.CreateBadRequest(result.Error, "RoadmapsController.UpdateChildrenOrder"));
    }

    [HttpPost("{id}/children/{childRoadmapId}/order")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Roadmaps)]
    [OpenApiOperation("Update the order of child roadmaps based on a single change.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateChildOrder(Guid id, Guid childRoadmapId, [FromBody] UpdateRoadmapChildOrderRequest request, CancellationToken cancellationToken)
    {
        if (id != request.RoadmapId)
            return BadRequest();

        if (childRoadmapId != request.ChildRoadmapId)
            return BadRequest();


        var result = await _sender.Send(new UpdateRoadmapChildOrderCommand(request.RoadmapId, request.ChildRoadmapId, request.Order), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(ErrorResult.CreateBadRequest(result.Error, "RoadmapsController.UpdateChildOrder"));
    }

    [HttpGet("visibility-options")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Risks)]
    [OpenApiOperation("Get a list of all visibility.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<VisibilityDto>>> GetVisibilityOptions(CancellationToken cancellationToken)
    {
        var items = await _sender.Send(new GetVisibilitiesQuery(), cancellationToken);
        return Ok(items.OrderBy(c => c.Order));
    }
}
