namespace Moda.Common.Application.Requests.WorkManagement.Commands;
public sealed record DeleteExternalWorkItemsCommand(Guid WorkspaceId, int[] WorkItemIds) : ICommand, ILongRunningRequest;
