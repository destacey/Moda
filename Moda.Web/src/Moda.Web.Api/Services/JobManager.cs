using Hangfire;
using Moda.AppIntegration.Application.Interfaces;
using Moda.Common.Application.Enums;
using Moda.Common.Application.Exceptions;
using Moda.Common.Application.Interfaces;
using Moda.Web.Api.Interfaces;

namespace Moda.Web.Api.Services;

public class JobManager(ILogger<JobManager> logger, IEmployeeService employeeService, IAzureDevOpsBoardsSyncManager azdoBoardsSyncManager) : IJobManager
{
    // TODO: does this belong in JobService/HangfireService?

    private readonly ILogger<JobManager> _logger = logger;
    private readonly IEmployeeService _employeeService = employeeService;
    private readonly IAzureDevOpsBoardsSyncManager _azdoBoardsSyncManager = azdoBoardsSyncManager;

    [DisableConcurrentExecution(60)]
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = [30, 60, 120])]
    public async Task RunSyncExternalEmployees(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Running SyncExternalEmployees job");
        var result = await _employeeService.SyncExternalEmployees(cancellationToken);
        if (result.IsFailure)
        {
            _logger.LogError("Failed to sync external employees: {Error}", result.Error);
            throw new InternalServerException($"Failed to sync external employees. Error: {result.Error}");
        }
        _logger.LogInformation("Completed SyncExternalEmployees job");
    }

    [DisableConcurrentExecution(60 * 3)]
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = [30, 60, 120])]
    public async Task RunSyncAzureDevOpsBoards(SyncType syncType, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Running SyncAzureDevOpsBoards job");
        await _azdoBoardsSyncManager.Sync(syncType, cancellationToken);
        _logger.LogInformation("Completed SyncAzureDevOpsBoards job");
    }
}
