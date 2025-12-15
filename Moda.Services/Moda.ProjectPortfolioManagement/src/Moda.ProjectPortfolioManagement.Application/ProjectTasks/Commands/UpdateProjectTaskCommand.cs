using Moda.ProjectPortfolioManagement.Domain.Enums;

namespace Moda.ProjectPortfolioManagement.Application.ProjectTasks.Commands;

public sealed record UpdateProjectTaskCommand(
    Guid Id,
    string Name,
    string? Description,
    Domain.Enums.TaskStatus Status,
    TaskPriority? Priority,
    Guid? TeamId,
    FlexibleDateRange? PlannedDateRange,
    LocalDate? PlannedDate,
    FlexibleDateRange? ActualDateRange,
    LocalDate? ActualDate,
    decimal? EstimatedEffortHours,
    decimal? ActualEffortHours,
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
            .MaximumLength(256);

        RuleFor(x => x.Description)
            .MaximumLength(2048);

        RuleFor(x => x.Status)
            .IsInEnum();

        RuleFor(x => x.Priority)
            .IsInEnum()
            .When(x => x.Priority.HasValue);

        RuleFor(x => x.TeamId)
            .Must(id => id == null || id != Guid.Empty)
            .WithMessage("TeamId cannot be an empty GUID.");

        RuleFor(x => x.EstimatedEffortHours)
            .GreaterThan(0)
            .When(x => x.EstimatedEffortHours.HasValue);

        RuleFor(x => x.ActualEffortHours)
            .GreaterThan(0)
            .When(x => x.ActualEffortHours.HasValue);

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
                .Include(t => t.Assignments)
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

            if (task is null)
            {
                _logger.LogInformation("Project task with Id {TaskId} not found.", request.Id);
                return Result.Failure("Project task not found.");
            }

            // Validate team if specified
            if (request.TeamId.HasValue)
            {
                var teamExists = await _ppmDbContext.PpmTeams
                    .AnyAsync(t => t.Id == request.TeamId.Value && t.IsActive, cancellationToken);

                if (!teamExists)
                {
                    _logger.LogInformation("Team {TeamId} not found or inactive.", request.TeamId);
                    return Result.Failure("Team not found or inactive.");
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

            // Update planned dates
            var plannedDatesResult = task.UpdatePlannedDates(request.PlannedDateRange, request.PlannedDate);
            if (plannedDatesResult.IsFailure)
            {
                _logger.LogError("Error updating task {TaskId} planned dates. Error: {Error}", task.Id, plannedDatesResult.Error);
                return plannedDatesResult;
            }

            // Update actual dates
            var actualDatesResult = task.UpdateActualDates(request.ActualDateRange, request.ActualDate);
            if (actualDatesResult.IsFailure)
            {
                _logger.LogError("Error updating task {TaskId} actual dates. Error: {Error}", task.Id, actualDatesResult.Error);
                return actualDatesResult;
            }

            // Update effort
            var effortResult = task.UpdateEffort(request.EstimatedEffortHours, request.ActualEffortHours);
            if (effortResult.IsFailure)
            {
                _logger.LogError("Error updating task {TaskId} effort. Error: {Error}", task.Id, effortResult.Error);
                return effortResult;
            }

            // Update team assignment
            var teamResult = task.AssignTeam(request.TeamId);
            if (teamResult.IsFailure)
            {
                _logger.LogError("Error assigning team to task {TaskId}. Error: {Error}", task.Id, teamResult.Error);
                return teamResult;
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
}
