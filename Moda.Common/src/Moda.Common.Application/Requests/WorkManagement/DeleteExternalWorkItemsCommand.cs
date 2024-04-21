namespace Moda.Common.Application.Requests.WorkManagement;
public sealed record DeleteExternalWorkItemsCommand(Guid WorkspaceId, int[] WorkItemIds) : ICommand;
