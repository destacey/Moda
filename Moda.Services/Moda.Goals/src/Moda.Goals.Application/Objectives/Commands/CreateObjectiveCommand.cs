using Moda.Goals.Application.Persistence;
using Moda.Goals.Domain.Enums;
using Moda.Goals.Domain.Models;

namespace Moda.Goals.Application.Objectives.Commands;
public sealed record CreateObjectiveCommand(string Name, string? Description, ObjectiveType Type, Guid? OwnerId, Guid? PlanId, LocalDate? StartDate, LocalDate? TargetDate) : ICommand<Guid>;

public sealed class CreateObjectiveCommandValidator : CustomValidator<CreateObjectiveCommand>
{
    public CreateObjectiveCommandValidator()
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
    }
}

internal sealed class CreateObjectiveCommandHandler : ICommandHandler<CreateObjectiveCommand, Guid>
{
    private readonly IGoalsDbContext _goalsDbContext;
    private readonly ILogger<CreateObjectiveCommandHandler> _logger;

    public CreateObjectiveCommandHandler(IGoalsDbContext goalsDbContext, ILogger<CreateObjectiveCommandHandler> logger)
    {
        _goalsDbContext = goalsDbContext;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateObjectiveCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var objective = Objective.Create(
                request.Name,
                request.Description,
                request.Type,
                request.OwnerId,
                request.PlanId,
                request.StartDate,
                request.TargetDate
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
