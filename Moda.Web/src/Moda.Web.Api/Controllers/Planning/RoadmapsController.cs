using Moda.Common.Application.Models;
using Moda.Common.Application.Requests;
using Moda.Planning.Application.Roadmaps.Commands;
using Moda.Planning.Application.Roadmaps.Dtos;
using Moda.Planning.Application.Roadmaps.Queries;
using Moda.Planning.Domain.Models.Roadmaps;
using Moda.Web.Api.Extensions;
using Moda.Web.Api.Models.Planning.Roadmaps;

namespace Moda.Web.Api.Controllers.Planning;

[Route("api/planning/[controller]")]
[ApiVersionNeutral]
[ApiController]
public class RoadmapsController : ControllerBase
{
    private readonly ILogger<RoadmapsController> _logger;
    private readonly ISender _sender;

    public RoadmapsController(ILogger<RoadmapsController> logger, ISender sender)
    {
        _logger = logger;
        _sender = sender;
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Roadmaps)]
    [OpenApiOperation("Get a list of roadmaps.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<RoadmapListDto>>> GetRoadmaps(CancellationToken cancellationToken)
    {
        var roadmaps = await _sender.Send(new GetRoadmapsQuery(), cancellationToken);
        return Ok(roadmaps);
    }

    [HttpGet("{idOrKey}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Roadmaps)]
    [OpenApiOperation("Get roadmap details.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
        
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetRoadmap), new { idOrKey = result.Value.Id.ToString() }, result.Value)
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPut("{id}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Roadmaps)]
    [OpenApiOperation("Update a roadmap.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateRoadmapRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(HttpContext));

        var result = await _sender.Send(request.ToUpdateRoadmapCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpDelete("{id}")]
    [MustHavePermission(ApplicationAction.Delete, ApplicationResource.Roadmaps)]
    [OpenApiOperation("Delete a roadmap.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteRoadmapCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    #region Roadmap Items

    [HttpGet("{idOrKey}/items")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Roadmaps)]
    [OpenApiOperation("Get roadmap items", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<RoadmapItemListDto>>> GetItems(string idOrKey, CancellationToken cancellationToken)
    {
        var items = await _sender.Send(new GetRoadmapItemsQuery(idOrKey), cancellationToken);
        return Ok(items);
    }

    [HttpGet("{idOrKey}/items/activities")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Roadmaps)]
    [OpenApiOperation("Get roadmap activities", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<RoadmapActivityListDto>>> GetActivities(string idOrKey, CancellationToken cancellationToken)
    {
        var items = await _sender.Send(new GetRoadmapActivitiesQuery(idOrKey), cancellationToken);
        return Ok(items);
    }

    [HttpGet("{roadmapIdOrKey}/items/{itemId}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Roadmaps)]
    [OpenApiOperation("Get roadmap item details", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RoadmapItemDetailsDto>> GetItem(string roadmapIdOrKey, Guid itemId, CancellationToken cancellationToken)
    {
        var item = await _sender.Send(new GetRoadmapItemQuery(roadmapIdOrKey, itemId), cancellationToken);
        return Ok(item);
    }

    [HttpPost("{roadmapId}/items")]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.Roadmaps)]
    [OpenApiOperation("Create a roadmap item of type: Activity, Timebox, Milestone.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.CreateReturn201IdAndKey))]
    public async Task<ActionResult<ObjectIdAndKey>> CreateItem(Guid roadmapId, [FromBody] CreateRoadmapItemRequest request, CancellationToken cancellationToken)
    {
        if (roadmapId != request.RoadmapId)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(nameof(roadmapId), nameof(request.RoadmapId), HttpContext));

        var command = request switch
        {
            CreateRoadmapActivityRequest activity => activity.ToCreateRoadmapItemCommand(),
            CreateRoadmapTimeboxRequest timebox => timebox.ToCreateRoadmapItemCommand(),
            CreateRoadmapMilestoneRequest milestone => milestone.ToCreateRoadmapItemCommand(),
            _ => throw new ArgumentException("Invalid roadmap item type", nameof(request))
        };

        var result = await _sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetItem), new { roadmapIdOrKey = roadmapId, itemId = result.Value.ToString() }, result.Value)
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPut("{roadmapId}/items/{itemId}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Roadmaps)]
    [OpenApiOperation("Update a roadmap item of type: Activity, Timebox, Milestone.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> UpdateItem(Guid roadmapId, Guid itemId, [FromBody] UpdateRoadmapItemRequest request, CancellationToken cancellationToken)
    {
        if (roadmapId != request.RoadmapId)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(nameof(roadmapId), nameof(request.RoadmapId), HttpContext));
        else if (itemId != request.ItemId)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(nameof(itemId), nameof(request.ItemId), HttpContext));

        var command = request switch
        {
            UpdateRoadmapActivityRequest activity => activity.ToUpdateRoadmapItemCommand(),
            UpdateRoadmapTimeboxRequest timebox => timebox.ToUpdateRoadmapItemCommand(),
            UpdateRoadmapMilestoneRequest milestone => milestone.ToUpdateRoadmapItemCommand(),
            _ => throw new ArgumentException("Invalid roadmap item type", nameof(request))
        };

        var result = await _sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }


    //[HttpPost("{id}/children/order")]
    //[MustHavePermission(ApplicationAction.Update, ApplicationResource.Roadmaps)]
    //[OpenApiOperation("Update the order of child roadmaps.", "")]
    //[ProducesResponseType(StatusCodes.Status204NoContent)]
    //[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    //public async Task<ActionResult> UpdateChildrenOrder(Guid id, [FromBody] UpdateRoadmapChildrenOrderRequest request, CancellationToken cancellationToken)
    //{
    //    if (id != request.RoadmapId)
    //        return BadRequest();

    //    var result = await _sender.Send(new UpdateRoadmapRootActivitiesOrderCommand(request.RoadmapId, request.ChildrenOrder), cancellationToken);

    //    return result.IsSuccess
    //        ? NoContent()
    //        : BadRequest(ErrorResult.CreateBadRequest(result.Error, "RoadmapsController.UpdateChildrenOrder"));
    //}

    //[HttpPost("{id}/children/{childRoadmapId}/order")]
    //[MustHavePermission(ApplicationAction.Update, ApplicationResource.Roadmaps)]
    //[OpenApiOperation("Update the order of child roadmaps based on a single change.", "")]
    //[ProducesResponseType(StatusCodes.Status204NoContent)]
    //[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    //public async Task<ActionResult> UpdateChildOrder(Guid id, Guid childRoadmapId, [FromBody] UpdateRoadmapChildOrderRequest request, CancellationToken cancellationToken)
    //{
    //    if (id != request.RoadmapId)
    //        return BadRequest();

    //    if (childRoadmapId != request.ChildRoadmapId)
    //        return BadRequest();


    //    var result = await _sender.Send(new UpdateRoadmapRootActivityOrderCommand(request.RoadmapId, request.ChildRoadmapId, request.Order), cancellationToken);

    //    return result.IsSuccess
    //        ? NoContent()
    //        : BadRequest(ErrorResult.CreateBadRequest(result.Error, "RoadmapsController.UpdateChildOrder"));
    //}

    [HttpDelete("{roadmapId}/items/{itemId}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Roadmaps)]
    [OpenApiOperation("Delete a roadmap item.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> DeleteItem(Guid roadmapId, Guid itemId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteRoadmapItemCommand(roadmapId, itemId), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    #endregion Roadmap Items

    [HttpGet("visibility-options")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Risks)]
    [OpenApiOperation("Get a list of all visibility.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<VisibilityDto>>> GetVisibilityOptions(CancellationToken cancellationToken)
    {
        var items = await _sender.Send(new GetVisibilitiesQuery(), cancellationToken);
        return Ok(items.OrderBy(c => c.Order));
    }
}
