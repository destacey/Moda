namespace Moda.Planning.Application.ProgramIncrements.Commands;
public sealed record CreateProgramIncrementCommand(string Name, string? Description, LocalDateRange DateRange) : ICommand<int>;

public sealed class CreateProgramIncrementCommandValidator : CustomValidator<CreateProgramIncrementCommand>
{
    public CreateProgramIncrementCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(e => e.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(e => e.Description)
            .MaximumLength(1024);

        RuleFor(e => e.DateRange)
            .NotNull();
    }
}

internal sealed class CreateProgramIncrementCommandHandler : ICommandHandler<CreateProgramIncrementCommand, int>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<CreateProgramIncrementCommandHandler> _logger;

    public CreateProgramIncrementCommandHandler(IPlanningDbContext planningDbContext, IDateTimeService dateTimeService, ILogger<CreateProgramIncrementCommandHandler> logger)
    {
        _planningDbContext = planningDbContext;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(CreateProgramIncrementCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var programIncrement = ProgramIncrement.Create(
                request.Name,
                request.Description,
                request.DateRange
                );

            await _planningDbContext.ProgramIncrements.AddAsync(programIncrement, cancellationToken);

            await _planningDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(programIncrement.LocalId);
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure<int>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}
