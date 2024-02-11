using System.Net;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Moda.Integrations.AzureDevOps.Clients;
using Moda.Integrations.AzureDevOps.Models;
using Moda.Integrations.AzureDevOps.Models.Processes;

namespace Moda.Integrations.AzureDevOps.Services;
internal sealed class ProcessService(string organizationUrl, string token, string apiVersion, ILogger<ProcessService> logger)
{
    private readonly ProcessClient _processClient = new ProcessClient(organizationUrl, token, apiVersion);
    private readonly ILogger<ProcessService> _logger = logger;

    public async Task<Result<List<AzdoWorkProcess>>> GetProcesses(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _processClient.GetProcesses(cancellationToken);
            if (!response.IsSuccessful)
            {
                _logger.LogError("Error getting processes from Azure DevOps: {ErrorMessage}.", response.ErrorMessage);
                return Result.Failure<List<AzdoWorkProcess>>(response.ErrorMessage);
            }

            _logger.LogDebug("{ProcessCount} processes found.", response.Data?.Count ?? 0);

            var processes = response.Data?.Items.ToAzdoWorkProcesses() ?? [];

            return Result.Success(processes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown getting processes from Azure DevOps.");
            return Result.Failure<List<AzdoWorkProcess>>(ex.ToString());
        }
    }

    public async Task<Result<AzdoWorkProcessConfiguration>> GetProcess(Guid processId, CancellationToken cancellationToken)
    {
        try
        {
            var processResult = await GetProcessById(processId, cancellationToken);
            if (processResult.IsFailure)
                return Result.Failure<AzdoWorkProcessConfiguration>(processResult.Error);

            var behaviorsResult = await GetProcessBehaviors(processId, cancellationToken);
            if (behaviorsResult.IsFailure)
                return Result.Failure<AzdoWorkProcessConfiguration>(behaviorsResult.Error);

            var workTypesResult = await GetProcessWorkItemTypes(processId, cancellationToken);
            if (workTypesResult.IsFailure)
                return Result.Failure<AzdoWorkProcessConfiguration>(workTypesResult.Error);

            // TODO: Get the work flow configuration for the work types
            return Result.Success(processResult.Value.ToAzdoWorkProcessDetails(behaviorsResult.Value, workTypesResult.Value));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown getting process {ProcessId} from Azure DevOps.", processId);
            return Result.Failure<AzdoWorkProcessConfiguration>(ex.ToString());
        }
    }

    private async Task<Result<ProcessDto>> GetProcessById(Guid processId, CancellationToken cancellationToken)
    {
        var response = await _processClient.GetProcess(processId, cancellationToken);
        if (!response.IsSuccessful && response.StatusCode != HttpStatusCode.NotFound)
        {
            var statusDescription = response.StatusCode is 0 ? "Connection Error" : response.StatusDescription;
            var errorMessage = response.ErrorMessage is null ? statusDescription : $"{statusDescription} - {response.ErrorMessage}";
            _logger.LogError("Error getting process {ProcessId} from Azure DevOps: {ErrorMessage}.", processId, errorMessage);
            return Result.Failure<ProcessDto>(errorMessage);
        }
        else if ((!response.IsSuccessful && response.StatusCode is HttpStatusCode.NotFound) || response.Data is null)
        {
            var errorMesssage = response.IsSuccessful ? "No process data returned" : response.StatusDescription;
            _logger.LogError("Error getting process {ProcessId} from Azure DevOps: {ErrorMessage}.", processId, errorMesssage);
            return Result.Failure<ProcessDto>(errorMesssage);
        }

        _logger.LogDebug("Process {ProcessId} found.", processId);

        return Result.Success(response.Data);
    }

    private async Task<Result<List<BehaviorDto>>> GetProcessBehaviors(Guid processId, CancellationToken cancellationToken)
    {
        var response = await _processClient.GetBehaviors(processId, cancellationToken);
        if (!response.IsSuccessful)
        {
            _logger.LogError("Error getting behaviors for process {ProcessId} from Azure DevOps: {ErrorMessage}.", processId, response.ErrorMessage);
            return Result.Failure<List<BehaviorDto>>(response.ErrorMessage);
        }

        _logger.LogDebug("{BehaviorCount} behaviors found for process {ProcessId}.", response.Data?.Count ?? 0, processId);

        return Result.Success(response.Data?.Items ?? []);
    }

    private async Task<Result<List<ProcessWorkItemTypeDto>>> GetProcessWorkItemTypes(Guid processId, CancellationToken cancellationToken)
    {
        var response = await _processClient.GetWorkItemTypes(processId, cancellationToken);
        if (!response.IsSuccessful)
        {
            _logger.LogError("Error getting work item types for process {ProcessId} from Azure DevOps: {ErrorMessage}.", processId, response.ErrorMessage);
            return Result.Failure<List<ProcessWorkItemTypeDto>>(response.ErrorMessage);
        }

        _logger.LogDebug("{WorkItemTypeCount} work item types found for process {ProcessId}.", response.Data?.Count ?? 0, processId);

        return Result.Success(response.Data?.Items ?? []);
    }
}
