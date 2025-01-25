using Moda.Common.Application.Models;
using Moda.Common.Domain.Enums.StrategicManagement;
using Moda.StrategicManagement.Application.StrategicThemes.Commands;
using Moda.StrategicManagement.Application.StrategicThemes.Dtos;
using Moda.StrategicManagement.Application.StrategicThemes.Queries;
using Moda.Web.Api.Extensions;
using Moda.Web.Api.Models.StrategicManagement.StrategicThemes;

namespace Moda.Web.Api.Controllers.StrategicManagement;

[Route("api/strategic-management/strategic-themes")]
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
    public async Task<ActionResult<IEnumerable<StrategicThemeListDto>>> GetStrategicThemes(CancellationToken cancellationToken, [FromQuery] int? state = null)
    {
        StrategicThemeState? filter = state.HasValue ? (StrategicThemeState)state.Value : null;

        var themes = await _sender.Send(new GetStrategicThemesQuery(filter), cancellationToken);

        return Ok(themes);
    }

    [HttpGet("{idOrKey}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.StrategicThemes)]
    [OpenApiOperation("Get strategic themes details.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StrategicThemeDetailsDto>> GetStrategicTheme(string idOrKey, CancellationToken cancellationToken)
    {
        var theme = await _sender.Send(new GetStrategicThemeQuery(idOrKey), cancellationToken);

        return theme is not null
            ? Ok(theme)
            : NotFound();
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.StrategicThemes)]
    [OpenApiOperation("Create a strategic theme.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.CreateReturn201IdAndKey))]
    public async Task<ActionResult<ObjectIdAndKey>> Create([FromBody] CreateStrategicThemeRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToCreateStrategicThemeCommand(), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetStrategicTheme), new { idOrKey = result.Value.Id.ToString() }, result.Value)
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPut("{id}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.StrategicThemes)]
    [OpenApiOperation("Update a strategic theme.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateStrategicThemeRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(HttpContext));

        var result = await _sender.Send(request.ToUpdateStrategicThemeCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpDelete("{id}")]
    [MustHavePermission(ApplicationAction.Delete, ApplicationResource.StrategicThemes)]
    [OpenApiOperation("Delete a strategic theme.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteStrategicThemeCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpGet("states")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.StrategicThemes)]
    [OpenApiOperation("Get a list of all strategic theme states.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<StrategicThemeStateDto>>> GetStateOptions(CancellationToken cancellationToken)
    {
        var items = await _sender.Send(new GetStrategicThemeStatesQuery(), cancellationToken);
        return Ok(items.OrderBy(s => s.Order));
    }
}
