using Wayd.Common.Domain.Enums.AppIntegrations;

namespace Wayd.Common.Application.Requests.WorkManagement.Commands;

public sealed record SetSystemIdOnExternalWorkspacesCommand(IEnumerable<Guid> WorkspaceIds, Connector Connector, string SystemId) : ICommand;
