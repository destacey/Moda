using Moda.Common.Application.Enums;

namespace Moda.AppIntegration.Application.Interfaces;
public interface IAzureDevOpsSyncManager : ITransientService
{
    Task<Result> Sync(SyncType syncType, CancellationToken cancellationToken);
}
