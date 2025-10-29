using Moda.Common.Domain.Enums.AppIntegrations;

namespace Moda.Common.Application.Requests.WorkManagement;
public sealed record SetSystemIdOnExternalWorkspacesCommand(IEnumerable<Guid> WorkspaceIds, Connector Connector, string SystemId) : ICommand;
