namespace Moda.Work.Application.WorkTypeLevels.Commands;
public sealed record UpdateWorkTypeLevelCommand : ICommand<int>
{
    public UpdateWorkTypeLevelCommand(int id, string name, string? description, int rank)
    {
        Id = id;
        Name = name;
        Description = description;
        Order = rank;
    }

    public int Id { get; }

    /// <summary>The name of the work type.  The name cannot be changed.</summary>
    /// <value>The name.</value>
    public string Name { get; }

    /// <summary>The description of the work type.</summary>
    /// <value>The description.</value>
    public string? Description { get; }

    /// <summary>
    /// The order of the work type level.
    /// </summary>
    /// <value>The order.</value>
    public int Order { get; }
}

public sealed class UpdateWorkTypeLevelCommandValidator : CustomValidator<UpdateWorkTypeLevelCommand>
{
    private readonly IWorkDbContext _workDbContext;

    public UpdateWorkTypeLevelCommandValidator(IWorkDbContext workDbContext)
    {
        _workDbContext = workDbContext;

        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(128)
            .MustAsync(async (cmd, workTypeLevel, cancellationToken) => await BeUniqueName(cmd.Id, workTypeLevel, cancellationToken)).WithMessage("The work type level name already exists."); ;

        RuleFor(c => c.Description)
            .MaximumLength(1024);
    }

    public async Task<bool> BeUniqueName(int id, string name, CancellationToken cancellationToken)
    {
        var levelNames = await _workDbContext.WorkTypeHierarchies
            .SelectMany(s => s.Levels.Where(c => c.Id != id).Select(l => l.Name))
            .ToListAsync(cancellationToken);

        return levelNames.All(l => l != name);
    }
}

internal sealed class UpdateWorkTypeLevelCommandHandler : ICommandHandler<UpdateWorkTypeLevelCommand, int>
{
    private readonly IWorkDbContext _workDbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<UpdateWorkTypeLevelCommandHandler> _logger;

    public UpdateWorkTypeLevelCommandHandler(IWorkDbContext workDbContext, IDateTimeProvider dateTimeProvider, ILogger<UpdateWorkTypeLevelCommandHandler> logger)
    {
        _workDbContext = workDbContext;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(UpdateWorkTypeLevelCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var hierarchy = await _workDbContext.WorkTypeHierarchies
                .Include(s => s.Levels)
                .FirstOrDefaultAsync(cancellationToken);

            if (hierarchy is null)
                return Result.Failure<int>("The system work type hierarchy does not exist.");

            var level = hierarchy.Levels.FirstOrDefault(p => p.Id == request.Id);
            if (level is null)
                return Result.Failure<int>("Work type Level was not found.");

            var updateResult = level.Update(request.Name, request.Description, request.Order, _dateTimeProvider.Now);

            if (updateResult.IsFailure)
            {
                var requestName = request.GetType().Name;
                _logger.LogError("Moda Request: Failure for Request {Name} {@Request}.  Error message: {Error}", requestName, request, updateResult.Error);
                return Result.Failure<int>(updateResult.Error);
            }

            await _workDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(level.Id);
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure<int>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}

