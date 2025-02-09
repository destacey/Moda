using Moda.ProjectPortfolioManagement.Application.Portfolios.Dtos;
using Moda.ProjectPortfolioManagement.Application.Portfolios.Queries;
using Moda.ProjectPortfolioManagement.Domain.Enums;

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
    public async Task<ActionResult<IEnumerable<ProjectPortfolioListDto>>> GetProjectPortfolios(CancellationToken cancellationToken, [FromQuery] int? status = null)
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
    public async Task<ActionResult<ProjectPortfolioDetailsDto>> GetProjectPortfolio(string idOrKey, CancellationToken cancellationToken)
    {
        var portfolio = await _sender.Send(new GetProjectPortfolioQuery(idOrKey), cancellationToken);

        return portfolio is not null
            ? Ok(portfolio)
            : NotFound();
    }
}
