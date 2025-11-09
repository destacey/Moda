using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Application.Requests.WorkManagement;
using Moda.Work.Application.Persistence;

namespace Moda.Work.Application.WorkItems.Commands;
internal sealed class SyncExternalWorkItemDependencyChangesCommandHandler(IWorkDbContext workDbContext, ILogger<SyncExternalWorkItemDependencyChangesCommandHandler> logger) : ICommandHandler<SyncExternalWorkItemDependencyChangesCommand>
{
    private const string AppRequestName = nameof(SyncExternalWorkItemDependencyChangesCommand);
    private const int DefaultBatchSize = 500;

    private static readonly Dictionary<string, int> _operationPriority = new()
    {
        { "remove", 0 },
        { "create", 1 }
    };

    private readonly IWorkDbContext _workDbContext = workDbContext;
    private readonly ILogger<SyncExternalWorkItemDependencyChangesCommandHandler> _logger = logger;

    public async Task<Result> Handle(SyncExternalWorkItemDependencyChangesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var validLinks = await ValidateAndMapWorkItemLinks(request.WorkItemLinks, cancellationToken);
            if (!validLinks.Any())
            {
                _logger.LogInformation("{AppRequestName}: No valid work item dependency links found for workspace {WorkspaceId} ", AppRequestName, request.WorkspaceId);
                return Result.Success();
            }

            _logger.LogInformation("{AppRequestName}: Found {ValidLinksCount} valid work item dependency links out of {WorkItemLinksCount}.", AppRequestName, validLinks.Count, request.WorkItemLinks.Count);

            await ProcessLinksInBatches(validLinks, cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown handling {AppRequestName}", AppRequestName);
            return Result.Failure(ex.ToString());
        }
    }

    private async Task<IReadOnlyList<DependencyInfo>> ValidateAndMapWorkItemLinks(IReadOnlyList<IExternalWorkItemLink> workItemLinks, CancellationToken cancellationToken)
    {
        if (!workItemLinks.Any())
        {
            return [];
        }

        var workItemMap = await GetWorkItemMap(workItemLinks, cancellationToken);
        var validLinks = await MapValidLinks(workItemLinks, workItemMap, cancellationToken);

        return validLinks;
    }

    private async Task<Dictionary<WorkItemExternalId, Guid>> GetWorkItemMap(IReadOnlyList<IExternalWorkItemLink> workItemLinks, CancellationToken cancellationToken)
    {
        var uniqueIds = workItemLinks.SelectMany(wil => new[]
        {
            new WorkItemExternalId(wil.SourceId, wil.SourceWorkspaceId),
            new WorkItemExternalId(wil.TargetId, wil.TargetWorkspaceId)
        }).ToHashSet();

        // Split the uniqueIds into their components for the join
        var externalIds = uniqueIds.Select(id => id.ExternalId).ToHashSet();
        var workspaceExternalIds = uniqueIds.Select(id => id.WorkspaceExternalId.ToString()).ToHashSet();

        var workItems = await _workDbContext.WorkItems
            .Where(wi => wi.ExternalId.HasValue &&
                externalIds.Contains(wi.ExternalId.Value) &&
                wi.Workspace.OwnershipInfo.ExternalId != null &&
                workspaceExternalIds.Contains(wi.Workspace.OwnershipInfo.ExternalId))
            .Select(wi => new
            {
                wi.Id,
                ExternalId = wi.ExternalId!.Value,
                WorkspaceExternalId = wi.Workspace.OwnershipInfo.ExternalId!
            })
            .ToListAsync(cancellationToken);

        // Post-process to ensure we only get exact matches
        return workItems
            .Where(wi => uniqueIds.Contains(new WorkItemExternalId(wi.ExternalId, Guid.Parse(wi.WorkspaceExternalId))))
            .ToDictionary(
                wi => new WorkItemExternalId(wi.ExternalId, Guid.Parse(wi.WorkspaceExternalId)),
                wi => wi.Id);
    }

