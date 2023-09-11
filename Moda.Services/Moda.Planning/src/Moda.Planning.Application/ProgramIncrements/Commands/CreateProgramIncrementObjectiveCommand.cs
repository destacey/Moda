using MediatR;
using Moda.Goals.Application.Objectives.Commands;

namespace Moda.Planning.Application.ProgramIncrements.Commands;
public sealed record CreateProgramIncrementObjectiveCommand(Guid ProgramIncrementId, Guid TeamId, string Name, string? Description, LocalDate? StartDate, LocalDate? TargetDate, bool IsStretch) : ICommand<int>;

public sealed class CreateProgramIncrementObjectiveCommandValidator : CustomValidator<CreateProgramIncrementObjectiveCommand>
{
    private readonly IPlanningDbContext _planningDbContext;

    public CreateProgramIncrementObjectiveCommandValidator(IPlanningDbContext planningDbContext)
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
            .MustAsync(BeWithinProgramIncrementDates)
            .WithMessage("The start date must be within the Program Increment dates.");

        RuleFor(o => o.TargetDate)
            .MustAsync(BeWithinProgramIncrementDates)
            .WithMessage("The target date must be within the Program Increment dates.");
    }

    public async Task<bool> BeWithinProgramIncrementDates(CreateProgramIncrementObjectiveCommand command, LocalDate? date, CancellationToken cancellationToken)
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

internal sealed class CreateProgramIncrementObjectiveCommandHandler : ICommandHandler<CreateProgramIncrementObjectiveCommand, int>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly ISender _sender;
    private readonly ILogger<CreateProgramIncrementObjectiveCommandHandler> _logger;

    public CreateProgramIncrementObjectiveCommandHandler(IPlanningDbContext planningDbContext, ISender sender, ILogger<CreateProgramIncrementObjectiveCommandHandler> logger)
    {
        _planningDbContext = planningDbContext;
        _sender = sender;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(CreateProgramIncrementObjectiveCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var programIncrement = await _planningDbContext.ProgramIncrements
                .FirstOrDefaultAsync(p => p.Id == request.ProgramIncrementId, cancellationToken);
            if (programIncrement is null)
            {
                _logger.LogError("Program Increment {ProgramIncrementId} not found.", request.ProgramIncrementId);
                return Result.Failure<int>("Program Increment not found.");
            }

            if (programIncrement.ObjectivesLocked)
            {
                _logger.LogError("Objectives are locked for the Program Increment {ProgramIncrementId}", request.ProgramIncrementId);
                return Result.Failure<int>("Objectives are locked for the Program Increment");
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
                Goals.Domain.Enums.ObjectiveType.ProgramIncrement,
                request.TeamId,
                request.ProgramIncrementId,
                request.StartDate, 
                request.TargetDate), cancellationToken);
            if (objectiveResult.IsFailure)
            {
                _logger.LogError("Unable to create objective.  Error: {Error}", objectiveResult.Error);
                return Result.Failure<int>($"Unable to create objective.  Error: {objectiveResult.Error}");
            }

            var result = programIncrement.CreateObjective(team, objectiveResult.Value, request.IsStretch);
            if (result.IsFailure)
            {
                var deleteResult = await _sender.Send(new DeleteObjectiveCommand(objectiveResult.Value), cancellationToken);
                if (deleteResult.IsFailure)
                    _logger.LogError("Unable to delete objective.  Error: {Error}", deleteResult.Error);

                _logger.LogError("Unable to create PI objective.  Error: {Error}", result.Error);
                return Result.Failure<int>($"Unable to PI create objective.  Error: {result.Error}");
            }

            await _planningDbContext.SaveChangesAsync(cancellationToken);

            var key = programIncrement.Objectives
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
