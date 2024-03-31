namespace Moda.Web.Api.Interfaces;

public interface IJobManager
{
    Task RunSyncExternalEmployees(CancellationToken cancellationToken);
    Task RunSyncAzureDevOpsBoards(CancellationToken cancellationToken);
}
