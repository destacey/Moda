﻿using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moda.AppIntegration.Application.Connections.Commands;
using Moda.AppIntegration.Application.Connections.Queries;
using Moda.AppIntegration.Application.Interfaces;

namespace Moda.AppIntegration.Application.Connections.Services;
public sealed class AzureDevOpsBoardsImportService : IAzureDevOpsBoardsImportService
{
    private readonly ILogger<AzureDevOpsBoardsImportService> _logger;
    private readonly IAzureDevOpsService _azureDevOpsService;
    private readonly ISender _sender;
    private readonly IDateTimeService _dateTimeService;

    public AzureDevOpsBoardsImportService(ILogger<AzureDevOpsBoardsImportService> logger, IAzureDevOpsService azureDevOpsService, ISender sender, IDateTimeService dateTimeService)
    {
        _logger = logger;
        _azureDevOpsService = azureDevOpsService;
        _sender = sender;
        _dateTimeService = dateTimeService;
    }

    public async Task<Result> ImportWorkspaces(Guid connectionId, CancellationToken cancellationToken)
    {
        try
        {
            var connection = await _sender.Send(new GetAzureDevOpsBoardsConnectionQuery(connectionId), cancellationToken);
            if (connection is null)
            {
                _logger.LogError("Unable to find Azure DevOps Boards connection with id {ConnectionId}.", connectionId);
                return Result.Failure($"Unable to find Azure DevOps Boards connection with id {connectionId}.");                
            }

            if (connection.IsValidConfiguration is false)
            {
                _logger.LogError("The configuration for Azure DevOps Boards connection {ConnectionId} is not valid.", connectionId);
                return Result.Failure($"The configuration for connection {connectionId} is not valid.");
            }

            // Load Processes
            var workProcessesResult = await _azureDevOpsService.GetWorkProcesses(connection.Configuration.OrganizationUrl, connection.Configuration.PersonalAccessToken, cancellationToken);
            if (workProcessesResult.IsFailure)
                return workProcessesResult;

            List<AzureDevOpsBoardsWorkProcess> processes = new();
            foreach (var externalProcess in workProcessesResult.Value)
            {
                var process = AzureDevOpsBoardsWorkProcess.Create(externalProcess.Id, externalProcess.Name, externalProcess.Description);
                processes.Add(process);
            }

            // Load workspaces
            var workspacesResult = await _azureDevOpsService.GetWorkspaces(connection.Configuration.OrganizationUrl, connection.Configuration.PersonalAccessToken);
            if (workspacesResult.IsFailure)
                return workspacesResult;

            List<AzureDevOpsBoardsWorkspace> workspaces = new();
            foreach (var externalWorkspace in workspacesResult.Value)
            {
                var workProcessId = workProcessesResult.Value.FirstOrDefault(p => p.WorkspaceIds.Contains(externalWorkspace.Id))?.Id;
                var workspace = AzureDevOpsBoardsWorkspace.Create(externalWorkspace.Id, externalWorkspace.Name, externalWorkspace.Description, workProcessId);
                workspaces.Add(workspace);
            }

            var bulkUpsertResult = await _sender.Send(new BulkUpsertAzureDevOpsBoardsWorkspacesCommand(connectionId, processes, workspaces), cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while trying to import projects from Azure DevOps Boards for connection {ConnectionId}.", connectionId);
            return Result.Failure($"An error occurred while trying to import projects from Azure DevOps Boards for connection {connectionId}.");
        }

    }
}
