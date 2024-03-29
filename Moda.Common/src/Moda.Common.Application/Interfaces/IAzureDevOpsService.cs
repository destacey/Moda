﻿using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Application.Interfaces.Work;

namespace Moda.Common.Application.Interfaces;
public interface IAzureDevOpsService
{
    Task<Result<List<IExternalWorkProcess>>> GetWorkProcesses(string organizationUrl, string token, CancellationToken cancellationToken);
    Task<Result<IExternalWorkProcessConfiguration>> GetWorkProcess(string organizationUrl, string token, Guid processId, CancellationToken cancellationToken);
    Task<Result<IExternalWorkspaceConfiguration>> GetWorkspace(string organizationUrl, string token, Guid workspaceId, CancellationToken cancellationToken);
    Task<Result<List<IExternalWorkspace>>> GetWorkspaces(string organizationUrl, string token, CancellationToken cancellationToken);
    Task<Result<IExternalWorkItem>> GetWorkItem(string organizationUrl, string token, Guid projectId, int workItemId, CancellationToken cancellationToken);
    Task<Result<List<IExternalWorkItem>>> GetWorkItems(string organizationUrl, string token, Guid projectId, int[] workItemIds, CancellationToken cancellationToken);
    Task<Result<List<IExternalWorkItem>>> GetWorkItems(string organizationUrl, string token, Guid projectId, string projectName, CancellationToken cancellationToken);
    //Task<Result<List<IExternalWorkType>>> GetWorkItemTypes(string organizationUrl, string token, Guid projectId, CancellationToken cancellationToken);
    Task<Result> TestConnection(string organizationUrl, string token);
}