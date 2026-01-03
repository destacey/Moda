using Moda.ProjectPortfolioManagement.Domain.Models;

namespace Moda.ProjectPortfolioManagement.Application.ProjectTasks.Commands;

/// <summary>
/// Moves a task to a new parent in the hierarchy.
/// </summary>
public sealed record MoveProjectTaskCommand(Guid TaskId, Guid? NewParentId) : ICommand;

internal sealed class MoveProjectTaskCommandHandler(
    IProjectPortfolioManagementDbContext ppmDbContext,
    ILogger<MoveProjectTaskCommandHandler> logger)
    : ICommandHandler<MoveProjectTaskCommand>
{
    private readonly IProjectPortfolioManagementDbContext _ppmDbContext = ppmDbContext;
    private readonly ILogger<MoveProjectTaskCommandHandler> _logger = logger;

    public async Task<Result> Handle(MoveProjectTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _ppmDbContext.ProjectTasks
            .Include(t => t.Parent)
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

        if (task is null)
        {
            var message = $"ProjectTask with ID {request.TaskId} not found.";
            _logger.LogWarning("MoveProjectTask: {Message}", message);
            return Result.Failure(message);
        }

        // Validate new parent exists if specified
        ProjectTask? newParent = null;
        if (request.NewParentId.HasValue)
        {
            newParent = await _ppmDbContext.ProjectTasks
                .FirstOrDefaultAsync(t => t.Id == request.NewParentId.Value, cancellationToken);

            if (newParent is null)
            {
                var message = $"Parent task with ID {request.NewParentId.Value} not found.";
                _logger.LogWarning("MoveProjectTask: {Message}", message);
                return Result.Failure(message);
            }

            // Verify tasks are in the same project
            if (task.ProjectId != newParent.ProjectId)
            {
                var message = "Cannot move task to a parent in a different project.";
                _logger.LogWarning("MoveProjectTask: {Message} TaskProject={TaskProject}, ParentProject={ParentProject}",
                    message, task.ProjectId, newParent.ProjectId);
                return Result.Failure(message);
            }
        }

        var result = task.ChangeParent(request.NewParentId, newParent);
        if (result.IsFailure)
        {
            _logger.LogWarning("MoveProjectTask: Failed to move task {TaskId} to parent {NewParentId}. Error: {Error}",
                task.Id, request.NewParentId, result.Error);
            return result;
        }

        await _ppmDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("MoveProjectTask: Successfully moved task {TaskKey} to parent {NewParentKey}.",
            task.Key.Value, newParent?.Key.Value ?? "root");

        return Result.Success();
    }
}

public sealed class MoveProjectTaskCommandValidator : CustomValidator<MoveProjectTaskCommand>
{
    public MoveProjectTaskCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty();

        RuleFor(x => x.NewParentId)
            .Must(id => id == null || id != Guid.Empty)
            .WithMessage("NewParentId cannot be an empty GUID.");
    }
}
