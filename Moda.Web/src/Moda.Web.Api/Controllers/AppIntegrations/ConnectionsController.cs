using CSharpFunctionalExtensions;
using Moda.AppIntegration.Application.Connections.Commands.AzureDevOps;
using Moda.AppIntegration.Application.Connections.Commands.AzureOpenAI;
using Moda.AppIntegration.Application.Connections.Dtos.AzureDevOps;
using Moda.AppIntegration.Application.Connections.Dtos.AzureOpenAI;
using Moda.Web.Api.Extensions;
using Moda.Web.Api.Models.AppIntegrations.Connections;

namespace Moda.Web.Api.Controllers.AppIntegrations;

[Route("api/app-integrations/connections")]
[ApiVersionNeutral]
[ApiController]
public class ConnectionsController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Connections)]
    [OpenApiOperation("Get list of all connections.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ConnectionListDto>>> GetConnections(
        CancellationToken cancellationToken,
        bool includeDisabled = false)
    {
        var connections = await _sender.Send(new GetConnectionsQuery(includeDisabled), cancellationToken);
        return Ok(connections);
    }

    [HttpGet("connectors")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Connectors)]
    [OpenApiOperation("Get list of all connector types.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ConnectorListDto>>> GetConnectors(CancellationToken cancellationToken)
    {
        var connectors = await _sender.Send(new GetConnectorsQuery(), cancellationToken);
        return Ok(connectors.OrderBy(c => c.Name));
    }

    [HttpGet("{id}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Connections)]
    [OpenApiOperation("Get connection details by ID.", "Returns polymorphic response based on connector type.")]
    [ProducesResponseType(typeof(ConnectionDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConnectionDetailsDto>> GetConnection(Guid id, CancellationToken cancellationToken)
    {
        var connection = await _sender.Send(new GetConnectionQuery(id), cancellationToken);

        if (connection is null)
            return NotFound();

        // Mask sensitive fields
        if (connection is AzureDevOpsConnectionDetailsDto azdo)
        {
            azdo.Configuration.MaskPersonalAccessToken();
        }
        else if (connection is AzureOpenAIConnectionDetailsDto aoai)
        {
            aoai.Configuration.ApiKey = "***MASKED***";
        }

        return this.OkPolymorphic(connection);
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.Connections)]
    [OpenApiOperation("Create a new connection.", "Accepts polymorphic request based on connector type.")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.CreateReturn201Guid))]
    public async Task<ActionResult> CreateConnection(
        [FromBody] CreateConnectionRequest request,
        CancellationToken cancellationToken)
    {
        Result<Guid> result = request switch
        {
            CreateAzureDevOpsConnectionRequest azdo =>
                await _sender.Send(azdo.ToCommand(), cancellationToken),
            CreateAzureOpenAIConnectionRequest aoai =>
                await _sender.Send(aoai.ToCommand(), cancellationToken),
            _ => Result.Failure<Guid>($"Connector type not supported")
        };

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetConnection), new { id = result.Value }, result.Value)
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPut("{id}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Connections)]
    [OpenApiOperation("Update a connection.", "Accepts polymorphic request based on connector type.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateConnection(
        Guid id,
        [FromBody] UpdateConnectionRequest request,
        CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(HttpContext));

        Result result = request switch
        {
            UpdateAzureDevOpsConnectionRequest azdo =>
                await _sender.Send(azdo.ToCommand(), cancellationToken),
            UpdateAzureOpenAIConnectionRequest aoai =>
                await _sender.Send(aoai.ToCommand(), cancellationToken),
            _ => Result.Failure($"Connector type not supported")
        };

        return result.IsSuccess ? NoContent() : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpDelete("{id}")]
    [MustHavePermission(ApplicationAction.Delete, ApplicationResource.Connections)]
    [OpenApiOperation("Delete a connection.", "Works for all connector types.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteConnection(Guid id, CancellationToken cancellationToken)
    {
        // Determine connection type first to dispatch correct command
        var connection = await _sender.Send(new GetConnectionQuery(id), cancellationToken);
        if (connection is null)
            return NotFound();

        Result result = connection switch
        {
            AzureDevOpsConnectionDetailsDto =>
                await _sender.Send(new DeleteAzureDevOpsConnectionCommand(id), cancellationToken),
            AzureOpenAIConnectionDetailsDto =>
                await _sender.Send(new DeleteAzureOpenAIConnectionCommand(id), cancellationToken),
            _ => Result.Failure($"Connector type not supported")
        };

        return result.IsSuccess ? NoContent() : BadRequest(result.ToBadRequestObject(HttpContext));
    }
}
