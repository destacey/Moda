using Microsoft.AspNetCore.Authorization;
using Moda.Common.Application.Search;
using Moda.Common.Application.Search.Dtos;
using Moda.Web.Api.Extensions;

namespace Moda.Web.Api.Controllers;

[Route("api/search")]
[ApiVersionNeutral]
[ApiController]
[Authorize]
public class SearchController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;

    [HttpGet]
    [OpenApiOperation("Global search across all modules.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GlobalSearchResultDto>> Search(
        [FromQuery] string query, CancellationToken cancellationToken, [FromQuery] int maxResultsPerCategory = 5)
    {
        var result = await _sender.Send(
            new GlobalSearchQuery(query, maxResultsPerCategory), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }
}
