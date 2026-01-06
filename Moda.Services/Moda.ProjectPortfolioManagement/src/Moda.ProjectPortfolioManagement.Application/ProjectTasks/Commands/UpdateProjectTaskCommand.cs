using Moda.ProjectPortfolioManagement.Domain.Enums;

namespace Moda.ProjectPortfolioManagement.Application.ProjectTasks.Commands;

public sealed record UpdateProjectTaskCommand(
    Guid Id,
    string Name,
    string? Description,
    Domain.Enums.TaskStatus Status,
    TaskPriority Priority,
    Progress? Progress,
    Guid? ParentId,
    FlexibleDateRange? PlannedDateRange,
    LocalDate? PlannedDate,
    decimal? EstimatedEffortHours,
    List<TaskRoleAssignment>? Assignments
) : ICommand;

public sealed class UpdateProjectTaskCommandValidator : AbstractValidator<UpdateProjectTaskCommand>
{
    public UpdateProjectTaskCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.Description)
            .MaximumLength(2048);

        RuleFor(x => x.Status)
            .IsInEnum();

        RuleFor(x => x.Priority)
            .IsInEnum();

        RuleFor(x => x.ParentId)
            .Must(id => id == null || id != Guid.Empty)
            .WithMessage("ParentId cannot be an empty GUID.");

        RuleFor(x => x.EstimatedEffortHours)
            .GreaterThan(0)
            .When(x => x.EstimatedEffortHours.HasValue);

        RuleFor(x => x.Assignments)
            .Must(assignments => assignments == null || assignments.All(a => a.EmployeeId != Guid.Empty))
            .WithMessage("Assignment employee IDs cannot be empty GUIDs.");
    }
}

