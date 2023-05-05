namespace Moda.Planning.Application.ProgramIncrements.Commands;
public sealed record UpdateProgramIncrementCommand(Guid Id, string Name, string? Description, LocalDateRange DateRange) : ICommand<int>;

public sealed class UpdateProgramIncrementCommandValidator : CustomValidator<UpdateProgramIncrementCommand>
{
    public UpdateProgramIncrementCommandValidator()
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

internal sealed class UpdateProgramIncrementCommandHandler : ICommandHandler<UpdateProgramIncrementCommand, int>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<UpdateProgramIncrementCommandHandler> _logger;

    public UpdateProgramIncrementCommandHandler(IPlanningDbContext planningDbContext, IDateTimeService dateTimeService, ILogger<UpdateProgramIncrementCommandHandler> logger)
    {
        _planningDbContext = planningDbContext;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(UpdateProgramIncrementCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var programIncrement = await _planningDbContext.ProgramIncrements
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
            if (programIncrement is null)
                return Result.Failure<int>("Program Increment not found.");

            var updateResult = programIncrement.Update(
                request.Name,
                request.Description,
                request.DateRange
                );

            if (updateResult.IsFailure)
            {
                // Reset the entity
                await _planningDbContext.Entry(programIncrement).ReloadAsync(cancellationToken);
                programIncrement.ClearDomainEvents();

                var requestName = request.GetType().Name;
                _logger.LogError("Moda Request: Failure for Request {Name} {@Request}.  Error message: {Error}", requestName, request, updateResult.Error);
                return Result.Failure<int>(updateResult.Error);
            }

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

