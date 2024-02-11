using MediatR;
using Moda.Goals.Application.Objectives.Commands;

namespace Moda.Planning.Application.PlanningIntervals.Commands;
public sealed record CreatePlanningIntervalObjectiveCommand(Guid PlanningIntervalId, Guid TeamId, string Name, string? Description, LocalDate? StartDate, LocalDate? TargetDate, bool IsStretch) : ICommand<int>;

public sealed class CreatePlanningIntervalObjectiveCommandValidator : CustomValidator<CreatePlanningIntervalObjectiveCommand>
{
    private readonly IPlanningDbContext _planningDbContext;

    public CreatePlanningIntervalObjectiveCommandValidator(IPlanningDbContext planningDbContext)
    {
        _planningDbContext = planningDbContext;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(o => o.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(o => o.Description)
            .MaximumLength(1024);

        When(o => o.StartDate.HasValue && o.TargetDate.HasValue, () =>
        {
            RuleFor(o => o.StartDate)
                .LessThan(o => o.TargetDate)
                .WithMessage("The start date must be before the target date.");
        });

        RuleFor(o => o.StartDate)
            .MustAsync(BeWithinPlanningIntervalDates)
            .WithMessage("The start date must be within the Planning Interval dates.");

        RuleFor(o => o.TargetDate)
            .MustAsync(BeWithinPlanningIntervalDates)
            .WithMessage("The target date must be within the Planning Interval dates.");
    }

    public async Task<bool> BeWithinPlanningIntervalDates(CreatePlanningIntervalObjectiveCommand command, LocalDate? date, CancellationToken cancellationToken)
    {
        if (!date.HasValue)
            return true;

        var planningInterval = await _planningDbContext.PlanningIntervals
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == command.PlanningIntervalId, cancellationToken);

        return planningInterval is null
            ? false
            : planningInterval.DateRange.Start <= date
                && date <= planningInterval.DateRange.End;
    }
}

internal sealed class CreatePlanningIntervalObjectiveCommandHandler : ICommandHandler<CreatePlanningIntervalObjectiveCommand, int>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly ISender _sender;
    private readonly ILogger<CreatePlanningIntervalObjectiveCommandHandler> _logger;

    public CreatePlanningIntervalObjectiveCommandHandler(IPlanningDbContext planningDbContext, ISender sender, ILogger<CreatePlanningIntervalObjectiveCommandHandler> logger)
    {
        _planningDbContext = planningDbContext;
        _sender = sender;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(CreatePlanningIntervalObjectiveCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var planningInterval = await _planningDbContext.PlanningIntervals
                .FirstOrDefaultAsync(p => p.Id == request.PlanningIntervalId, cancellationToken);
            if (planningInterval is null)
            {
                _logger.LogError("Planning Interval {PlanningIntervalId} not found.", request.PlanningIntervalId);
                return Result.Failure<int>("Planning Interval not found.");
            }

            if (planningInterval.ObjectivesLocked)
            {
                _logger.LogError("Objectives are locked for the Planning Interval {PlanningIntervalId}", request.PlanningIntervalId);
                return Result.Failure<int>("Objectives are locked for the Planning Interval");
            }

            var team = await _planningDbContext.PlanningTeams
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == request.TeamId, cancellationToken);
            if (team is null)
            {
                _logger.LogError("Team {TeamId} not found.", request.TeamId);
                return Result.Failure<int>("Team not found.");
            }

            var objectiveResult = await _sender.Send(new CreateObjectiveCommand(
                request.Name,
                request.Description,
                Goals.Domain.Enums.ObjectiveType.PlanningInterval,
                request.TeamId,
                request.PlanningIntervalId,
                request.StartDate,
                request.TargetDate), cancellationToken);
            if (objectiveResult.IsFailure)
            {
                _logger.LogError("Unable to create objective.  Error: {Error}", objectiveResult.Error);
                return Result.Failure<int>($"Unable to create objective.  Error: {objectiveResult.Error}");
            }

            var result = planningInterval.CreateObjective(team, objectiveResult.Value, request.IsStretch);
            if (result.IsFailure)
            {
                var deleteResult = await _sender.Send(new DeleteObjectiveCommand(objectiveResult.Value), cancellationToken);
                if (deleteResult.IsFailure)
                    _logger.LogError("Unable to delete objective.  Error: {Error}", deleteResult.Error);

                _logger.LogError("Unable to create PI objective.  Error: {Error}", result.Error);
                return Result.Failure<int>($"Unable to PI create objective.  Error: {result.Error}");
            }

            await _planningDbContext.SaveChangesAsync(cancellationToken);

            var key = planningInterval.Objectives
                .First(o => o.ObjectiveId == objectiveResult.Value)
                .Key;

            return Result.Success(key);
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure<int>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}
