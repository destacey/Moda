﻿namespace Moda.Planning.Application.ProgramIncrements.Commands;
public sealed record UpdateProgramIncrementCommand(Guid Id, string Name, string? Description, LocalDateRange DateRange, bool objectivesLocked) : ICommand<int>;

public sealed class UpdateProgramIncrementCommandValidator : CustomValidator<UpdateProgramIncrementCommand>
{
    private readonly IPlanningDbContext _planningDbContext;
    public UpdateProgramIncrementCommandValidator(IPlanningDbContext planningDbContext)
    {
        _planningDbContext = planningDbContext;

        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(e => e.Name)
            .NotEmpty()
            .MaximumLength(128)
            .MustAsync(async (model, name, cancellationToken) 
                => await BeUniqueProgramIncrementName(model.Id, name, cancellationToken))
                .WithMessage("The Program Increment name already exists."); ;

        RuleFor(e => e.Description)
            .MaximumLength(1024);

        RuleFor(e => e.DateRange)
            .NotNull();
    }

    public async Task<bool> BeUniqueProgramIncrementName(Guid id, string name, CancellationToken cancellationToken)
    {
        return await _planningDbContext.ProgramIncrements.Where(t => t.Id != id).AllAsync(x => x.Name != name, cancellationToken);
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
                request.DateRange,
                request.objectivesLocked
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

            return Result.Success(programIncrement.Key);
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure<int>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}
