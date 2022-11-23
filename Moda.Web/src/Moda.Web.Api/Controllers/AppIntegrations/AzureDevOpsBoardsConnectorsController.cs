using MediatR;

namespace Moda.Web.Api.Controllers.AppIntegrations;

public class AzureDevOpsBoardsConnectorsController : VersionNeutralApiController
{
    private readonly ILogger<AzureDevOpsBoardsConnectorsController> _logger;
    private readonly ISender _sender;

    public AzureDevOpsBoardsConnectorsController(ILogger<AzureDevOpsBoardsConnectorsController> logger, ISender sender)
    {
        _logger = logger;
        _sender = sender;
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Connectors)]
    [OpenApiOperation("Get a list of all Azure DevOps Boards connectors.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ConnectorListDto>>> GetList(CancellationToken cancellationToken, bool includeDisabled = false)
    {
        var connectors = await _sender.Send(new GetConnectorsQuery(includeDisabled, ConnectorType.AzureDevOpsBoards), cancellationToken);
        return Ok(connectors.OrderBy(c => c.Name));
    }

    [HttpGet("{id}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Connectors)]
    [OpenApiOperation("Get Azure DevOps Boards connector based on id.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ConnectorListDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var connector = await _sender.Send(new GetConnectorQuery(id), cancellationToken);
        
        return connector is not null
            ? Ok(connector)
            : NotFound();
    }

    [HttpGet("{id}/config")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Connectors)]
    [OpenApiOperation("Get Azure DevOps Boards connector based on id.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AzureDevOpsBoardsConnectorConfigurationDto>> GetConfigById(Guid id, CancellationToken cancellationToken)
    {
        var config = await _sender.Send(new GetAzureDevOpsBoardsConnectorConfigurationQuery(id), cancellationToken);

        return config is not null
            ? Ok(config)
            : NotFound();
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.Connectors)]
    [OpenApiOperation("Create an Azure DevOps Boards connector.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.Create))]
    public async Task<ActionResult> Create(CreateAzureDevOpsBoardConnectorRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToCreateConnectorCommand(), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpPut("{id}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Connectors)]
    [OpenApiOperation("Update an Azure DevOps Boards connector.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Update(Guid id, UpdateAzureDevOpsBoardConnectorRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest();

        var result = await _sender.Send(request.ToUpadateAzureDevOpsBoardsConnectorCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.Error);
    }

    [HttpPut("{id}/config")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Connectors)]
    [OpenApiOperation("Update an Azure DevOps Boards connector.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateConfig(Guid id, UpdateAzureDevOpsBoardConnectorConfigurationRequest request, CancellationToken cancellationToken)
    {
        if (id != request.ConnectorId)
            return BadRequest();

        var result = await _sender.Send(request.ToUpdateAzureDevOpsBoardsConnectorConfigurationCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.Error);
    }
}
