namespace Moda.ProjectPortfolioManagement.Application.ProjectTasks.Commands;

/// <summary>
/// Updates the order of a task within its parent (for reordering siblings).
/// </summary>
public sealed record UpdateProjectTaskOrderCommand(Guid TaskId, int Order) : ICommand;

public sealed class UpdateProjectTaskOrderCommandValidator : CustomValidator<UpdateProjectTaskOrderCommand>
{
    public UpdateProjectTaskOrderCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty();

        RuleFor(x => x.Order)
            .GreaterThan(0)
            .WithMessage("Order must be greater than 0.");
    }
}

internal sealed class UpdateProjectTaskOrderCommandHandler(
    IProjectPortfolioManagementDbContext ppmDbContext,
    ILogger<UpdateProjectTaskOrderCommandHandler> logger)
    : ICommandHandler<UpdateProjectTaskOrderCommand>
{
    private const string AppRequestName = nameof(UpdateProjectTaskOrderCommand);

    private readonly IProjectPortfolioManagementDbContext _ppmDbContext = ppmDbContext;
    private readonly ILogger<UpdateProjectTaskOrderCommandHandler> _logger = logger;

    public async Task<Result> Handle(UpdateProjectTaskOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var task = await _ppmDbContext.ProjectTasks
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

        if (task is null)
        {
            _logger.LogWarning("Project task {TaskId} not found.", request.TaskId);
            return Result.Failure("Project task not found.");
        }

        var result = task.SetOrder(request.Order);
        if (result.IsFailure)
        {
            _logger.LogError("Failed to update order for task {TaskId}. Error: {Error}",
                task.Id, result.Error);
            return result;
        }

        await _ppmDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully updated order for task {TaskId} to {Order}.",
            task.Id, request.Order);

        return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception handling {CommandName} command for request {@Request}.", AppRequestName, request);
            return Result.Failure($"Error handling {AppRequestName} command.");
        }
    }
}
