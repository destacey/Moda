using Moda.Common.Application.Interfaces.Work;
using Moda.Common.Application.Requests.WorkManagement;
using Moda.Common.Domain.Enums;

namespace Moda.Work.Application.WorkItems.Commands;

// TODO: add validation

internal sealed class SyncExternalWorkItemsCommandHandler(IWorkDbContext workDbContext, ILogger<SyncExternalWorkItemsCommandHandler> logger) : ICommandHandler<SyncExternalWorkItemsCommand>
{
    private const string AppRequestName = nameof(SyncExternalWorkItemsCommand);

    private readonly IWorkDbContext _workDbContext = workDbContext;
    private readonly ILogger<SyncExternalWorkItemsCommandHandler> _logger = logger;

    public async Task<Result> Handle(SyncExternalWorkItemsCommand request, CancellationToken cancellationToken)
    {
        if (request.WorkItems.Count == 0)
        {
            return Result.Success();
        }

        var workspace = await _workDbContext.Workspaces
            .FirstOrDefaultAsync(w => w.Id == request.WorkspaceId && w.Ownership == Ownership.Managed, cancellationToken);
        if (workspace is null)
        {
            _logger.LogWarning("Unable to sync external work items for workspace {WorkspaceId} because the workspace does not exist.", request.WorkspaceId);
            return Result.Failure($"Unable to sync external work items for workspace {request.WorkspaceId} because the workspace does not exist.");
        }

        var syncLog = new WorkItemSyncLog(request.WorkItems.Count);

        try
        {
            var workTypes = (await _workDbContext.WorkTypes.Select(t => new { t.Id, t.Name }).ToListAsync(cancellationToken)).ToHashSet();
            var workStatuses = (await _workDbContext.WorkStatuses.Select(s => new { s.Id, s.Name }).ToListAsync(cancellationToken)).ToHashSet();
            var employees = (await _workDbContext.Employees.Select(e => new { e.Id, e.Email }).ToListAsync(cancellationToken)).ToHashSet();
            var workItemIds = (await _workDbContext.WorkItems.Where(w => w.WorkspaceId == request.WorkspaceId).Select(w => new { w.Id, w.ExternalId}).ToListAsync(cancellationToken)).ToHashSet();

            int chunkSize = 1000;
            var chunks = request.WorkItems.OrderBy(w => w.LastModified).Chunk(chunkSize);

            var missingParents = new Dictionary<int, int>();
            foreach (var chunk in chunks)
            {
                // TODO: this is not a performant way to do this.  Make it better.
                await _workDbContext.Entry(workspace)
                    .Collection(w => w.WorkItems)
                    .Query()
                    .Where(wi => chunk.Select(c => c.Id).Contains(wi.ExternalId!.Value))
                    .LoadAsync(cancellationToken);

                List<WorkItem> newWorkItems = new(chunkSize);

                foreach (var externalWorkItem in chunk)
                {
                    syncLog.ItemRequested(externalWorkItem.Id);

                    try
                    {
                        Guid? parentId = null;
                        if (externalWorkItem.ParentId.HasValue)
                        {
                            parentId = workItemIds.FirstOrDefault(wi => wi.ExternalId == externalWorkItem.ParentId.Value)?.Id;
                            if (parentId is null || parentId == Guid.Empty)
                            { 
                                missingParents.Add(externalWorkItem.Id, externalWorkItem.ParentId.Value);
                            }
                        }

                        var workItem = workspace.WorkItems.FirstOrDefault(wi => wi.ExternalId == externalWorkItem.Id);
                        if (workItem is null)
                        {
                            workItem = WorkItem.CreateExternal(
                                workspace,
                                externalWorkItem.Id,
                                externalWorkItem.Title,
                                workTypes.Single(t => t.Name == externalWorkItem.WorkType).Id,
                                workStatuses.Single(s => s.Name == externalWorkItem.WorkStatus).Id,
                                parentId,
                                externalWorkItem.Created,
                                employees.SingleOrDefault(e => e.Email == externalWorkItem.CreatedBy)?.Id,
                                externalWorkItem.LastModified,
                                employees.SingleOrDefault(e => e.Email == externalWorkItem.LastModifiedBy)?.Id,
                                employees.SingleOrDefault(e => e.Email == externalWorkItem.AssignedTo)?.Id,
                                externalWorkItem.Priority,
                                externalWorkItem.StackRank
                            );
                            newWorkItems.Add( workItem );

                            syncLog.ItemCreated();
                        }
                        else
                        {
                            workItem.Update(
                                externalWorkItem.Title,
                                workTypes.Single(t => t.Name == externalWorkItem.WorkType).Id,
                                workStatuses.Single(s => s.Name == externalWorkItem.WorkStatus).Id,
                                parentId,
                                externalWorkItem.LastModified,
                                employees.SingleOrDefault(e => e.Email == externalWorkItem.LastModifiedBy)?.Id,
                                employees.SingleOrDefault(e => e.Email == externalWorkItem.AssignedTo)?.Id,
                                externalWorkItem.Priority,
                                externalWorkItem.StackRank
                            );

                            syncLog.ItemUpdated();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Exception thrown while syncing external work item {ExternalId} in workspace {WorkspaceId} ({WorkspaceName}).", externalWorkItem.Id, workspace.Id, workspace.Name);
                        throw;
                    }
                }

                if (newWorkItems.Count > 0)
                {
                    await _workDbContext.WorkItems.AddRangeAsync(newWorkItems, cancellationToken);
                }

                await _workDbContext.SaveChangesAsync(cancellationToken);
            }

            await MapMissingParents(workspace, missingParents, cancellationToken);
        }
        catch (Exception ex)
        {
            // Reset the entity
            await _workDbContext.Entry(workspace).ReloadAsync(cancellationToken);
            workspace.ClearDomainEvents();

            _logger.LogError(ex, "An error occurred while syncing workspace {WorkspaceId} work items.", workspace.Id);
            return Result.Failure($"An error occurred while syncing workspace {workspace.Id} work items.");
        }
        finally
        {
            _logger.LogInformation("Synced {Processed} external work items for workspace {WorkspaceId}. Requested: {Requested}, Created: {Created}, Updated: {Updated}, Last External Id: {LastExternalId}.", syncLog.Processed, workspace.Id, syncLog.Requested, syncLog.Created, syncLog.Updated, syncLog.LastExternalId);
        }

        return Result.Success();
    }

    private async Task MapMissingParents(Workspace workspace, Dictionary<int, int> missingParents, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Mapping {WorkItemCount} missing parents for workspace {WorkspaceId}.", missingParents.Count, workspace.Id);
        if (missingParents.Count == 0)
        {
            return;
        }

        var missingParentIds = missingParents.Values.Distinct().ToArray();
        var parentWorkItemIds = (await _workDbContext.WorkItems.Where(w => w.WorkspaceId == workspace.Id && missingParentIds.Contains(w.ExternalId!.Value)).Select(w => new { w.Id, w.ExternalId }).ToListAsync(cancellationToken)).ToHashSet();

        var workItemsMissingParents = missingParents.Keys.ToArray();
        var missingParentWorkItems = await _workDbContext.WorkItems.Where(w => w.WorkspaceId == workspace.Id && workItemsMissingParents.Contains(w.ExternalId!.Value)).ToListAsync(cancellationToken);
        foreach (var workItem in missingParentWorkItems)
        {
            var parentExternalId = missingParents[workItem.ExternalId!.Value];
            var parentId = parentWorkItemIds.FirstOrDefault(wi => wi.ExternalId == parentExternalId)?.Id;
            if (parentId is not null)
            {
                workItem.UpdateParent(parentId);
            }
            else
            {
                _logger.LogWarning("Unable to map missing parent for work item {ExternalId} in workspace {WorkspaceId}.", workItem.ExternalId, workspace.Id);
            }
        }

        await _workDbContext.SaveChangesAsync(cancellationToken);
    }

    private sealed class WorkItemSyncLog(int requested)
    {
        public int Requested { get; init; } = requested;
        public int Created { get; private set; }
        public int Updated { get; private set; }
        public int Processed => Created + Updated;
        public int? LastExternalId { get; private set; }

        public void ItemRequested(int externalId)
        {
            LastExternalId = externalId;
        }

        public void ItemCreated()
        {
            Created++;
            LastExternalId = null;
        }

        public void ItemUpdated()
        {
            Updated++;            
            LastExternalId = null;
        }
    }
}
