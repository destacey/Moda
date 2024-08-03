using CSharpFunctionalExtensions;
using Moda.AppIntegration.Application.Interfaces;
using Moda.AppIntegration.Domain.Models;
using Moda.Common.Application.Interfaces;
using Moda.Web.Api.Models.AppIntegrations.Connections;

namespace Moda.Web.Api.Controllers.AppIntegrations;

[Route("api/app-integrations/azure-devops-boards-connections")]
[ApiVersionNeutral]
[ApiController]
public class AzureDevOpsBoardsConnectionsController : ControllerBase
{
    private readonly ILogger<AzureDevOpsBoardsConnectionsController> _logger;
    private readonly ISender _sender;
    private readonly Connector _connector = Connector.AzureDevOpsBoards;

    public AzureDevOpsBoardsConnectionsController(ILogger<AzureDevOpsBoardsConnectionsController> logger, ISender sender)
    {
        _logger = logger;
        _sender = sender;
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Connections)]
    [OpenApiOperation("Get a list of all Azure DevOps Boards connections.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ConnectionListDto>>> GetList(CancellationToken cancellationToken, bool includeDisabled = false)
    {
        var connections = await _sender.Send(new GetConnectionsQuery(includeDisabled, _connector), cancellationToken);
        return Ok(connections);
    }

    [HttpGet("{id}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Connections)]
    [OpenApiOperation("Get Azure DevOps Boards connection based on id.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AzureDevOpsBoardsConnectionDetailsDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var connection = await _sender.Send(new GetAzureDevOpsBoardsConnectionQuery(id), cancellationToken);

        connection?.Configuration?.MaskPersonalAccessToken();

        return connection is not null
            ? Ok(connection)
            : NotFound();
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.Connections)]
    [OpenApiOperation("Create an Azure DevOps Boards connection.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.CreateReturn201Guid))]
    public async Task<ActionResult> Create(CreateAzureDevOpsBoardConnectionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToCreateAzureDevOpsBoardsConnectionCommand(), cancellationToken);

        if (result.IsFailure)
        {
            var error = new ErrorResult
            {
                StatusCode = 400,
                SupportMessage = result.Error,
                Source = "AzureDevOpsBoardsConnectionsController.Update"
            };
            return BadRequest(error);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    [HttpPut("{id}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Connections)]
    [OpenApiOperation("Update an Azure DevOps Boards connection.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Update(Guid id, UpdateAzureDevOpsBoardConnectionRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest();

        var result = await _sender.Send(request.ToUpadateAzureDevOpsBoardsConnectionCommand(), cancellationToken);

        if (result.IsFailure)
        {
            var error = new ErrorResult
            {
                StatusCode = 400,
                SupportMessage = result.Error,
                Source = "AzureDevOpsBoardsConnectionsController.Update"
            };
            return BadRequest(error);
        }

        return NoContent();
    }

    [HttpPost("{id}/sync-state")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Connections)]
    [OpenApiOperation("Update an Azure DevOps Boards connection sync state.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> UpdateSyncState(Guid id, bool isSyncEnabled, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpdateAzureDevOpsBoardsConnectionSyncStateCommand(id, isSyncEnabled), cancellationToken);

        if (result.IsFailure)
        {
            var error = new ErrorResult
            {
                StatusCode = 400,
                SupportMessage = result.Error,
                Source = "AzureDevOpsBoardsConnectionsController.Update"
            };
            return BadRequest(error);
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [MustHavePermission(ApplicationAction.Delete, ApplicationResource.Connections)]
    [OpenApiOperation("Delete an Azure DevOps Boards connection.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteAzureDevOpsBoardsConnectionCommand(id), cancellationToken);

        if (result.IsFailure)
        {
            var error = new ErrorResult
            {
                StatusCode = 400,
                SupportMessage = result.Error,
                Source = "AzureDevOpsBoardsConnectionsController.Delete"
            };
            return BadRequest(error);
        }

        return NoContent();
    }

    [HttpGet("{id}/teams")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Connections)]
    [OpenApiOperation("Get Azure DevOps connection teams based on id.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<AzureDevOpsBoardsWorkspaceTeamDto>>> GetConnectionTeams(Guid id, Guid? workspaceId, CancellationToken cancellationToken)
    {
        var teams = await _sender.Send(new GetAzureDevOpsBoardsConnectionTeamsQuery(id, workspaceId), cancellationToken);

        return teams;
    }

    [HttpPost("{id}/teams")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Connections)]
    [OpenApiOperation("Update Azure DevOps connection team mappings.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> MapConnectionTeams(Guid id, [FromBody] AzdoConnectionTeamMappingsRequest request, CancellationToken cancellationToken)
    {
        if (id != request.ConnectionId)
            return BadRequest();

        var result = await _sender.Send(request.ToUpdateAzureDevOpsConnectionTeamMappingsCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(ErrorResult.CreateBadRequest(result.Error, "AzureDevOpsBoardsConnectionsController.MapConnectionTeams"));
    }

    [HttpPost("test")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Connections)]
    [OpenApiOperation("Test Azure DevOps Boards connection configuration.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> TestConfig(TestAzureDevOpsBoardConnectionRequest request, [FromServices] IAzureDevOpsService azureDevOpsService)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Organization) || string.IsNullOrWhiteSpace(request.PersonalAccessToken))
        {
            var error = new ErrorResult
            {
                StatusCode = 400,
                SupportMessage = "The Organization and PersonalAccessToken values are required to test.",
                Source = "AzureDevOpsBoardsConnectionsController.Test"
            };
            return BadRequest(error);
        }

        var config = new AzureDevOpsBoardsConnectionConfiguration(request.Organization, request.PersonalAccessToken);

        var result = await azureDevOpsService.TestConnection(config.OrganizationUrl, config.PersonalAccessToken);
        if (result.IsFailure)
        {
            var error = new ErrorResult
            {
                StatusCode = 400,
                SupportMessage = result.Error,
                Source = "AzureDevOpsBoardsConnectionsController.Test"
            };
            return BadRequest(error);
        }

        return NoContent();
    }

    [HttpPost("{id}/sync-organization-configuration")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Connections)]
    [OpenApiOperation("Sync Azure DevOps processes and projects.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> SyncOrganizationConfiguration(Guid id, [FromServices] IAzureDevOpsBoardsInitManager azureDevOpsBoardsInitManager, CancellationToken cancellationToken)
    {
        var result = await azureDevOpsBoardsInitManager.SyncOrganizationConfiguration(id, cancellationToken);
        if (result.IsFailure)
        {
            var error = new ErrorResult
            {
                StatusCode = 400,
                SupportMessage = result.Error,
                Source = "AzureDevOpsBoardsConnectionsController.SyncOrganizationConfiguration"
            };
            return BadRequest(error);
        }

        return NoContent();
    }

    [HttpPost("{id}/init-work-process-integration")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Connections)]
    [OpenApiOperation("Initialize Azure DevOps project integration as a Moda workspace.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> InitWorkProcesssIntegration(Guid id, InitWorkProcessIntegrationRequest request, [FromServices] IAzureDevOpsBoardsInitManager azureDevOpsBoardsImportService, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest();

        var result = await azureDevOpsBoardsImportService.InitWorkProcessIntegration(request.Id, request.ExternalId, cancellationToken);
        if (result.IsFailure)
        {
            var error = new ErrorResult
            {
                StatusCode = 400,
                SupportMessage = result.Error,
                Source = "AzureDevOpsBoardsConnectionsController.InitWorkProcesssIntegration"
            };
            return BadRequest(error);
        }

        return NoContent();
    }

    [HttpPost("{id}/init-workspace-integration")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Connections)]
    [OpenApiOperation("Initialize Azure DevOps project integration as a Moda workspace.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> InitWorkspaceIntegration(Guid id, InitWorkspaceIntegrationRequest request, [FromServices] IAzureDevOpsBoardsInitManager azureDevOpsBoardsImportService, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest();

        var result = await azureDevOpsBoardsImportService.InitWorkspaceIntegration(request.Id, request.ExternalId, request.WorkspaceKey, request.WorkspaceName, request.ExternalViewWorkItemUrlTemplate, cancellationToken);
        if (result.IsFailure)
        {
            var error = new ErrorResult
            {
                StatusCode = 400,
                SupportMessage = result.Error,
                Source = "AzureDevOpsBoardsConnectionsController.InitWorkspaceIntegration"
            };
            return BadRequest(error);
        }

        return NoContent();
    }
}
