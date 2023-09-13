using CSharpFunctionalExtensions;

namespace Moda.AppIntegration.Application.Interfaces;
public interface IAzureDevOpsBoardsImportService : ITransientService
{
    Task<Result> ImportWorkspaces(Guid connectionId, CancellationToken cancellationToken);
}
