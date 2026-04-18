using Wayd.Common.Application.Enums;

namespace Wayd.AppIntegration.Application.Interfaces;

public interface IAzureDevOpsSyncManager : ITransientService
{
    Task<Result> Sync(SyncType syncType, CancellationToken cancellationToken);
}
