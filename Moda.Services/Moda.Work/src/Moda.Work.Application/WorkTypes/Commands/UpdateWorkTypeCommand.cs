namespace Moda.Work.Application.WorkTypes.Commands;
public sealed record UpdateWorkTypeCommand : ICommand<int>
{
    public UpdateWorkTypeCommand(int id, string? description)
    {
        Id = id;
        Description = description;
    }

    public int Id { get; }

    /// <summary>The description of the work type.</summary>
    /// <value>The description.</value>
    public string? Description { get; }
}

public sealed class UpdateWorkTypeCommandValidator : CustomValidator<UpdateWorkTypeCommand>
{
    public UpdateWorkTypeCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Description)
            .MaximumLength(1024);
    }
}

internal sealed class UpdateWorkTypeCommandHandler : ICommandHandler<UpdateWorkTypeCommand, int>
{
    private readonly IWorkDbContext _workDbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<UpdateWorkTypeCommandHandler> _logger;

    public UpdateWorkTypeCommandHandler(IWorkDbContext workDbContext, IDateTimeProvider dateTimeProvider, ILogger<UpdateWorkTypeCommandHandler> logger)
    {
        _workDbContext = workDbContext;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(UpdateWorkTypeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var workType = await _workDbContext.WorkTypes
                .FirstAsync(p => p.Id == request.Id, cancellationToken);
            if (workType is null)
                return Result.Failure<int>("Work Type not found.");

            var updateResult = workType.Update(request.Description, _dateTimeProvider.Now);

            if (updateResult.IsFailure)
            {
                // Reset the entity
                await _workDbContext.Entry(workType).ReloadAsync(cancellationToken);
                workType.ClearDomainEvents();

                var requestName = request.GetType().Name;
                _logger.LogError("Moda Request: Failure for Request {Name} {@Request}.  Error message: {Error}", requestName, request, updateResult.Error);
                return Result.Failure<int>(updateResult.Error);
            }

            await _workDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(workType.Id);
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure<int>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}

