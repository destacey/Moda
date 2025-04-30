using Moda.Work.Application.Persistence;

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

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception handling {CommandName} command for request {@Request}.", AppRequestName, request);
            return Result.Failure($"Error handling {AppRequestName} command.");
        }
    }
}