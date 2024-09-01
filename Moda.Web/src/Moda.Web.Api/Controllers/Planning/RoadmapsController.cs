using Moda.Planning.Application.Roadmaps.Dtos;
using Moda.Planning.Application.Roadmaps.Queries;

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
    public async Task<ActionResult<List<RoadmapListDto>>> GetRoadmaps()
    {
        var roadmaps = await _sender.Send(new GetRoadmapsQuery());
        return Ok(roadmaps);
    }

    [HttpGet("{idOrKey}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Roadmaps)]
    [OpenApiOperation("Get roadmap details.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType(typeof(ErrorResult))]
    public async Task<ActionResult<RoadmapDetailsDto>> GetRoadmap(string idOrKey)
    {
        var roadmap = await _sender.Send(new GetRoadmapQuery(idOrKey));

        return roadmap is not null
            ? Ok(roadmap)
            : NotFound();
    }
}
