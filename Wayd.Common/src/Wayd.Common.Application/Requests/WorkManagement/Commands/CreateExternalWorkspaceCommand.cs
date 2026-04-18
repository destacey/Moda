using Wayd.Common.Application.Interfaces.ExternalWork;
using Wayd.Common.Domain.Enums.AppIntegrations;
using Wayd.Common.Domain.Models;
using Wayd.Common.Models;

namespace Wayd.Common.Application.Requests.WorkManagement.Commands;

public sealed record CreateExternalWorkspaceCommand(Connector Connector, string SystemId, IExternalWorkspaceConfiguration ExternalWorkspace, WorkspaceKey WorkspaceKey, string WorkspaceName, string? ExternalViewWorkItemUrlTemplate) : ICommand<IntegrationState<Guid>>;