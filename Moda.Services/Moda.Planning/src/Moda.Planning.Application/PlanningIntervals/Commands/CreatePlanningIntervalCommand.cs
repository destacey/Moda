using Moda.Planning.Domain.Enums;

namespace Moda.Planning.Application.PlanningIntervals.Commands;
public sealed record CreatePlanningIntervalCommand(string Name, string? Description, LocalDateRange DateRange, int IterationWeeks, string? IterationPrefix) : ICommand<int>;

public sealed class CreatePlanningIntervalCommandValidator : CustomValidator<CreatePlanningIntervalCommand>
{
    private readonly IPlanningDbContext _planningDbContext;
    public CreatePlanningIntervalCommandValidator(IPlanningDbContext planningDbContext)
    {
        _planningDbContext = planningDbContext;

        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(128)
            .MustAsync(BeUniquePlanningIntervalName).WithMessage("The Planning Interval name already exists.");

        RuleFor(c => c.Description)
            .MaximumLength(2048);

        RuleFor(c => c.DateRange)
            .NotNull();

        RuleFor(c => c.IterationWeeks)
            .GreaterThan(0);
    }

    public async Task<bool> BeUniquePlanningIntervalName(string name, CancellationToken cancellationToken)
    {
        return await _planningDbContext.PlanningIntervals.AllAsync(x => x.Name != name, cancellationToken);
    }
}

internal sealed class CreatePlanningIntervalCommandHandler : ICommandHandler<CreatePlanningIntervalCommand, int>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<CreatePlanningIntervalCommandHandler> _logger;

    public CreatePlanningIntervalCommandHandler(IPlanningDbContext planningDbContext, IDateTimeProvider dateTimeProvider, ILogger<CreatePlanningIntervalCommandHandler> logger)
    {
        _planningDbContext = planningDbContext;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(CreatePlanningIntervalCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = PlanningInterval.Create(
                request.Name,
                request.Description,
                request.DateRange,
                request.IterationWeeks,
                request.IterationPrefix
                );
            if (result.IsFailure)
                return Result.Failure<int>(result.Error);


            await _planningDbContext.PlanningIntervals.AddAsync(result.Value, cancellationToken);

            await _planningDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(result.Value.Key);
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure<int>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}
