using Moda.Goals.Application.Persistence;
using Moda.Goals.Domain.Enums;
using Moda.Goals.Domain.Models;

namespace Moda.Goals.Application.Objectives.Commands;
public sealed record ImportObjectiveCommand(string Name, string? Description, ObjectiveType Type, ObjectiveStatus Status, Guid? OwnerId, Guid? PlanId, LocalDate? StartDate, LocalDate? TargetDate, Instant? ClosedDate) : ICommand<Guid>;

public sealed class ImportObjectiveCommandValidator : CustomValidator<ImportObjectiveCommand>
{
    public ImportObjectiveCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(o => o.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(o => o.Description)
            .MaximumLength(1024);

        RuleFor(o => o.Type)
            .IsInEnum()
            .WithMessage("A valid objective type must be selected.");

        RuleFor(o => o.Status)
            .IsInEnum()
            .WithMessage("A valid objective status must be selected.");

        When(o => o.OwnerId.HasValue, () =>
        {
            RuleFor(o => o.OwnerId)
                .NotEmpty()
                .WithMessage("An owner must be selected.");
        });

        When(o => o.PlanId.HasValue, () =>
        {
            RuleFor(o => o.PlanId)
                .NotEmpty()
                .WithMessage("A plan must be selected.");
        });

        When(o => o.StartDate.HasValue && o.TargetDate.HasValue, () =>
        {
            RuleFor(o => o.StartDate)
                .LessThan(o => o.TargetDate)
                .WithMessage("The start date must be before the target date.");
        });

        When(o => o.Status is ObjectiveStatus.Completed or ObjectiveStatus.Canceled or ObjectiveStatus.Incomplete,
            () => RuleFor(o => o.ClosedDate)
                .NotEmpty()
                    .WithMessage("The ClosedDateUtc can not be empty if the status is Completed, Canceled, or Incomplete."))
            .Otherwise(() => RuleFor(o => o.ClosedDate)
                .Empty()
                    .WithMessage("The ClosedDateUtc must be empty if the status is not Completed, Canceled, or Incomplete."));
    }
}

internal sealed class ImportObjectiveCommandHandler : ICommandHandler<ImportObjectiveCommand, Guid>
{
    private readonly IGoalsDbContext _goalsDbContext;
    private readonly ILogger<ImportObjectiveCommandHandler> _logger;

    public ImportObjectiveCommandHandler(IGoalsDbContext goalsDbContext, ILogger<ImportObjectiveCommandHandler> logger)
    {
        _goalsDbContext = goalsDbContext;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(ImportObjectiveCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var objective = Objective.Import(
                request.Name,
                request.Description,
                request.Type,
                request.Status,
                request.OwnerId,
                request.PlanId,
                request.StartDate,
                request.TargetDate,
                request.ClosedDate
                );

            await _goalsDbContext.Objectives.AddAsync(objective, cancellationToken);

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