    private async Task<List<DependencyInfo>> MapValidLinks(IReadOnlyList<IExternalWorkItemLink> workItemLinks, Dictionary<WorkItemExternalId, Guid> workItemMap, CancellationToken cancellationToken)
    {
        var employees = (await _workDbContext.Employees.Select(e => new { e.Id, e.Email }).ToListAsync(cancellationToken)).ToHashSet();

        var validLinks = new List<DependencyInfo>();

        foreach (var link in workItemLinks)
        {
            var sourceId = new WorkItemExternalId(link.SourceId, link.SourceWorkspaceId);
            var targetId = new WorkItemExternalId(link.TargetId, link.TargetWorkspaceId);

            if (workItemMap.TryGetValue(sourceId, out var sourceInternalId) &&
                workItemMap.TryGetValue(targetId, out var targetInternalId))
            {
                validLinks.Add(new DependencyInfo(
                    sourceInternalId,
                    targetInternalId,
                    employees.SingleOrDefault(e => e.Email == link.ChangedBy)?.Id,
                    link.ChangedOperation,
                    link.ChangedDate,
                    link.Comment));
            }
            else
            {
                LogInvalidLink(link, sourceId, targetId, workItemMap);
            }
        }

        return validLinks;
    }

    private void LogInvalidLink(IExternalWorkItemLink link, WorkItemExternalId sourceId, WorkItemExternalId targetId, Dictionary<WorkItemExternalId, Guid> workItemMap)
    {
        if (!_logger.IsEnabled(LogLevel.Debug))
            return;

        if (!workItemMap.ContainsKey(sourceId))
        {
            _logger.LogDebug(
                "Source work item {SourceId} not found in workspace {WorkspaceId}",
                sourceId.ExternalId,
                sourceId.WorkspaceExternalId);
        }
        if (!workItemMap.ContainsKey(targetId))
        {
            _logger.LogDebug(
                "Target work item {TargetId} not found for source work item {SourceId} in workspace {WorkspaceId}",
                targetId.ExternalId,
                sourceId.ExternalId,
                targetId.WorkspaceExternalId);
        }
    }

    private async Task ProcessLinksInBatches(IReadOnlyList<DependencyInfo> links, CancellationToken cancellationToken)
    {

        var orderedLinks = links
            .GroupBy(link => link.SourceId)
            .SelectMany(group => group
                .OrderBy(link => link.ChangedDate)
                .ThenBy(link => _operationPriority.GetValueOrDefault(
                    link.ChangedOperation.ToLowerInvariant(),
                    int.MaxValue)));

        var batches = orderedLinks.Chunk(DefaultBatchSize).ToList();
        var totalBatches = batches.Count;

        for (var i = 0; i < totalBatches; i++)
        {
            try
            {
                await ProcessBatch(batches[i], cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error processing batch {CurrentBatch} of {TotalBatches}. Continuing with next batch.",
                    i + 1,
                    totalBatches);
                // Continue processing remaining batches
            }
        }
    }

    private async Task ProcessBatch(IReadOnlyList<DependencyInfo> batch, CancellationToken cancellationToken)
    {
        // Get unique source work item IDs from the batch
        var sourceWorkItemIds = batch.Select(x => x.SourceId).Distinct().ToList();

        // Load the source work items with their dependencies
        var sourceWorkItems = await _workDbContext.WorkItems
            .Include(wi => wi.OutboundDependencies)
            .Where(wi => sourceWorkItemIds.Contains(wi.Id))
            .ToListAsync(cancellationToken);

        // Create a lookup for faster access
        var workItemLookup = sourceWorkItems.ToDictionary(wi => wi.Id);

        foreach (var link in batch)
        {
            if (workItemLookup.TryGetValue(link.SourceId, out var sourceWorkItem))
            {
                if (link.ChangedOperation.Equals("create", StringComparison.OrdinalIgnoreCase))
                {
                    sourceWorkItem.AddSuccessorLink(link.TargetId, link.ChangedDate, link.ChangedById, link.Comment);
                }
                else if (link.ChangedOperation.Equals("remove", StringComparison.OrdinalIgnoreCase))
                {
                    sourceWorkItem.RemoveSuccessorLink(link.TargetId, link.ChangedDate, link.ChangedById);
                }
            }
            else
            {
                _logger.LogWarning(
                    "Source work item {SourceId} not found in database while processing dependency link to {TargetId}",
                    link.SourceId,
                    link.TargetId);
            }
        }

        await _workDbContext.SaveChangesAsync(cancellationToken);
    }

    private record struct WorkItemExternalId(int ExternalId, Guid WorkspaceExternalId);

    private record DependencyInfo(
        Guid SourceId,
        Guid TargetId,
        Guid? ChangedById,
        string ChangedOperation,
        Instant ChangedDate,
        string? Comment);
}
