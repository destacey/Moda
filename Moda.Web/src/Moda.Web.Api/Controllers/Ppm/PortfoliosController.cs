using Moda.Common.Application.Models;
using Moda.ProjectPortfolioManagement.Application.Portfolios.Command;
using Moda.ProjectPortfolioManagement.Application.Portfolios.Dtos;
using Moda.ProjectPortfolioManagement.Application.Portfolios.Queries;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.Web.Api.Extensions;
using Moda.Web.Api.Models.Ppm.Portfolios;

namespace Moda.Web.Api.Controllers.Ppm;

[Route("api/ppm/[controller]")]
[ApiVersionNeutral]
[ApiController]
public class PortfoliosController(ILogger<PortfoliosController> logger, ISender sender) 
    : ControllerBase
{
    private readonly ILogger<PortfoliosController> _logger = logger;
    private readonly ISender _sender = sender;

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.ProjectPortfolios)]
    [OpenApiOperation("Get a list of project portfolios.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ProjectPortfolioListDto>>> GetPortfolios(CancellationToken cancellationToken, [FromQuery] int? status = null)
    {
        ProjectPortfolioStatus? filter = status.HasValue ? (ProjectPortfolioStatus)status.Value : null;

        var portfolios = await _sender.Send(new GetProjectPortfoliosQuery(filter), cancellationToken);

        return Ok(portfolios);
    }

    [HttpGet("{idOrKey}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.ProjectPortfolios)]
    [OpenApiOperation("Get project portfolio details.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectPortfolioDetailsDto>> GetPortfolio(string idOrKey, CancellationToken cancellationToken)
    {
        var portfolio = await _sender.Send(new GetProjectPortfolioQuery(idOrKey), cancellationToken);

        return portfolio is not null
            ? Ok(portfolio)
            : NotFound();
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.ProjectPortfolios)]
    [OpenApiOperation("Create a portfolio.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.CreateReturn201IdAndKey))]
    public async Task<ActionResult<ObjectIdAndKey>> Create([FromBody] CreatePortfolioRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToCreateProjectPortfolioCommand(), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetPortfolio), new { idOrKey = result.Value.Id.ToString() }, result.Value)
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPut("{id}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.ProjectPortfolios)]
    [OpenApiOperation("Update a portfolio.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdatePortfolioRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(HttpContext));

        var result = await _sender.Send(request.ToUpdateProjectPortfolioCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPost("{id}/activate")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.ProjectPortfolios)]
    [OpenApiOperation("Activate a project portfolio.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ActivateProjectPortfolioCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPost("{id}/close")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.ProjectPortfolios)]
    [OpenApiOperation("Close a project portfolio.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Close(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CloseProjectPortfolioCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPost("{id}/archive")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.ProjectPortfolios)]
    [OpenApiOperation("Archive a project portfolio.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Archive(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ArchiveProjectPortfolioCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpDelete("{id}")]
    [MustHavePermission(ApplicationAction.Delete, ApplicationResource.ProjectPortfolios)]
    [OpenApiOperation("Delete a portfolio.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteProjectPortfolioCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }
}
