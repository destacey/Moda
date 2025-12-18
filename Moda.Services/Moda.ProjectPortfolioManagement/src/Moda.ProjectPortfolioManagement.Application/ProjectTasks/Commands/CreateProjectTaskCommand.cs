using Moda.Common.Application.Models;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.ProjectPortfolioManagement.Domain.Models;

namespace Moda.ProjectPortfolioManagement.Application.ProjectTasks.Commands;

public sealed record CreateProjectTaskCommand(
    Guid ProjectId,
    string Name,
    string? Description,
    ProjectTaskType Type,
    TaskPriority Priority,
    Guid? ParentId,
    Guid? TeamId,
    FlexibleDateRange? PlannedDateRange,
    LocalDate? PlannedDate,
    decimal? EstimatedEffortHours,
    List<TaskRoleAssignment>? Assignments
) : ICommand<ObjectIdAndTaskKey>;

public sealed record TaskRoleAssignment(Guid EmployeeId, TaskAssignmentRole Role);

public sealed class CreateProjectTaskCommandValidator : AbstractValidator<CreateProjectTaskCommand>
{
    public CreateProjectTaskCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.Description)
            .MaximumLength(2048);

        RuleFor(x => x.Type)
            .IsInEnum();

        RuleFor(x => x.Priority)
            .IsInEnum();

        RuleFor(x => x.ParentId)
            .Must(id => id == null || id != Guid.Empty)
            .WithMessage("ParentId cannot be an empty GUID.");

        RuleFor(x => x.TeamId)
            .Must(id => id == null || id != Guid.Empty)
            .WithMessage("TeamId cannot be an empty GUID.");

        // Milestone-specific validations
        RuleFor(x => x.PlannedDate)
            .NotNull()
            .When(x => x.Type == ProjectTaskType.Milestone)
            .WithMessage("PlannedDate is required for milestones.");

        RuleFor(x => x.PlannedDateRange)
            .Null()
            .When(x => x.Type == ProjectTaskType.Milestone)
            .WithMessage("Milestones cannot have a date range.");

        // Task-specific validations
        RuleFor(x => x.PlannedDate)
            .Null()
            .When(x => x.Type == ProjectTaskType.Task)
            .WithMessage("Tasks cannot have a single planned date. Use PlannedDateRange instead.");

        RuleFor(x => x.EstimatedEffortHours)
            .GreaterThan(0)
            .When(x => x.EstimatedEffortHours.HasValue);

        RuleFor(x => x.Assignments)
            .Must(assignments => assignments == null || assignments.All(a => a.EmployeeId != Guid.Empty))
            .WithMessage("Assignment employee IDs cannot be empty GUIDs.");
    }
}

internal sealed class CreateProjectTaskCommandHandler(
    IProjectPortfolioManagementDbContext ppmDbContext,
    ILogger<CreateProjectTaskCommandHandler> logger)
    : ICommandHandler<CreateProjectTaskCommand, ObjectIdAndTaskKey>
{
    private const string AppRequestName = nameof(CreateProjectTaskCommand);

    private readonly IProjectPortfolioManagementDbContext _ppmDbContext = ppmDbContext;
    private readonly ILogger<CreateProjectTaskCommandHandler> _logger = logger;

    public async Task<Result<ObjectIdAndTaskKey>> Handle(CreateProjectTaskCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Load the project
            var project = await _ppmDbContext.Projects
                .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);
            if (project is null)
            {
                _logger.LogInformation("Project with Id {ProjectId} not found.", request.ProjectId);
                return Result.Failure<ObjectIdAndTaskKey>("Project not found.");
            }

            // Validate parent task if specified
            ProjectTask? parentTask = null;
            if (request.ParentId.HasValue)
            {
                parentTask = await _ppmDbContext.ProjectTasks
                    .FirstOrDefaultAsync(t => t.Id == request.ParentId.Value && t.ProjectId == request.ProjectId, cancellationToken);
                if (parentTask is null)
                {
                    _logger.LogInformation("Parent task {ParentId} not found in project {ProjectId}.", request.ParentId, request.ProjectId);
                    return Result.Failure<ObjectIdAndTaskKey>("Parent task not found in this project.");
                }

                // Milestones cannot have children
                if (parentTask.Type == ProjectTaskType.Milestone)
                {
                    _logger.LogInformation("Cannot create child task under milestone {ParentId}.", request.ParentId);
                    return Result.Failure<ObjectIdAndTaskKey>("Milestones cannot have child tasks.");
                }
            }

            // Validate team if specified
            if (request.TeamId.HasValue)
            {
                var teamExists = await _ppmDbContext.PpmTeams
                    .AnyAsync(t => t.Id == request.TeamId.Value && t.IsActive, cancellationToken);
                if (!teamExists)
                {
                    _logger.LogInformation("Team {TeamId} not found or inactive.", request.TeamId);
                    return Result.Failure<ObjectIdAndTaskKey>("Team not found or inactive.");
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
                    return Result.Failure<ObjectIdAndTaskKey>("One or more employees not found.");
                }
            }

            // Prepare role assignments if specified
            Dictionary<TaskAssignmentRole, HashSet<Guid>>? assignments = null;
            if (request.Assignments is not null && request.Assignments.Count > 0)
            {
                assignments = request.Assignments
                    .GroupBy(a => a.Role)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(a => a.EmployeeId).ToHashSet()
                    );
            }

            // Get the next task key number atomically using a database query with row locking
            // This uses UPDLOCK and ROWLOCK to prevent race conditions when multiple tasks are created concurrently
            var nextKey = await _ppmDbContext.Database
                .SqlQuery<int>($@"
                    SELECT ISNULL(MAX([Key]), 0) + 1 AS [Value]
                    FROM [Ppm].[ProjectTasks] WITH (UPDLOCK, ROWLOCK)
                    WHERE [ProjectId] = {request.ProjectId}")
                .FirstOrDefaultAsync(cancellationToken);

            // Create the task through the project
            var createResult = project.CreateTask(
                nextKey,
                request.Name,
                request.Description,
                request.Type,
                request.Priority,
                request.ParentId,
                request.TeamId,
                request.PlannedDateRange,
                request.PlannedDate,
                request.EstimatedEffortHours,
                assignments
            );

            if (createResult.IsFailure)
            {
                _logger.LogError("Error creating task {TaskName} for project {ProjectId}. Error: {Error}",
                    request.Name, request.ProjectId, createResult.Error);
                return Result.Failure<ObjectIdAndTaskKey>(createResult.Error);
            }

            await _ppmDbContext.SaveChangesAsync(cancellationToken);

            var task = createResult.Value;

            _logger.LogInformation("Project task {TaskId} created with Key {TaskKey} for project {ProjectId}.",
                task.Id, task.TaskKey.Value, request.ProjectId);

            return Result.Success(new ObjectIdAndTaskKey(task.Id, task.TaskKey.Value));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception handling {CommandName} command for request {@Request}.", AppRequestName, request);
            return Result.Failure<ObjectIdAndTaskKey>($"Error handling {AppRequestName} command.");
        }
    }
}
