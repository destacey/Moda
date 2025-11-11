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

        var map = new Dictionary<WorkItemExternalId, Guid>(workItems.Count);

        foreach (var wi in workItems)
        {
            if (!Guid.TryParse(wi.WorkspaceExternalId, out var parsedWorkspaceId))
                continue;

            var key = new WorkItemExternalId(wi.ExternalId, parsedWorkspaceId);
            if (uniqueIds.Contains(key))
                map[key] = wi.Id;
        }

        return map;
    }

    private async Task<List<DependencyInfo>> MapValidLinks(IReadOnlyList<IExternalWorkItemLink> workItemLinks, Dictionary<WorkItemExternalId, Guid> workItemMap, CancellationToken cancellationToken)
    {
        var employeeDict = await _workDbContext.Employees
            .Select(e => new { Email = e.Email.ToString(), e.Id })
            .ToDictionaryAsync(e => e.Email, e => e.Id, cancellationToken);

        var employeeByEmail = new Dictionary<string, Guid>(employeeDict, StringComparer.OrdinalIgnoreCase);

        var validLinks = new List<DependencyInfo>();

        foreach (var link in workItemLinks)
        {
            var sourceId = new WorkItemExternalId(link.SourceId, link.SourceWorkspaceId);
            var targetId = new WorkItemExternalId(link.TargetId, link.TargetWorkspaceId);

            if (workItemMap.TryGetValue(sourceId, out var sourceInternalId) &&
                workItemMap.TryGetValue(targetId, out var targetInternalId))
            {
                Guid? changedById = null;
                if (link.ChangedBy != null && employeeByEmail.TryGetValue(link.ChangedBy!, out var empId))
                {
                    changedById = empId;
                }

                validLinks.Add(new DependencyInfo(
                    sourceInternalId,
                    targetInternalId,
                    changedById,
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
        var sourceWorkItemIds = batch.Select(x => x.SourceId).ToHashSet();
        var targetIds = batch.Select(x => x.TargetId).ToHashSet();

        // Load the source work items with only the outbound dependencies that are relevant to this batch
        var workItemLookup = await _workDbContext.WorkItems
            .Where(wi => sourceWorkItemIds.Contains(wi.Id))
            .Include(wi => wi.OutboundDependencies.Where(d => targetIds.Contains(d.TargetId)))
            .ToDictionaryAsync(wi => wi.Id, cancellationToken);

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
