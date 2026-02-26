using Microsoft.AspNetCore.OData.Query;
using Moda.Analytics.Application.AnalyticsViews.Commands;
using Moda.Analytics.Application.AnalyticsViews.Dtos;
using Moda.Analytics.Application.AnalyticsViews.Queries;
using Moda.Web.Api.Extensions;
using Moda.Web.Api.Models.Analytics.Views;

namespace Moda.Web.Api.Controllers.Analytics;

[Route("api/analytics/views")]
[ApiVersionNeutral]
[ApiController]
public class AnalyticsViewsController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.AnalyticsViews)]
    [OpenApiOperation("Get analytics views visible to the current user.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AnalyticsViewListDto>>> GetList(
        CancellationToken cancellationToken,
        bool includeInactive = false)
    {
        var views = await _sender.Send(new GetAnalyticsViewsQuery(includeInactive), cancellationToken);
        return Ok(views);
    }

    [HttpGet("{id}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.AnalyticsViews)]
    [OpenApiOperation("Get an analytics view by id.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AnalyticsViewDetailsDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetAnalyticsViewQuery(id), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound();
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.AnalyticsViews)]
    [OpenApiOperation("Create an analytics view.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.CreateReturn201Guid))]
    public async Task<ActionResult> Create([FromBody] CreateAnalyticsViewRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToCreateAnalyticsViewCommand(), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPut("{id}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.AnalyticsViews)]
    [OpenApiOperation("Update an analytics view.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<AnalyticsViewDetailsDto>> Update(Guid id, [FromBody] UpdateAnalyticsViewRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(HttpContext));

        var result = await _sender.Send(request.ToUpdateAnalyticsViewCommand(), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpDelete("{id}")]
    [MustHavePermission(ApplicationAction.Delete, ApplicationResource.AnalyticsViews)]
    [OpenApiOperation("Delete an analytics view.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteAnalyticsViewCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpGet("{id}/data")]
    [MustHavePermission(ApplicationAction.Run, ApplicationResource.AnalyticsViews)]
    [OpenApiOperation("Get analytics view data.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AnalyticsViewDataResultDto>> GetData(
        Guid id,
        CancellationToken cancellationToken,
        int pageNumber = 1,
        int pageSize = 50)
    {
        var result = await _sender.Send(
            new GetWorkItemAnalyticsQuery(id, pageNumber, pageSize),
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

}
