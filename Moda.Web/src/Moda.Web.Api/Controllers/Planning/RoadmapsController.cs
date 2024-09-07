using Moda.Common.Application.Models;
using Moda.Common.Application.Requests;
using Moda.Planning.Application.Risks.Dtos;
using Moda.Planning.Application.Risks.Queries;
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

    public RoadmapsController(ILogger<RoadmapsController> logger, ISender sender)
    {
        _logger = logger;
        _sender = sender;
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
    [ProducesResponseType(typeof(CreateRoadmapReponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesDefaultResponseType(typeof(ErrorResult))]
    public async Task<ActionResult<CreateRoadmapReponse>> Create([FromBody] CreateRoadmapRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToCreateRoadmapCommand(), cancellationToken);
        if (result.IsFailure)
            return BadRequest(ErrorResult.CreateBadRequest(result.Error, "RoadmapsController.Create"));

        var response = new CreateRoadmapReponse { RoadmapIds = result.Value };
        if (request.ParentId.HasValue)
        {
            var addLinkResult = await _sender.Send(new CreateRoadmapLinkCommand(request.ParentId.Value, response.RoadmapIds.Id), cancellationToken);
            if (addLinkResult.IsFailure)
                response.LinkToParentError = addLinkResult.Error;
        }

        return CreatedAtAction(nameof(GetRoadmap), new { idOrKey = response.RoadmapIds.Id.ToString() }, response);
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
