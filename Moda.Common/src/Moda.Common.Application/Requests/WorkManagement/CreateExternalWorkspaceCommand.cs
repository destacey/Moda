using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Domain.Enums.AppIntegrations;
using Moda.Common.Domain.Models;
using Moda.Common.Models;

namespace Moda.Common.Application.Requests.WorkManagement;
public sealed record CreateExternalWorkspaceCommand(Connector Connector, string SystemId, IExternalWorkspaceConfiguration ExternalWorkspace, WorkspaceKey WorkspaceKey, string WorkspaceName, string? ExternalViewWorkItemUrlTemplate) : ICommand<IntegrationState<Guid>>;