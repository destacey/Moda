using Moda.Work.Application.WorkStatusCategories.Dtos;
using Moda.Work.Application.WorkStatusCategories.Queries;

namespace Moda.Web.Api.Controllers.Work;

[Route("api/work/work-status-categories")]
[ApiVersionNeutral]
[ApiController]
public class WorkStatusCategoriesController : ControllerBase
{
    private readonly ISender _sender;

    public WorkStatusCategoriesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.WorkStatusCategories)]
    [OpenApiOperation("Get a list of all work status categories.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<WorkStatusCategoryListDto>>> GetList(CancellationToken cancellationToken)
    {
        var categories = await _sender.Send(new GetWorkStatusCategoriesQuery(), cancellationToken);
        return Ok(categories.OrderBy(c => c.Order));
    }
}
