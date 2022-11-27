using MediatR;
using Moda.Work.Application.WorkStateCategories.Queries;

namespace Moda.Web.Api.Controllers.Work;

public class WorkStateCategoriesController : VersionNeutralApiController
{
    private readonly ILogger<WorkStateCategoriesController> _logger;
    private readonly ISender _sender;

    public WorkStateCategoriesController(ILogger<WorkStateCategoriesController> logger, ISender sender)
    {
        _logger = logger;
        _sender = sender;
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.WorkStateCategories)]
    [OpenApiOperation("Get a list of all work state categories.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ConnectorListDto>>> GetList(CancellationToken cancellationToken)
    {
        var categories = await _sender.Send(new GetWorkStateCategoriesQuery(), cancellationToken);
        return Ok(categories.OrderBy(c => c.Order));
    }
}
