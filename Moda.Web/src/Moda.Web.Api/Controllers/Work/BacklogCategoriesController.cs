using Moda.Work.Application.BacklogCategories.Dtos;
using Moda.Work.Application.BacklogCategories.Queries;

namespace Moda.Web.Api.Controllers.Work;

[Route("api/work/backlog-categories")]
[ApiVersionNeutral]
[ApiController]
public class BacklogCategoriesController : ControllerBase
{
    private readonly ISender _sender;

    public BacklogCategoriesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.BacklogCategories)]
    [OpenApiOperation("Get a list of all backlog categories.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<BacklogCategoryDto>>> GetList(CancellationToken cancellationToken)
    {
        var categories = await _sender.Send(new GetBacklogCategoriesQuery(), cancellationToken);
        return Ok(categories.OrderBy(c => c.Order));
    }
}
