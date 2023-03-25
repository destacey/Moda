using Moda.Work.Application.WorkStateCategories.Dtos;
using Moda.Work.Application.WorkStateCategories.Queries;

namespace Moda.Web.Api.Controllers.Work;

[Route("api/work/work-state-categories")]
[ApiVersionNeutral]
[ApiController]
public class WorkStateCategoriesController : ControllerBase
{
    private readonly ISender _sender;

    public WorkStateCategoriesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.WorkStateCategories)]
    [OpenApiOperation("Get a list of all work state categories.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WorkStateCategoryListDto>>> GetList(CancellationToken cancellationToken)
    {
        var categories = await _sender.Send(new GetWorkStateCategoriesQuery(), cancellationToken);
        return Ok(categories.OrderBy(c => c.Order));
    }
}
