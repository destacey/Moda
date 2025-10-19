using Moda.Common.Application.Requests.WorkManagement;
using Moda.Common.Domain.Enums;
using Moda.Common.Domain.Enums.Work;
using Moda.Work.Application.Persistence;
using Moda.Work.Application.WorkItems.Dtos;
using Moda.Work.Domain.Interfaces;

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
            var workProcess = await _workDbContext.WorkProcesses
                .AsNoTracking()
                .Include(p => p.Schemes)
                    .ThenInclude(s => s.WorkType)
                        .ThenInclude(t => t!.Level)
                .Include(p => p.Schemes)
                    .ThenInclude(s => s.Workflow)
                        .ThenInclude(wf => wf!.Schemes)
                            .ThenInclude(wfs => wfs.WorkStatus)
                .FirstOrDefaultAsync(wp => wp.Id == workspace.WorkProcessId, cancellationToken);
            if (workProcess is null)
            {
                _logger.LogWarning("Unable to sync external work items for workspace {WorkspaceId} because the workspace does not have a work process.", workspace.Id);
                return Result.Failure($"Unable to sync external work items for workspace {workspace.Id} because the workspace does not have a work process.");
            }

            HashSet<WorkType> workTypes = [];
            HashSet<WorkStatusMapping> workStatusMappings = [];

            foreach (var scheme in workProcess.Schemes)
            {
                if (scheme.WorkType is null || scheme.Workflow is null || scheme.Workflow.Schemes.Count == 0 || !scheme.Workflow.Schemes.Any(s => s.WorkStatus != null))
                {
                    continue;
                }

                workTypes.Add(scheme.WorkType);
                foreach (var workflowScheme in scheme.Workflow.Schemes)
                {
                    if (workflowScheme.WorkStatus is not null)
                    {
                        workStatusMappings.Add(new WorkStatusMapping(scheme.WorkTypeId, workflowScheme.WorkStatus, workflowScheme.WorkStatusCategory));
                    }
                }
            }

            var employees = (await _workDbContext.Employees.Select(e => new { e.Id, e.Email }).ToListAsync(cancellationToken)).ToHashSet();

            // TODO: how do we handle parents in other workspaces, but in the same Azdo org?
            var potentialParents = (await _workDbContext.WorkItems
                    .Where(w => w.WorkspaceId == request.WorkspaceId)
                    .ProjectToType<WorkItemParentInfo>()
                    .ToArrayAsync(cancellationToken))
                .ToHashSet();

            int chunkSize = 2000;
            var chunks = request.WorkItems.OrderBy(w => w.LastModified).Chunk(chunkSize).ToList();

            var missingParents = new Dictionary<int, int>();
            int c = 1;
            foreach (var chunk in chunks)
            {
                // TODO: this is not a performant way to do this.  Make it better.
                await _workDbContext.Entry(workspace)
                    .Collection(w => w.WorkItems)
                    .Query()
                    .Include(wi => wi.ExtendedProps)
                    .Include(wi => wi.Type)
                        .ThenInclude(t => t.Level)
                    .Where(wi => chunk.Select(c => c.Id).Contains(wi.ExternalId!.Value))
                    .LoadAsync(cancellationToken);

                List<WorkItem> newWorkItems = new(chunkSize);

                foreach (var externalWorkItem in chunk)
                {
                    syncLog.ItemRequested(externalWorkItem.Id);

                    var workType = workTypes.Single(t => t.Name == externalWorkItem.WorkType);
                    var workFlowSchemeData = workStatusMappings.Single(m => m.WorkTypeId == workType.Id && m.WorkStatus.Name == externalWorkItem.WorkStatus);

                    IWorkItemParentInfo? parentWorkItemInfo = null;
                    if (externalWorkItem.ParentId.HasValue)
                    {
                        parentWorkItemInfo = potentialParents.FirstOrDefault(wi => wi.ExternalId == externalWorkItem.ParentId.Value);
                        if (parentWorkItemInfo is null)
                        {
                            missingParents.Add(externalWorkItem.Id, externalWorkItem.ParentId.Value);
                        }
                        else if (parentWorkItemInfo.Tier != WorkTypeTier.Portfolio)
                        {
                            if (_logger.IsEnabled(LogLevel.Debug))
                                _logger.LogDebug("Only portfolio tier work items can be parents. Unable to map parent for work item {ExternalId} in workspace {WorkspaceId}.", externalWorkItem.Id, workspace.Id);
                            parentWorkItemInfo = null;
                        }
                        else if (workType.Level!.Tier == WorkTypeTier.Portfolio && workType.Level.Order <= parentWorkItemInfo.LevelOrder)
                        {
                            if (_logger.IsEnabled(LogLevel.Debug))
                                _logger.LogDebug("The parent must be a higher level than the work item {ExternalId} in workspace {WorkspaceId}.", externalWorkItem.Id, workspace.Id);
                            parentWorkItemInfo = null;
                        }
                    }

                    var workItem = workspace.WorkItems.FirstOrDefault(wi => wi.ExternalId == externalWorkItem.Id);
                    try
                    {
                        Guid? teamId = null;
                        if (externalWorkItem.TeamId.HasValue)
                        {
                            request.TeamMappings.TryGetValue(externalWorkItem.TeamId.Value, out teamId);
                        }

                        if (workItem is null)
                        {
                            workItem = WorkItem.CreateExternal(
                                workspace,
                                externalWorkItem.Id,
                                externalWorkItem.Title,
                                workType,
                                workFlowSchemeData.WorkStatus.Id,
                                workFlowSchemeData.WorkStatusCategory,
                                parentWorkItemInfo,
                                teamId,
                                externalWorkItem.Created,
                                employees.SingleOrDefault(e => e.Email == externalWorkItem.CreatedBy)?.Id,
                                externalWorkItem.LastModified,
                                employees.SingleOrDefault(e => e.Email == externalWorkItem.LastModifiedBy)?.Id,
                                employees.SingleOrDefault(e => e.Email == externalWorkItem.AssignedTo)?.Id,
                                externalWorkItem.Priority,
                                externalWorkItem.StackRank,
                                externalWorkItem.ActivatedTimestamp,
                                externalWorkItem.DoneTimestamp,
                                string.IsNullOrWhiteSpace(externalWorkItem.ExternalTeamIdentifier) ? null : WorkItemExtended.Create(externalWorkItem.ExternalTeamIdentifier)
                            );
                            newWorkItems.Add(workItem);

                            syncLog.ItemCreated();
                        }
                        else
                        {
                            workItem.Update(
                                externalWorkItem.Title,
                                workType,
                                workFlowSchemeData.WorkStatus.Id,
                                workFlowSchemeData.WorkStatusCategory,
                                parentWorkItemInfo,
                                teamId,
                                externalWorkItem.LastModified,
                                employees.SingleOrDefault(e => e.Email == externalWorkItem.LastModifiedBy)?.Id,
                                employees.SingleOrDefault(e => e.Email == externalWorkItem.AssignedTo)?.Id,
                                externalWorkItem.Priority,
                                externalWorkItem.StackRank,
                                externalWorkItem.ActivatedTimestamp,
                                externalWorkItem.DoneTimestamp,
                                string.IsNullOrWhiteSpace(externalWorkItem.ExternalTeamIdentifier) ? null : WorkItemExtended.Create(workItem.Id, externalWorkItem.ExternalTeamIdentifier)
                            );

                            syncLog.ItemUpdated();
                        }
                    }
                    catch (Exception ex)
                    {
                        if (workItem is not null && workItem.Id != Guid.Empty)
                        {
                            // Reset the entity
                            await _workDbContext.Entry(workItem).ReloadAsync(cancellationToken);
                            workItem.ClearDomainEvents();
                        }

                        _logger.LogError(ex, "Exception thrown while syncing external work item {ExternalId} in workspace {WorkspaceId} ({WorkspaceName}).", externalWorkItem.Id, workspace.Id, workspace.Name);

                        syncLog.ItemError();
                        continue;
                    }
                }

                if (newWorkItems.Count > 0)
                {
                    await _workDbContext.WorkItems.AddRangeAsync(newWorkItems, cancellationToken);
                }

                await _workDbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Synced {ChunkCount} of {TotalChunks} for workspace {WorkspaceId} ({WorkspaceName}).", c++, chunks.Count, workspace.Id, workspace.Name);
            }

            await MapMissingParents(workspace, missingParents, cancellationToken);
            await SyncParentProjectIds(workspace, workTypes, cancellationToken);
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
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("Mapping {WorkItemCount} missing parents for workspace {WorkspaceId}.", missingParents.Count, workspace.Id);

        if (missingParents.Count == 0)
        {
            return;
        }

        var missingParentIds = missingParents.Values.Distinct().ToArray();
        var parentWorkItemIds = (await _workDbContext.WorkItems
            .Where(w => w.WorkspaceId == workspace.Id
                && w.Type.Level!.Tier == WorkTypeTier.Portfolio
                && missingParentIds.Contains(w.ExternalId!.Value))
            .ProjectToType<WorkItemParentInfo>()
            .ToListAsync(cancellationToken)).ToHashSet();

        var workItemIdsMissingParents = missingParents.Keys.ToArray();
        var missingParentWorkItems = await _workDbContext.WorkItems
            .Include(w => w.Type)
                .ThenInclude(t => t.Level)
            .Where(w => w.WorkspaceId == workspace.Id && workItemIdsMissingParents.Contains(w.ExternalId!.Value))
            .ToListAsync(cancellationToken);
        foreach (var workItem in missingParentWorkItems)
        {
            var missingParentId = missingParents[workItem.ExternalId!.Value];
            var parentInfo = parentWorkItemIds.FirstOrDefault(wi => wi.ExternalId == missingParentId);

            if (parentInfo is null)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug("Unable to map missing parent for work item {ExternalId} in workspace {WorkspaceId}.", workItem.ExternalId, workspace.Id);
            }
            else
            {
                var result = workItem.UpdateParent(parentInfo, workItem.Type);
                if (result.IsFailure && _logger.IsEnabled(LogLevel.Debug))
                {
                    if (workItem.ParentId.HasValue)
                    {
                        _logger.LogDebug("Broken parent link for work item {ExternalId} in workspace {WorkspaceId}. {Error}", workItem.ExternalId, workspace.Id, result.Error);
                    }
                    else
                    {
                        _logger.LogDebug("Unable to map parent for work item {ExternalId} in workspace {WorkspaceId}. {Error}", workItem.ExternalId, workspace.Id, result.Error);
                    }
                }
            }
        }

        await _workDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SyncParentProjectIds(Workspace workspace, HashSet<WorkType> workTypes, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("Syncing parent project ids for workspace {WorkspaceId}.", workspace.Id);

        if (workTypes.Any(t => t.Level is null))
        {
            _logger.LogError("Unable to sync parent project ids for workspace {WorkspaceId} because one or more work types do not have a level.", workspace.Id);
            return;
        }

        int updateCount = 0;

        // WorkTypeTier.Other tier is not a valid target for project ids
        WorkTypeTier[] tiers = [WorkTypeTier.Portfolio, WorkTypeTier.Requirement, WorkTypeTier.Task];
        var topTierLevelSkipped = false;
        foreach (var tier in tiers)
        {            
            var types = workTypes.Where(t => t.Level!.Tier == tier).OrderBy(t => t.Level!.Order);
            foreach (var workType in types)
            {
                if (!topTierLevelSkipped)
                {
                    topTierLevelSkipped = true;
                    continue;
                }
                int typeUpdateCount = 0;

                // get workitems for the work type that have a parent and the workitem.parentprojectid doesn't match the parent workitem projectId ?? parentProjectId
                var workItems = await _workDbContext.WorkItems
                    .Include(i => i.Type)
                        .ThenInclude(t => t.Level)
                    .Include(i => i.Status)
                    .Include(i => i.Parent)
                        .ThenInclude(p => p!.Type)
                            .ThenInclude(t => t.Level)
                    .Where(wi => wi.WorkspaceId == workspace.Id
                        && wi.TypeId == workType.Id
                        && wi.Parent != null
                        && wi.ParentProjectId != (wi.Parent.ProjectId ?? wi.Parent.ParentProjectId))
                    .ToListAsync(cancellationToken);

                foreach (var workItem in workItems)
                {
                    var parentInfo = workItem.Parent.Adapt<WorkItemParentInfo>();
                    workItem.UpdateParent(parentInfo, workType);

                    typeUpdateCount++;
                }

                if (typeUpdateCount > 0)
                {
                    await _workDbContext.SaveChangesAsync(cancellationToken);
                    updateCount += typeUpdateCount;
                }
            }
        }

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("Synced {UpdateCount} parent project ids for workspace {WorkspaceId}.", updateCount, workspace.Id);
    }

    private sealed record WorkStatusMapping(int WorkTypeId, WorkStatus WorkStatus, WorkStatusCategory WorkStatusCategory);

    private sealed record WorkItemSyncLog
    {
        public WorkItemSyncLog(int requested)
        {
            Requested = requested;
        }

        public int Requested { get; init; }
        public int Created { get; private set; }
        public int Updated { get; private set; }
        public int Errors { get; private set; }
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

        public void ItemError()
        {
            Errors++;
            LastExternalId = null;
        }
    }
}
