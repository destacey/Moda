namespace Moda.AppIntegration.Application.Interfaces;
public interface IAzureDevOpsBoardsInitManager : ITransientService
{
    Task<Result> SyncOrganizationConfiguration(Guid connectionId, CancellationToken cancellationToken);
    Task<Result<Guid>> InitWorkProcessIntegration(Guid connectionId, Guid workProcessExternalId, CancellationToken cancellationToken);
    Task<Result<Guid>> InitWorkspaceIntegration(Guid connectionId, Guid workspaceExternalId, string workspaceKey, string workspaceName, CancellationToken cancellationToken);
}
