using Moda.Common.Domain.Enums.Work;
using Moda.Work.Application.Persistence;
using Moda.Work.Application.WorkItems.Dtos;

namespace Moda.Work.Application.WorkItems.Commands;

public sealed record UpdateWorkItemProjectCommand(WorkItemKey WorkItemKey, Guid? ProjectId) : ICommand;

public sealed class UpdateWorkItemProjectCommandValidator : AbstractValidator<UpdateWorkItemProjectCommand>
{
    public UpdateWorkItemProjectCommandValidator()
    {
        RuleFor(x => x.WorkItemKey)
            .NotNull();

        When(x => x.ProjectId.HasValue, () =>
        {
            RuleFor(x => x.ProjectId)
                .NotEmpty();
        });
    }
}

internal sealed class UpdateWorkItemProjectCommandHandler(
    IWorkDbContext workDbContext,
    ILogger<UpdateWorkItemProjectCommandHandler> logger) : ICommandHandler<UpdateWorkItemProjectCommand>
{
    private const string AppRequestName = nameof(UpdateWorkItemProjectCommand);

    private readonly IWorkDbContext _workDbContext = workDbContext;
    private readonly ILogger<UpdateWorkItemProjectCommandHandler> _logger = logger;
    public async Task<Result> Handle(UpdateWorkItemProjectCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var workItem = await _workDbContext.WorkItems
                .Include(i => i.Type)
                    .ThenInclude(t => t.Level)
                .FirstOrDefaultAsync(w => w.Key == request.WorkItemKey, cancellationToken);
            if (workItem is null)
            {
                _logger.LogInformation("Work item {WorkItemKey} not found.", request.WorkItemKey);
                return Result.Failure("Work item not found.");
            }

            var result = workItem.UpdateProjectId(request.ProjectId);
            if (result.IsFailure)
            {
                // Reset the entity
                await _workDbContext.Entry(workItem).ReloadAsync(cancellationToken);
                workItem.ClearDomainEvents();

                _logger.LogError("Unable to update work item {WorkItemKey} project id to {ProjectId}. Error message: {Error}", request.WorkItemKey, request.ProjectId?.ToString() ?? "null", result.Error);
                return Result.Failure(result.Error);
            }

            await _workDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Work item {WorkItemKey} project id updated.", request.WorkItemKey);

            await UpdateChildren(workItem.Adapt<WorkItemParentInfo>(), cancellationToken);

            _logger.LogInformation("Work item {WorkItemKey} descendants updated.", request.WorkItemKey);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception handling {CommandName} command for request {@Request}.", AppRequestName, request);
            return Result.Failure($"Error handling {AppRequestName} command.");
        }
    }

    // TODO: this should be moved to an event handler
    private async Task UpdateChildren(WorkItemParentInfo parent, CancellationToken cancellationToken)
    {
        if (parent.Tier != WorkTypeTier.Portfolio)
        {
            return;
        }

        var children = await _workDbContext.WorkItems
            .Include(wi => wi.Type)
                .ThenInclude(t => t.Level)
            .Where(wi => wi.ParentId == parent.Id)
            .ToListAsync(cancellationToken);

        if (children.Count == 0)
            return;


        int updateCount = 0;

        foreach (var child in children)
        {
            var result = child.UpdateParent(parent, child.Type);
            if (result.IsFailure)
            {
                _logger.LogError("Error updating parent for work item {WorkItemId}. {Error}", child.Id, result.Error);
                continue;
            }

            updateCount++;
        }

        if (updateCount > 0)
        {
            await _workDbContext.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Updated {UpdateCount} children for work item {WorkItemId}.", updateCount, parent.Id);
        }

        foreach (var child in children)
        {
            if (child.ProjectId is null)
                await UpdateChildren(child.Adapt<WorkItemParentInfo>(), cancellationToken);
        }
    }
}