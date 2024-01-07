using CSharpFunctionalExtensions;

namespace Moda.AppIntegration.Application.Interfaces;
public interface IAzureDevOpsBoardsImportManager : ITransientService
{
    Task<Result> ImportOrganizationConfiguration(Guid connectionId, CancellationToken cancellationToken);
    Task<Result> InitWorkspaceIntegration(Guid connectionId, Guid workspaceExternalId, string workspaceKey, CancellationToken cancellationToken);
}
