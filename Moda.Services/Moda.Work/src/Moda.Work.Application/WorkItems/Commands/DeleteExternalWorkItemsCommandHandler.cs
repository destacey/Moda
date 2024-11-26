using Moda.Common.Application.Requests.WorkManagement;
using Moda.Common.Domain.Enums;

namespace Moda.Work.Application.WorkItems.Commands;
internal sealed class DeleteExternalWorkItemsCommandHandler(IWorkDbContext workDbContext, ILogger<SyncExternalWorkItemsCommandHandler> logger) : ICommandHandler<DeleteExternalWorkItemsCommand>
{
    private const string AppRequestName = nameof(DeleteExternalWorkItemsCommand);

    private readonly IWorkDbContext _workDbContext = workDbContext;
    private readonly ILogger<SyncExternalWorkItemsCommandHandler> _logger = logger;

    public async Task<Result> Handle(DeleteExternalWorkItemsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.WorkItemIds.Length == 0)
            {
                return Result.Success();
            }

            var workspace = await _workDbContext.Workspaces
                .FirstOrDefaultAsync(w => w.Id == request.WorkspaceId && w.Ownership == Ownership.Managed, cancellationToken);
            if (workspace is null)
            {
                _logger.LogWarning("Unable to delete external work items for workspace {WorkspaceId} because the workspace does not exist.", request.WorkspaceId);
                return Result.Failure($"Unable to delete external work items for workspace {request.WorkspaceId} because the workspace does not exist.");
            }

            var workItems = await _workDbContext.WorkItems
                .Include(w => w.Children)
                .Include(w => w.OutboundLinksHistory)
                .Include(w => w.InboundLinksHistory)
                .Include(w => w.ReferenceLinks)
                .Where(w => w.WorkspaceId == workspace.Id && request.WorkItemIds.Contains(w.ExternalId!.Value))
                .ToListAsync(cancellationToken);

            _logger.LogInformation("{WorkItemCount} external work items found to delete for workspace {WorkspaceId}.", workItems.Count, request.WorkspaceId);

            if (workItems.Count == 0)
            {
                return Result.Success();
            }

            _workDbContext.WorkItemLinks.RemoveRange(workItems.SelectMany(w => w.OutboundLinksHistory.Concat(w.InboundLinksHistory)));
            _workDbContext.WorkItems.RemoveRange(workItems);

            await _workDbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown deleting external work items for workspace {WorkspaceId}.", request.WorkspaceId);
            throw;
        }
    }
}
