using Moda.Common.Application.Interfaces;

namespace Moda.ProjectPortfolioManagement.Application.ProjectTasks.Commands;

/// <summary>
/// Removes (soft deletes) a dependency between two tasks.
/// </summary>
public sealed record RemoveProjectTaskDependencyCommand(Guid PredecessorId, Guid SuccessorId) : ICommand;

internal sealed class RemoveProjectTaskDependencyCommandHandler(
    IProjectPortfolioManagementDbContext ppmDbContext,
    IDateTimeProvider dateTimeProvider,
    ILogger<RemoveProjectTaskDependencyCommandHandler> logger)
    : ICommandHandler<RemoveProjectTaskDependencyCommand>
{
    private readonly IProjectPortfolioManagementDbContext _ppmDbContext = ppmDbContext;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly ILogger<RemoveProjectTaskDependencyCommandHandler> _logger = logger;

    public async Task<Result> Handle(RemoveProjectTaskDependencyCommand request, CancellationToken cancellationToken)
    {
        var predecessor = await _ppmDbContext.ProjectTasks
            .Include(t => t.Successors)
            .FirstOrDefaultAsync(t => t.Id == request.PredecessorId, cancellationToken);

        if (predecessor is null)
        {
            var message = $"Predecessor task with ID {request.PredecessorId} not found.";
            _logger.LogWarning("RemoveProjectTaskDependency: {Message}", message);
            return Result.Failure(message);
        }

        var result = predecessor.RemoveDependency(request.SuccessorId, _dateTimeProvider.Now);
        if (result.IsFailure)
        {
            _logger.LogWarning("RemoveProjectTaskDependency: Failed to remove dependency from {PredecessorId} to {SuccessorId}. Error: {Error}",
                request.PredecessorId, request.SuccessorId, result.Error);
            return result;
        }

        await _ppmDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("RemoveProjectTaskDependency: Successfully removed dependency from {PredecessorKey} to successor {SuccessorId}.",
            predecessor.Key.Value, request.SuccessorId);

        return Result.Success();
    }
}

public sealed class RemoveProjectTaskDependencyCommandValidator : CustomValidator<RemoveProjectTaskDependencyCommand>
{
    public RemoveProjectTaskDependencyCommandValidator()
    {
        RuleFor(x => x.PredecessorId)
            .NotEmpty();

        RuleFor(x => x.SuccessorId)
            .NotEmpty();
    }
}
