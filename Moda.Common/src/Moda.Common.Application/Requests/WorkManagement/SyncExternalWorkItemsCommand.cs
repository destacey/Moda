using Moda.Common.Application.Interfaces.Work;

namespace Moda.Common.Application.Requests.WorkManagement;
public sealed record SyncExternalWorkItemsCommand(Guid WorkspaceId, List<IExternalWorkItem> WorkItems) : ICommand;
