namespace Moda.ProjectPortfolioManagement.Application.ProjectTasks.Commands;

/// <summary>
/// Updates the order of a task within its parent (for reordering siblings).
/// </summary>
public sealed record UpdateProjectTaskOrderCommand(Guid TaskId, int Order) : ICommand;

internal sealed class UpdateProjectTaskOrderCommandHandler(
    IProjectPortfolioManagementDbContext ppmDbContext,
    ILogger<UpdateProjectTaskOrderCommandHandler> logger)
    : ICommandHandler<UpdateProjectTaskOrderCommand>
{
    private readonly IProjectPortfolioManagementDbContext _ppmDbContext = ppmDbContext;
    private readonly ILogger<UpdateProjectTaskOrderCommandHandler> _logger = logger;

    public async Task<Result> Handle(UpdateProjectTaskOrderCommand request, CancellationToken cancellationToken)
    {
        var task = await _ppmDbContext.ProjectTasks
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

        if (task is null)
        {
            var message = $"ProjectTask with ID {request.TaskId} not found.";
            _logger.LogWarning("UpdateProjectTaskOrder: {Message}", message);
            return Result.Failure(message);
        }

        var result = task.SetOrder(request.Order);
        if (result.IsFailure)
        {
            _logger.LogWarning("UpdateProjectTaskOrder: Failed to update order for task {TaskId}. Error: {Error}",
                task.Id, result.Error);
            return result;
        }

        await _ppmDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("UpdateProjectTaskOrder: Successfully updated order for task {TaskKey} to {Order}.",
            task.TaskKey.Value, request.Order);

        return Result.Success();
    }
}

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
