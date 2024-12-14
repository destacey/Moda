namespace Moda.Web.Api.Controllers.AppIntegrations;

[Route("api/app-integrations/connectors")]
[ApiVersionNeutral]
[ApiController]
public class ConnectorsController : ControllerBase
{
    private readonly ILogger<ConnectorsController> _logger;
    private readonly ISender _sender;

    public ConnectorsController(ILogger<ConnectorsController> logger, ISender sender)
    {
        _logger = logger;
        _sender = sender;
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Connectors)]
    [OpenApiOperation("Get a list of all connectors.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ConnectorListDto>>> GetList(CancellationToken cancellationToken)
    {
        var connectors = await _sender.Send(new GetConnectorsQuery(), cancellationToken);
        return Ok(connectors.OrderBy(c => c.Name));
    }
}
