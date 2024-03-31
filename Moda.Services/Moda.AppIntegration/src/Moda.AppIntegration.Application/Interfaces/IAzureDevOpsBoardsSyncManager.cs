namespace Moda.AppIntegration.Application.Interfaces;
public interface IAzureDevOpsBoardsSyncManager : ITransientService
{
    Task<Result> Sync(CancellationToken cancellationToken);
}
