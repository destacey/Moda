using Moda.Goals.Application.Persistence;
using Moda.Goals.Domain.Enums;

namespace Moda.Goals.Application.Objectives.Commands;
public sealed record UpdateObjectiveCommand(Guid Id, string Name, string? Description, ObjectiveStatus Status, double Progress, Guid? OwnerId, LocalDate? StartDate, LocalDate? TargetDate) : ICommand<Guid>;

public sealed class UpdateObjectiveCommandValidator : CustomValidator<UpdateObjectiveCommand>
{
    public UpdateObjectiveCommandValidator()
    {
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

        When(o => o.OwnerId.HasValue, () =>
        {
            RuleFor(o => o.OwnerId)
                .NotEmpty()
                .WithMessage("An owner must be selected.");
        });

        When(o => o.StartDate.HasValue && o.TargetDate.HasValue, () =>
        {
            RuleFor(o => o.StartDate)
                .LessThan(o => o.TargetDate)
                .WithMessage("The start date must be before the target date.");
        });
    }
}

internal sealed class UpdateObjectiveCommandHandler : ICommandHandler<UpdateObjectiveCommand, Guid>
{
    private readonly IGoalsDbContext _goalsDbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<UpdateObjectiveCommandHandler> _logger;

    public UpdateObjectiveCommandHandler(IGoalsDbContext goalsDbContext, IDateTimeProvider dateTimeProvider, ILogger<UpdateObjectiveCommandHandler> logger)
    {
        _goalsDbContext = goalsDbContext;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(UpdateObjectiveCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var objective = await _goalsDbContext.Objectives
                .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
            if (objective is null)
                return Result.Failure<Guid>("Objective not found.");

            var updateResult = objective.Update(
                request.Name,
                request.Description,
                request.Status,
                request.Progress,
                request.OwnerId,
                request.StartDate,
                request.TargetDate,
                _dateTimeProvider.Now
                );

            if (updateResult.IsFailure)
            {
                // Reset the entity
                await _goalsDbContext.Entry(objective).ReloadAsync(cancellationToken);
                objective.ClearDomainEvents();

                var requestName = request.GetType().Name;
                _logger.LogError("Moda Request: Failure for Request {Name} {@Request}.  Error message: {Error}", requestName, request, updateResult.Error);
                return Result.Failure<Guid>(updateResult.Error);
            }

            await _goalsDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(objective.Id);
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure<Guid>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}
