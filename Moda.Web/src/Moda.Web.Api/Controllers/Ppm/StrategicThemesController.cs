using Moda.Common.Domain.Enums.StrategicManagement;
using Moda.ProjectPortfolioManagement.Application.StrategicThemes.Dtos;
using Moda.ProjectPortfolioManagement.Application.StrategicThemes.Queries;

namespace Moda.Web.Api.Controllers.Ppm;

[Route("api/ppm/[controller]")]
[ApiVersionNeutral]
[ApiController]
public class StrategicThemesController(ILogger<StrategicThemesController> logger, ISender sender)
    : ControllerBase
{
    private readonly ILogger<StrategicThemesController> _logger = logger;
    private readonly ISender _sender = sender;

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.PpmStrategicThemes)]
    [OpenApiOperation("PpmStrategicThemes_GetStrategicThemes", "Get a list of strategic themes.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<PpmStrategicThemeListDto>>> GetStrategicThemes(CancellationToken cancellationToken, [FromQuery] int? state = null)
    {
        StrategicThemeState? filter = state.HasValue ? (StrategicThemeState)state.Value : null;

        var themes = await _sender.Send(new GetStrategicThemesQuery(filter), cancellationToken);

        return Ok(themes);
    }

    [HttpGet("{idOrKey}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.PpmStrategicThemes)]
    [OpenApiOperation("PpmStrategicThemes_GetStrategicTheme", "Get strategic themes details.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PpmStrategicThemeDetailsDto>> GetStrategicTheme(string idOrKey, CancellationToken cancellationToken)
    {
        var theme = await _sender.Send(new GetStrategicThemeQuery(idOrKey), cancellationToken);

        return theme is not null
            ? Ok(theme)
            : NotFound();
    }
}
