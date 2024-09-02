using Moda.Common.Application.Models;
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
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.CreateReturn201IdAndKey))]
    public async Task<ActionResult<ObjectIdAndKey>> CreateRoadmap([FromBody] CreateRoadmapRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToCreateRoadmapCommand(), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetRoadmap), new { idOrKey = result.Value.Id.ToString() }, result.Value)
            : BadRequest(ErrorResult.CreateBadRequest(result.Error, "RoadmapsController.Create"));
    }
}
