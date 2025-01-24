using Moda.Common.Application.Models;
using Moda.StrategicManagement.Application.StrategicThemes.Commands;
using Moda.StrategicManagement.Application.StrategicThemes.Queries;
using Moda.StrategicManagement.Application.Strategies.Commands;
using Moda.StrategicManagement.Application.Strategies.Dtos;
using Moda.StrategicManagement.Application.Strategies.Queries;
using Moda.StrategicManagement.Domain.Enums;
using Moda.Web.Api.Extensions;
using Moda.Web.Api.Models.StrategicManagement.Strategies;

namespace Moda.Web.Api.Controllers.StrategicManagement;

[Route("api/strategic-management/[controller]")]
[ApiVersionNeutral]
[ApiController]
public class StrategiesController : ControllerBase
{
    private readonly ILogger<StrategiesController> _logger;
    private readonly ISender _sender;

    public StrategiesController(ILogger<StrategiesController> logger, ISender sender)
    {
        _logger = logger;
        _sender = sender;
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Strategies)]
    [OpenApiOperation("Get a list of strategies.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<StrategyListDto>>> GetStrategies(CancellationToken cancellationToken, [FromQuery] int? status = null)
    {
        StrategyStatus? filter = status.HasValue ? (StrategyStatus)status.Value : null;

        var strategies = await _sender.Send(new GetStrategiesQuery(filter), cancellationToken);

        return Ok(strategies);
    }

    [HttpGet("{idOrKey}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Strategies)]
    [OpenApiOperation("Get strategy details.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StrategyDetailsDto>> GetStrategy(string idOrKey, CancellationToken cancellationToken)
    {
        var strategy = await _sender.Send(new GetStrategyQuery(idOrKey), cancellationToken);

        return strategy is not null
            ? Ok(strategy)
            : NotFound();
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.Strategies)]
    [OpenApiOperation("Create a strategy.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.CreateReturn201IdAndKey))]
    public async Task<ActionResult<ObjectIdAndKey>> Create([FromBody] CreateStrategyRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToCreateStrategyCommand(), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetStrategy), new { idOrKey = result.Value.Id.ToString() }, result.Value)
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPut("{id}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Strategies)]
    [OpenApiOperation("Update a strategy.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStrategyRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(HttpContext));

        var result = await _sender.Send(request.ToUpdateStrategyCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpDelete("{id}")]
    [MustHavePermission(ApplicationAction.Delete, ApplicationResource.Strategies)]
    [OpenApiOperation("Delete a strategy.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteStrategyCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpGet("statuses")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Strategies)]
    [OpenApiOperation("Get a list of all strategy statuses.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<StrategyStatusDto>>> GetStatusOptions(CancellationToken cancellationToken)
    {
        var items = await _sender.Send(new GetStrategyStatusesQuery(), cancellationToken);
        return Ok(items.OrderBy(s => s.Order));
    }
}
