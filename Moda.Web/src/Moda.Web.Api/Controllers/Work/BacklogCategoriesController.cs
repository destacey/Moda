using MediatR;
using Moda.Work.Application.BacklogCategories.Queries;

namespace Moda.Web.Api.Controllers.Work;

public class BacklogCategoriesController : VersionNeutralApiController
{
    private readonly ILogger<BacklogCategoriesController> _logger;
    private readonly ISender _sender;

    public BacklogCategoriesController(ILogger<BacklogCategoriesController> logger, ISender sender)
    {
        _logger = logger;
        _sender = sender;
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.BacklogCategories)]
    [OpenApiOperation("Get a list of all backlog categories.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ConnectorListDto>>> GetList(CancellationToken cancellationToken)
    {
        var categories = await _sender.Send(new GetBacklogCategoriesQuery(), cancellationToken);
        return Ok(categories.OrderBy(c => c.Order));
    }
}
