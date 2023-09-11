using MediatR;
using Moda.Goals.Application.Objectives.Commands;
using Moda.Goals.Application.Objectives.Queries;
using Moda.Goals.Domain.Enums;

namespace Moda.Planning.Application.ProgramIncrements.Commands;
public sealed record UpdateProgramIncrementObjectiveCommand(Guid ProgramIncrementId, Guid ProgramIncrementObjectiveId, string Name, string? Description, ObjectiveStatus Status, double Progress, LocalDate? StartDate, LocalDate? TargetDate, bool IsStretch) : ICommand<int>;

public sealed class UpdateProgramIncrementObjectiveCommandValidator : CustomValidator<UpdateProgramIncrementObjectiveCommand>
{
    private readonly IPlanningDbContext _planningDbContext;
    public UpdateProgramIncrementObjectiveCommandValidator(IPlanningDbContext planningDbContext)
    {
        _planningDbContext = planningDbContext;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(o => o.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(o => o.Description)
            .MaximumLength(1024);

        RuleFor(o => o.Status)
            .IsInEnum()
            .WithMessage("A valid objective status must be selected.");

        RuleFor(o => o.Progress)
            .InclusiveBetween(0.0d, 100.0d)
            .WithMessage("The progress must be between 0 and 100.");

        When(o => o.StartDate.HasValue && o.TargetDate.HasValue, () =>
        {
            RuleFor(o => o.StartDate)
                .LessThan(o => o.TargetDate)
                .WithMessage("The start date must be before the target date.");
        });

        RuleFor(o => o.StartDate)
            .MustAsync(BeWithinProgramIncrementDates)
            .WithMessage("The start date must be within the Program Increment dates.");

        RuleFor(o => o.TargetDate)
            .MustAsync(BeWithinProgramIncrementDates)
            .WithMessage("The target date must be within the Program Increment dates.");
    }

    public async Task<bool> BeWithinProgramIncrementDates(UpdateProgramIncrementObjectiveCommand command, LocalDate? date, CancellationToken cancellationToken)
    {
        if (!date.HasValue)
            return true;

        var programIncrement = await _planningDbContext.ProgramIncrements
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == command.ProgramIncrementId, cancellationToken);

        return programIncrement is null
            ? false
            : programIncrement.DateRange.Start <= date
                && date <= programIncrement.DateRange.End;
    }
}

internal sealed class UpdateProgramIncrementObjectiveCommandHandler : ICommandHandler<UpdateProgramIncrementObjectiveCommand, int>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly ISender _sender;
    private readonly ILogger<UpdateProgramIncrementObjectiveCommandHandler> _logger;

    public UpdateProgramIncrementObjectiveCommandHandler(IPlanningDbContext planningDbContext, ISender sender, ILogger<UpdateProgramIncrementObjectiveCommandHandler> logger)
    {
        _planningDbContext = planningDbContext;
        _sender = sender;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(UpdateProgramIncrementObjectiveCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var programIncrement = await _planningDbContext.ProgramIncrements
                .Include(pi => pi.Objectives.Where(o => o.Id == request.ProgramIncrementObjectiveId))
                .FirstOrDefaultAsync(p => p.Id == request.ProgramIncrementId, cancellationToken);
            if (programIncrement is null)
                return Result.Failure<int>($"Program Increment {request.ProgramIncrementId} not found.");

            var updatePiObjectiveResult = programIncrement.UpdateObjective(request.ProgramIncrementObjectiveId, request.IsStretch);
            if (updatePiObjectiveResult.IsFailure)
            {
                _logger.LogError("Unable to update PI objective.  Error: {Error}", updatePiObjectiveResult.Error);
                return Result.Failure<int>($"Unable to PI create objective.  Error: {updatePiObjectiveResult.Error}");
            }

            await _planningDbContext.SaveChangesAsync(cancellationToken);

            var objectiveName = request.Name;
            if (programIncrement.ObjectivesLocked)
            {
                var currentObjective = await _sender.Send(new GetObjectiveForProgramIncrementQuery(updatePiObjectiveResult.Value.ObjectiveId, programIncrement.Id), cancellationToken);
                if (currentObjective is null)
                    return Result.Failure<int>($"Objective {request.ProgramIncrementObjectiveId} not found.");

                objectiveName = currentObjective.Name;
            }

            var objectiveResult = await _sender.Send(new UpdateObjectiveCommand(
                updatePiObjectiveResult.Value.ObjectiveId,
                objectiveName,
                request.Description,
                request.Status,
                request.Progress,
                updatePiObjectiveResult.Value.TeamId,
                request.StartDate,
                request.TargetDate), cancellationToken);
            if (objectiveResult.IsFailure)
                return Result.Failure<int>($"Unable to update the underlying objective.  Error: {objectiveResult.Error}");
            // TODO: isStretch is still updated in this scenario.

            return Result.Success(updatePiObjectiveResult.Value.Key);
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure<int>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}
