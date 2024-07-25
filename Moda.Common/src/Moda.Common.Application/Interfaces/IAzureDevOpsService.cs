using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Application.Interfaces.Work;

namespace Moda.Common.Application.Interfaces;
public interface IAzureDevOpsService
{
    Task<Result<List<IExternalWorkProcess>>> GetWorkProcesses(string organizationUrl, string token, CancellationToken cancellationToken);
    Task<Result<IExternalWorkProcessConfiguration>> GetWorkProcess(string organizationUrl, string token, Guid processId, CancellationToken cancellationToken);
    Task<Result<IExternalWorkspaceConfiguration>> GetWorkspace(string organizationUrl, string token, Guid workspaceId, CancellationToken cancellationToken);
    Task<Result<List<IExternalWorkspace>>> GetWorkspaces(string organizationUrl, string token, CancellationToken cancellationToken);
    Task<Result<List<IExternalWorkItem>>> GetWorkItems(string organizationUrl, string token, string projectName, DateTime lastChangedDate, string[] workItemTypes, CancellationToken cancellationToken);
    Task<Result<int[]>> GetDeletedWorkItemIds(string organizationUrl, string token, string projectName, DateTime lastChangedDate, CancellationToken cancellationToken);
    Task<Result> TestConnection(string organizationUrl, string token);
}