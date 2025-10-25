using Hangfire;
using Moda.AppIntegration.Application.Interfaces;
using Moda.Common.Application.Enums;
using Moda.Common.Application.Exceptions;
using Moda.Common.Application.Interfaces;
using Moda.Organization.Application.Teams.Commands;
using Moda.Planning.Application.Iterations.Queries;
using Moda.ProjectPortfolioManagement.Application.Projects.Queries;
using Moda.StrategicManagement.Application.StrategicThemes.Queries;
using Moda.Web.Api.Interfaces;
using Moda.Work.Application.WorkIterations.Commands;
using Moda.Work.Application.WorkProjects.Commands;
using PpmSyncStrategicThemesCommand = Moda.ProjectPortfolioManagement.Application.StrategicThemes.Commands.SyncStrategicThemesCommand;

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

    [DisableConcurrentExecution(60 *3)]
    public async Task RunSyncIterations(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Running {BackgroundJob} job", nameof(RunSyncIterations));

        var iterations = await _sender.Send(new GetSimpleIterationsQuery(), cancellationToken);

        var result = await _sender.Send(new SyncWorkIterationsCommand(iterations), cancellationToken);
        if (result.IsFailure)
        {
            _logger.LogError("Failed to sync iterations: {Error}", result.Error);
            throw new InternalServerException($"Failed to sync iterations. Error: {result.Error}");
        }

        _logger.LogInformation("Completed {BackgroundJob} job", nameof(RunSyncIterations));
    }


    [DisableConcurrentExecution(60 * 3)]
    public async Task RunSyncStrategicThemes(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Running {BackgroundJob} job", nameof(RunSyncStrategicThemes));

        var strategicThemes = await _sender.Send(new GetStrategicThemesDataQuery(), cancellationToken);

        var result = await _sender.Send(new PpmSyncStrategicThemesCommand(strategicThemes), cancellationToken);
        if (result.IsFailure)
        {
            _logger.LogError("Failed to sync strategic themes: {Error}", result.Error);
            throw new InternalServerException($"Failed to sync strategic themes. Error: {result.Error}");
        }

        _logger.LogInformation("Completed {BackgroundJob} job", nameof(RunSyncStrategicThemes));
    }

    [DisableConcurrentExecution(60 * 3)]
    public async Task RunSyncProjects(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Running {BackgroundJob} job", nameof(RunSyncProjects));

        var projects = await _sender.Send(new GetSimpleProjectsQuery(), cancellationToken);

        var result = await _sender.Send(new SyncWorkProjectsCommand(projects), cancellationToken);
        if (result.IsFailure)
        {
            _logger.LogError("Failed to sync projects: {Error}", result.Error);
            throw new InternalServerException($"Failed to sync projects. Error: {result.Error}");
        }
        _logger.LogInformation("Completed {BackgroundJob} job", nameof(RunSyncProjects));
    }
}
