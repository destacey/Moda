namespace Moda.ProjectPortfolioManagement.Application.ProjectTasks.Commands;

/// <summary>
/// Moves a task to a new parent in the hierarchy.
/// </summary>
public sealed record MoveProjectTaskCommand(Guid TaskId, Guid ProjectId, Guid? NewParentId) : ICommand;

public sealed class MoveProjectTaskCommandValidator : CustomValidator<MoveProjectTaskCommand>
{
    public MoveProjectTaskCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty();
        RuleFor(x => x.ProjectId)
            .NotEmpty();
        RuleFor(x => x.NewParentId)
            .Must(id => id == null || id != Guid.Empty)
            .WithMessage("NewParentId cannot be an empty GUID.");
    }
}

internal sealed class MoveProjectTaskCommandHandler(
    IProjectPortfolioManagementDbContext ppmDbContext,
    ILogger<MoveProjectTaskCommandHandler> logger)
    : ICommandHandler<MoveProjectTaskCommand>
{
    private const string AppRequestName = nameof(MoveProjectTaskCommand);

    private readonly IProjectPortfolioManagementDbContext _ppmDbContext = ppmDbContext;
    private readonly ILogger<MoveProjectTaskCommandHandler> _logger = logger;

    public async Task<Result> Handle(MoveProjectTaskCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var taskExists = await _ppmDbContext.ProjectTasks
            .AnyAsync(t => t.Id == request.TaskId, cancellationToken);
            if (!taskExists)
            {
                _logger.LogInformation("Project Task {TaskId} not found.", request.TaskId);
                return Result.Failure($"Project Task not found.");
            }

            var project = await _ppmDbContext.Projects
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);
            if (project is null)
            {
                _logger.LogInformation("Project {ProjectId} not found.", request.ProjectId);
                return Result.Failure($"Project {request.ProjectId} not found.");
            }

            var parentResult = project.ChangeTaskParent(request.TaskId, request.NewParentId);
            if (parentResult.IsFailure)
            {
                _logger.LogError("Error changing parent for task {TaskId}. Error: {Error}", request.TaskId, parentResult.Error);
                return parentResult;
            }

            await _ppmDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully changed parent for task {TaskId} to {NewParentId}.", request.TaskId, request.NewParentId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception handling {CommandName} command for request {@Request}.", AppRequestName, request);
            return Result.Failure($"Error handling {AppRequestName} command.");
        }
    }
}
