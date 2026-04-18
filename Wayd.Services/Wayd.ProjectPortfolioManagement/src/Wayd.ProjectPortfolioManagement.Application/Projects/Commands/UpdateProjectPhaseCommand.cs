using Wayd.ProjectPortfolioManagement.Domain.Enums;
using TaskStatus = Wayd.ProjectPortfolioManagement.Domain.Enums.TaskStatus;

namespace Wayd.ProjectPortfolioManagement.Application.Projects.Commands;

public sealed record UpdateProjectPhaseCommand(
    Guid ProjectId,
    Guid PhaseId,
    string Description,
    int Status,
    LocalDate? PlannedStart,
    LocalDate? PlannedEnd,
    decimal Progress,
    List<Guid>? AssigneeIds) : ICommand;

public sealed class UpdateProjectPhaseCommandValidator : CustomValidator<UpdateProjectPhaseCommand>
{
    public UpdateProjectPhaseCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.PhaseId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1024);
        RuleFor(x => x.Status).Must(s => Enum.IsDefined(typeof(TaskStatus), s))
            .WithMessage("Invalid status value.");
        RuleFor(x => x.Progress).InclusiveBetween(0, 100);
    }
}

internal sealed class UpdateProjectPhaseCommandHandler(
    IProjectPortfolioManagementDbContext ppmDbContext,
    ILogger<UpdateProjectPhaseCommandHandler> logger)
    : ICommandHandler<UpdateProjectPhaseCommand>
{
    private const string AppRequestName = nameof(UpdateProjectPhaseCommand);

    private readonly IProjectPortfolioManagementDbContext _ppmDbContext = ppmDbContext;
    private readonly ILogger<UpdateProjectPhaseCommandHandler> _logger = logger;

    public async Task<Result> Handle(UpdateProjectPhaseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var phase = await _ppmDbContext.ProjectPhases
                .Include(p => p.Roles)
                .FirstOrDefaultAsync(p => p.ProjectId == request.ProjectId && p.Id == request.PhaseId, cancellationToken);

            if (phase is null)
            {
                _logger.LogInformation("Project Phase {PhaseId} not found for Project {ProjectId}.", request.PhaseId, request.ProjectId);
                return Result.Failure($"Project Phase {request.PhaseId} not found.");
            }

            var descriptionResult = phase.UpdateDescription(request.Description);
            if (descriptionResult.IsFailure)
                return descriptionResult;

            var statusResult = phase.UpdateStatus((TaskStatus)request.Status);
            if (statusResult.IsFailure)
                return statusResult;

            FlexibleDateRange? dateRange = request.PlannedStart.HasValue && request.PlannedEnd.HasValue
                ? new FlexibleDateRange(request.PlannedStart.Value, request.PlannedEnd.Value)
                : null;

            var datesResult = phase.UpdatePlannedDates(dateRange);
            if (datesResult.IsFailure)
                return datesResult;

            var progressResult = phase.UpdateProgress(new Progress(request.Progress));
            if (progressResult.IsFailure)
                return progressResult;

            if (request.AssigneeIds is not null)
            {
                var updatedRoles = new Dictionary<ProjectPhaseRole, HashSet<Guid>>
                {
                    { ProjectPhaseRole.Assignee, [.. request.AssigneeIds] }
                };

                var rolesResult = phase.UpdateRoles(updatedRoles);
                if (rolesResult.IsFailure)
                    return rolesResult;
            }

            await _ppmDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Project Phase {PhaseId} updated for Project {ProjectId}.", request.PhaseId, request.ProjectId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception handling {CommandName} command for request {@Request}.", AppRequestName, request);
            return Result.Failure($"Error handling {AppRequestName} command.");
        }
    }
}
