using MediatR;

namespace Moda.Web.Api.Controllers.AppIntegrations;

public class ConnectorsController : VersionNeutralApiController
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
    public async Task<ActionResult<IReadOnlyList<ConnectorListDto>>> GetList(CancellationToken cancellationToken)
    {
        var connectors = await _sender.Send(new GetConnectorsQuery(), cancellationToken);
        return Ok(connectors.OrderBy(c => c.Name));
    }
}
