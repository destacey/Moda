namespace Moda.Planning.Application.PlanningIntervals.Commands;
public sealed record CreatePlanningIntervalCommand(string Name, string? Description, LocalDateRange DateRange) : ICommand<int>;

public sealed class CreatePlanningIntervalCommandValidator : CustomValidator<CreatePlanningIntervalCommand>
{
    private readonly IPlanningDbContext _planningDbContext;
    public CreatePlanningIntervalCommandValidator(IPlanningDbContext planningDbContext)
    {
        _planningDbContext = planningDbContext;

        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(e => e.Name)
            .NotEmpty()
            .MaximumLength(128)
            .MustAsync(BeUniquePlanningIntervalName).WithMessage("The Planning Interval name already exists.");

        RuleFor(e => e.Description)
            .MaximumLength(1024);

        RuleFor(e => e.DateRange)
            .NotNull();
    }

    public async Task<bool> BeUniquePlanningIntervalName(string name, CancellationToken cancellationToken)
    {
        return await _planningDbContext.PlanningIntervals.AllAsync(x => x.Name != name, cancellationToken);
    }
}

internal sealed class CreatePlanningIntervalCommandHandler : ICommandHandler<CreatePlanningIntervalCommand, int>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<CreatePlanningIntervalCommandHandler> _logger;

    public CreatePlanningIntervalCommandHandler(IPlanningDbContext planningDbContext, IDateTimeService dateTimeService, ILogger<CreatePlanningIntervalCommandHandler> logger)
    {
        _planningDbContext = planningDbContext;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(CreatePlanningIntervalCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var planningInterval = PlanningInterval.Create(
                request.Name,
                request.Description,
                request.DateRange
                );

            await _planningDbContext.PlanningIntervals.AddAsync(planningInterval, cancellationToken);

            await _planningDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(planningInterval.Key);
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure<int>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}
