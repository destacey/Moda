namespace Moda.Work.Application.BacklogLevels.Commands;
public sealed record UpdateBacklogLevelCommand : ICommand<int>
{
    public UpdateBacklogLevelCommand(int id, string name, string? description, int rank)
    {
        Id = id;
        Name = name;
        Description = description;
        Rank = rank;
    }

    public int Id { get; }

    /// <summary>The name of the work type.  The name cannot be changed.</summary>
    /// <value>The name.</value>
    public string Name { get; }

    /// <summary>The description of the work type.</summary>
    /// <value>The description.</value>
    public string? Description { get; }

    /// <summary>
    /// The rank of the backlog level. The higher the number, the higher the level.
    /// </summary>
    /// <value>The rank.</value>
    public int Rank { get; }
}

public sealed class UpdateBacklogLevelCommandValidator : CustomValidator<UpdateBacklogLevelCommand>
{
    private readonly IWorkDbContext _workDbContext;

    public UpdateBacklogLevelCommandValidator(IWorkDbContext workDbContext)
    {
        _workDbContext = workDbContext;

        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(256)
            .MustAsync(async (cmd, backlogLevel, cancellationToken) => await BeUniqueName(cmd.Id, backlogLevel, cancellationToken)).WithMessage("The backlog level name already exists."); ;

        RuleFor(c => c.Description)
            .MaximumLength(1024);
    }

    public async Task<bool> BeUniqueName(int id, string name, CancellationToken cancellationToken)
    {
        var backlogLevelNames = await _workDbContext.BacklogLevelSchemes
            .SelectMany(s => s.BacklogLevels.Where(c => c.Id != id).Select(l => l.Name))
            .ToListAsync(cancellationToken);

        return backlogLevelNames.All(l => l != name);
    }
}

internal sealed class UpdateBacklogLevelCommandHandler : ICommandHandler<UpdateBacklogLevelCommand, int>
{
    private readonly IWorkDbContext _workDbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<UpdateBacklogLevelCommandHandler> _logger;

    public UpdateBacklogLevelCommandHandler(IWorkDbContext workDbContext, IDateTimeProvider dateTimeProvider, ILogger<UpdateBacklogLevelCommandHandler> logger)
    {
        _workDbContext = workDbContext;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(UpdateBacklogLevelCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var scheme = await _workDbContext.BacklogLevelSchemes
                .Include(s => s.BacklogLevels)
                .FirstOrDefaultAsync(cancellationToken);

            if (scheme is null)
                return Result.Failure<int>("The system backlog level scheme does not exist.");

            var backlogLevel = scheme.BacklogLevels.FirstOrDefault(p => p.Id == request.Id);
            if (backlogLevel is null)
                return Result.Failure<int>("Backlog Level not found.");

            var updateResult = backlogLevel.Update(request.Name, request.Description, request.Rank, _dateTimeProvider.Now);

            if (updateResult.IsFailure)
            {
                var requestName = request.GetType().Name;
                _logger.LogError("Moda Request: Failure for Request {Name} {@Request}.  Error message: {Error}", requestName, request, updateResult.Error);
                return Result.Failure<int>(updateResult.Error);
            }

            await _workDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(backlogLevel.Id);
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure<int>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}

