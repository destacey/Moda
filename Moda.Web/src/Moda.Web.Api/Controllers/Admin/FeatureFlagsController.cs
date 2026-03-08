using Moda.Common.Application.FeatureManagement.Commands;
using Moda.Common.Application.FeatureManagement.Dtos;
using Moda.Common.Application.FeatureManagement.Queries;
using Moda.Web.Api.Extensions;
using Moda.Web.Api.Models.Admin;

namespace Moda.Web.Api.Controllers.Admin;

[Route("api/admin/feature-flags")]
[ApiVersionNeutral]
[ApiController]
public class FeatureFlagsController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.FeatureFlags)]
    [OpenApiOperation("Get a list of all feature flags.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<FeatureFlagListDto>>> GetFeatureFlags(CancellationToken cancellationToken, [FromQuery] bool includeArchived = false)
    {
        var flags = await _sender.Send(new GetFeatureFlagsQuery(includeArchived), cancellationToken);
        return Ok(flags);
    }

    [HttpGet("{id:int}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.FeatureFlags)]
    [OpenApiOperation("Get feature flag details.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FeatureFlagDto>> GetFeatureFlag(int id, CancellationToken cancellationToken)
    {
        var flag = await _sender.Send(new GetFeatureFlagQuery(id), cancellationToken);
        return flag is not null
            ? Ok(flag)
            : NotFound();
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.FeatureFlags)]
    [OpenApiOperation("Create a feature flag.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.CreateReturn201Int))]
    public async Task<ActionResult<int>> Create([FromBody] CreateFeatureFlagRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToCreateFeatureFlagCommand(), cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetFeatureFlag), new { id = result.Value }, result.Value)
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPut("{id:int}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.FeatureFlags)]
    [OpenApiOperation("Update a feature flag.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateFeatureFlagRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(nameof(id), nameof(request.Id), HttpContext));

        var result = await _sender.Send(request.ToUpdateFeatureFlagCommand(), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPut("{id:int}/toggle")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.FeatureFlags)]
    [OpenApiOperation("Toggle a feature flag on or off.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Toggle(int id, [FromBody] ToggleFeatureFlagRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(nameof(id), nameof(request.Id), HttpContext));

        var result = await _sender.Send(new ToggleFeatureFlagCommand(id, request.IsEnabled), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPut("{id:int}/archive")]
    [MustHavePermission(ApplicationAction.Delete, ApplicationResource.FeatureFlags)]
    [OpenApiOperation("Archive a feature flag.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Archive(int id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ArchiveFeatureFlagCommand(id), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }
}