internal sealed class UpdateProjectTaskCommandHandler(
    IProjectPortfolioManagementDbContext ppmDbContext,
    ILogger<UpdateProjectTaskCommandHandler> logger,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<UpdateProjectTaskCommand>
{
    private const string AppRequestName = nameof(UpdateProjectTaskCommand);

    private readonly IProjectPortfolioManagementDbContext _ppmDbContext = ppmDbContext;
    private readonly ILogger<UpdateProjectTaskCommandHandler> _logger = logger;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

    public async Task<Result> Handle(UpdateProjectTaskCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var task = await _ppmDbContext.ProjectTasks
                .Include(t => t.Roles)
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
            if (task is null)
            {
                _logger.LogInformation("Project task {TaskId} not found.", request.Id);
                return Result.Failure("Project task not found.");
            }

            // Validate parent if specified
            if (request.ParentId.HasValue)
            {
                // Cannot set self as parent
                if (request.ParentId.Value == request.Id)
                {
                    _logger.LogInformation("Cannot set task {TaskId} as its own parent.", request.Id);
                    return Result.Failure("A task cannot be its own parent.");
                }

                var parentExists = await _ppmDbContext.ProjectTasks
                    .AnyAsync(t => t.Id == request.ParentId.Value, cancellationToken);
                if (!parentExists)
                {
                    _logger.LogInformation("Parent task {ParentId} not found.", request.ParentId);
                    return Result.Failure("Parent task not found.");
                }

                // Check for circular reference (would create a cycle in the hierarchy)
                // Need to check if the new parent is a descendant of the current task
                var descendants = await GetDescendantIds(request.Id, cancellationToken);
                if (descendants.Contains(request.ParentId.Value))
                {
                    _logger.LogInformation("Cannot set task {ParentId} as parent of {TaskId} - would create circular reference.", request.ParentId, request.Id);
                    return Result.Failure("Cannot set a descendant task as parent - this would create a circular reference.");
                }
            }

            // Validate employees if assignments specified
            if (request.Assignments is not null && request.Assignments.Count > 0)
            {
                var employeeIds = request.Assignments.Select(a => a.EmployeeId).ToHashSet();
                var employees = await _ppmDbContext.Employees
                    .Where(e => employeeIds.Contains(e.Id))
                    .Select(e => e.Id)
                    .ToListAsync(cancellationToken);

                if (employees.Count != employeeIds.Count)
                {
                    _logger.LogInformation("One or more employees not found.");
                    return Result.Failure("One or more employees not found.");
                }
            }

            // Update basic details
            var detailsResult = task.UpdateDetails(request.Name, request.Description, request.Priority);
            if (detailsResult.IsFailure)
            {
                _logger.LogError("Error updating task {TaskId} details. Error: {Error}", task.Id, detailsResult.Error);
                return detailsResult;
            }

            // Update status
            var statusResult = task.UpdateStatus(request.Status, _dateTimeProvider.Now);
            if (statusResult.IsFailure)
            {
                _logger.LogError("Error updating task {TaskId} status. Error: {Error}", task.Id, statusResult.Error);
                return statusResult;
            }

            // Update progress
            if (task.Type is ProjectTaskType.Task)
            {
                if(request.Progress is null)
                {
                    _logger.LogInformation("Progress must be provided for task type 'Task'.");
                    return Result.Failure("Progress must be provided for task type 'Task'.");
                }

                var progressResult = task.UpdateProgress(request.Progress);
                if (progressResult.IsFailure)
                {
                    _logger.LogError("Error updating task {TaskId} progress. Error: {Error}", task.Id, progressResult.Error);
                    return progressResult;
                }
            }

            // Update planned dates
            var plannedDatesResult = task.UpdatePlannedDates(request.PlannedDateRange, request.PlannedDate);
            if (plannedDatesResult.IsFailure)
            {
                _logger.LogError("Error updating task {TaskId} planned dates. Error: {Error}", task.Id, plannedDatesResult.Error);
                return plannedDatesResult;
            }

            // Update effort
            var effortResult = task.UpdateEffort(request.EstimatedEffortHours);
            if (effortResult.IsFailure)
            {
                _logger.LogError("Error updating task {TaskId} effort. Error: {Error}", task.Id, effortResult.Error);
                return effortResult;
            }

            // Update parent if changed
            if (request.ParentId != task.ParentId)
            {
                // get the project with all tasks to perform the parent change
                var project = await _ppmDbContext.Projects
                    .Include(p => p.Tasks)
                    .FirstOrDefaultAsync(p => p.Id == task.ProjectId, cancellationToken);
                if (project is null)
                {
                    _logger.LogError("Project with Id {ProjectId} not found for task {TaskId}.", task.ProjectId, task.Id);
                    return Result.Failure("Project not found for the task.");
                }

                var parentResult = project.ChangeTaskPlacement(task.Id, request.ParentId, null);
                if (parentResult.IsFailure)
                {
                    _logger.LogError("Error changing parent for task {TaskId}. Error: {Error}", task.Id, parentResult.Error);
                    return parentResult;
                }
            }

            // Update role assignments if specified
            if (request.Assignments is not null)
            {
                var assignments = request.Assignments
                    .GroupBy(a => a.Role)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(a => a.EmployeeId).ToHashSet()
                    );

                var assignResult = task.UpdateRoles(assignments);
                if (assignResult.IsFailure)
                {
                    _logger.LogError("Error updating task {TaskId} role assignments. Error: {Error}", task.Id, assignResult.Error);
                    return assignResult;
                }
            }

            await _ppmDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Project task {TaskId} updated successfully.", task.Id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception handling {CommandName} command for request {@Request}.", AppRequestName, request);
            return Result.Failure($"Error handling {AppRequestName} command.");
        }
    }

    private async Task<HashSet<Guid>> GetDescendantIds(Guid taskId, CancellationToken cancellationToken)
    {
        var descendants = new HashSet<Guid>();
        var children = await _ppmDbContext.ProjectTasks
            .Where(t => t.ParentId == taskId)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);

        foreach (var childId in children)
        {
            descendants.Add(childId);
            var childDescendants = await GetDescendantIds(childId, cancellationToken);
            descendants.UnionWith(childDescendants);
        }

        return descendants;
    }
}
