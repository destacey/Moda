namespace Moda.AppIntegration.Application.Interfaces;
public interface IAzureDevOpsInitManager : ITransientService
{
    Task<Result> SyncOrganizationConfiguration(Guid connectionId, CancellationToken cancellationToken, Guid? syncId = null);
    Task<Result<Guid>> InitWorkProcessIntegration(Guid connectionId, Guid workProcessExternalId, CancellationToken cancellationToken);
    Task<Result<Guid>> InitWorkspaceIntegration(Guid connectionId, Guid workspaceExternalId, string workspaceKey, string workspaceName, string? externalViewWorkItemUrlTemplate, CancellationToken cancellationToken);
}
