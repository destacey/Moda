using Moda.Links.Commands;
using Moda.Links.Models;
using Moda.Links.Queries;
using Moda.Web.Api.Models.Links;
using Moda.Web.Api.Models.Planning.ProgramIncrements;

namespace Moda.Web.Api.Controllers.Links;

[Route("api/[controller]")]
[ApiVersionNeutral]
[ApiController]
public class LinksController : ControllerBase
{
    private readonly ILogger<LinksController> _logger;
    private readonly ISender _sender;

    public LinksController(ILogger<LinksController> logger, ISender sender)
    {
        _logger = logger;
        _sender = sender;
    }

    [HttpGet("{objectId}/list")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Links)]
    [OpenApiOperation("Get a list of links for a specific objectId.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<LinkDto>>> GetList(Guid objectId, CancellationToken cancellationToken)
    {
        var links = await _sender.Send(new GetLinksQuery(objectId), cancellationToken);
        return Ok(links);
    }

    [HttpGet("{id}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Links)]
    [OpenApiOperation("Get a link by id.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType(typeof(ErrorResult))]
    public async Task<ActionResult<LinkDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var link = await _sender.Send(new GetLinkQuery(id), cancellationToken);
        return link is not null
            ? Ok(link)
            : NotFound();
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.Links)]
    [OpenApiOperation("Create a link.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.CreateReturn201Guid))]
    public async Task<ActionResult> Create([FromBody] CreateLinkRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToCreateLinkCommand(), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpPut("{id}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Links)]
    [OpenApiOperation("Update a link.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateLinkRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest();

        var result = await _sender.Send(request.ToUpdateLinkCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.Error);
    }

    [HttpDelete("{id}")]
    [MustHavePermission(ApplicationAction.Delete, ApplicationResource.Links)]
    [OpenApiOperation("Delete a link.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteLinkCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.Error);
    }
}
