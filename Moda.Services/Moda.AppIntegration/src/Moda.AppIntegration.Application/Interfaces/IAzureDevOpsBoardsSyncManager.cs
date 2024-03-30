namespace Moda.AppIntegration.Application.Interfaces;
public interface IAzureDevOpsBoardsSyncManager : ITransientService
{
    Task Sync(CancellationToken cancellationToken);
}
