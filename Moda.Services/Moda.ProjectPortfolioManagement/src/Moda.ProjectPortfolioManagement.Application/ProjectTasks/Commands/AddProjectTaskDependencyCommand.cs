using Moda.ProjectPortfolioManagement.Domain.Models;

namespace Moda.ProjectPortfolioManagement.Application.ProjectTasks.Commands;

/// <summary>
/// Adds a dependency between two tasks (Finish-to-Start).
/// </summary>
public sealed record AddProjectTaskDependencyCommand(Guid PredecessorId, Guid SuccessorId) : ICommand;

internal sealed class AddProjectTaskDependencyCommandHandler(
    IProjectPortfolioManagementDbContext ppmDbContext,
    ILogger<AddProjectTaskDependencyCommandHandler> logger)
    : ICommandHandler<AddProjectTaskDependencyCommand>
{
    private readonly IProjectPortfolioManagementDbContext _ppmDbContext = ppmDbContext;
    private readonly ILogger<AddProjectTaskDependencyCommandHandler> _logger = logger;

    public async Task<Result> Handle(AddProjectTaskDependencyCommand request, CancellationToken cancellationToken)
    {
        // Load both tasks
        var predecessor = await _ppmDbContext.ProjectTasks
            .Include(t => t.Successors)
            .FirstOrDefaultAsync(t => t.Id == request.PredecessorId, cancellationToken);

        if (predecessor is null)
        {
            var message = $"Predecessor task with ID {request.PredecessorId} not found.";
            _logger.LogWarning("AddProjectTaskDependency: {Message}", message);
            return Result.Failure(message);
        }

        var successor = await _ppmDbContext.ProjectTasks
            .FirstOrDefaultAsync(t => t.Id == request.SuccessorId, cancellationToken);

        if (successor is null)
        {
            var message = $"Successor task with ID {request.SuccessorId} not found.";
            _logger.LogWarning("AddProjectTaskDependency: {Message}", message);
            return Result.Failure(message);
        }

        var result = predecessor.AddDependency(successor);
        if (result.IsFailure)
        {
            _logger.LogWarning("AddProjectTaskDependency: Failed to add dependency from {PredecessorKey} to {SuccessorKey}. Error: {Error}",
                predecessor.TaskKey.Value, successor.TaskKey.Value, result.Error);
            return result;
        }

        await _ppmDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("AddProjectTaskDependency: Successfully added dependency from {PredecessorKey} to {SuccessorKey}.",
            predecessor.TaskKey.Value, successor.TaskKey.Value);

        return Result.Success();
    }
}

public sealed class AddProjectTaskDependencyCommandValidator : CustomValidator<AddProjectTaskDependencyCommand>
{
    public AddProjectTaskDependencyCommandValidator()
    {
        RuleFor(x => x.PredecessorId)
            .NotEmpty();

        RuleFor(x => x.SuccessorId)
            .NotEmpty();

        RuleFor(x => x)
            .Must(x => x.PredecessorId != x.SuccessorId)
            .WithMessage("A task cannot depend on itself.");
    }
}
