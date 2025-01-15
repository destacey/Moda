using Moda.Planning.Application.Roadmaps.Dtos;
using Moda.StrategicManagement.Application.StrategicThemes.Queries;
using Moda.StrategicManagement.Domain.Enums;

namespace Moda.Web.Api.Controllers.StrategicManagement;

[Route("api/strategic-management/[controller]")]
[ApiVersionNeutral]
[ApiController]
public class StrategicThemesController : ControllerBase
{
    private readonly ILogger<StrategicThemesController> _logger;
    private readonly ISender _sender;

    public StrategicThemesController(ILogger<StrategicThemesController> logger, ISender sender)
    {
        _logger = logger;
        _sender = sender;
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.StrategicThemes)]
    [OpenApiOperation("Get a list of strategic themes.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<RoadmapListDto>>> GetStrategicThemes(int? state, CancellationToken cancellationToken)
    {
        StrategicThemeState? filter = state.HasValue ? (StrategicThemeState)state.Value : null;

        var themes = await _sender.Send(new GetStrategicThemesQuery(filter), cancellationToken);

        return Ok(themes);
    }
}
