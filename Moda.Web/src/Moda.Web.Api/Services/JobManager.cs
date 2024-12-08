﻿using Hangfire;
using Moda.AppIntegration.Application.Interfaces;
using Moda.Common.Application.Enums;
using Moda.Common.Application.Exceptions;
using Moda.Common.Application.Interfaces;
using Moda.Organization.Application.Teams.Commands;
using Moda.Web.Api.Interfaces;

namespace Moda.Web.Api.Services;

public class JobManager(ILogger<JobManager> logger, IEmployeeService employeeService, IAzureDevOpsBoardsSyncManager azdoBoardsSyncManager, ISender sender) : IJobManager
{
    // TODO: does this belong in JobService/HangfireService?

    private readonly ILogger<JobManager> _logger = logger;
    private readonly IEmployeeService _employeeService = employeeService;
    private readonly IAzureDevOpsBoardsSyncManager _azdoBoardsSyncManager = azdoBoardsSyncManager;
    private readonly ISender _sender = sender;

    [DisableConcurrentExecution(60)]
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = [30, 60, 120])]
    public async Task RunSyncExternalEmployees(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Running {BackgroundJob} job", nameof(RunSyncExternalEmployees));
        var result = await _employeeService.SyncExternalEmployees(cancellationToken);
        if (result.IsFailure)
        {
            _logger.LogError("Failed to sync external employees: {Error}", result.Error);
            throw new InternalServerException($"Failed to sync external employees. Error: {result.Error}");
        }
        _logger.LogInformation("Completed {BackgroundJob} job", nameof(RunSyncExternalEmployees));
    }

    [DisableConcurrentExecution(60 * 3)]
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = [30, 60, 120])]
    public async Task RunSyncAzureDevOpsBoards(SyncType syncType, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Running {BackgroundJob} job", nameof(RunSyncAzureDevOpsBoards));
        var result = await _azdoBoardsSyncManager.Sync(syncType, cancellationToken);
        if (result.IsFailure)
        {
            _logger.LogError("Failed to sync Azure DevOps boards: {Error}", result.Error);
            throw new InternalServerException($"Failed to sync Azure DevOps boards. Error: {result.Error}");
        }
        _logger.LogInformation("Completed {BackgroundJob} job", nameof(RunSyncAzureDevOpsBoards));
    }

    [DisableConcurrentExecution(60 * 3)]
    public async Task RunSyncTeamsWithGraphTables(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Running {BackgroundJob} job", nameof(RunSyncTeamsWithGraphTables));

        var teamNodesresult = await _sender.Send(new SyncTeamNodesCommand(), cancellationToken);
        if (teamNodesresult.IsFailure)
        {
            _logger.LogError("Failed to sync teams with graph tables: {Error}", teamNodesresult.Error);
            throw new InternalServerException($"Failed to sync teams with graph tables. Error: {teamNodesresult.Error}");
        }

        var teamMembershipEdgesResult = await _sender.Send(new SyncTeamMembershipEdgesCommand(), cancellationToken);
        if (teamMembershipEdgesResult.IsFailure)
        {
            _logger.LogError("Failed to sync team memberships with graph tables: {Error}", teamMembershipEdgesResult.Error);
            throw new InternalServerException($"Failed to sync team memberships with graph tables. Error: {teamMembershipEdgesResult.Error}");
        }

        _logger.LogInformation("Completed {BackgroundJob} job", nameof(RunSyncTeamsWithGraphTables));
    }
}
