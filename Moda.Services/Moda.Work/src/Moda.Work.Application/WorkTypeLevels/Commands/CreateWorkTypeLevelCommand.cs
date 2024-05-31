using Moda.Common.Domain.Enums;
using Moda.Common.Domain.Enums.Work;

namespace Moda.Work.Application.WorkTypeLevels.Commands;
public sealed record CreateWorkTypeLevelCommand : ICommand<int>
{
    public CreateWorkTypeLevelCommand(string name, string? description, int order)
    {
        Name = name;
        Description = description;
        Order = order;
    }

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

public sealed class CreateWorkTypeLevelCommandValidator : CustomValidator<CreateWorkTypeLevelCommand>
{
    private readonly IWorkDbContext _workDbContext;

    public CreateWorkTypeLevelCommandValidator(IWorkDbContext workDbContext)
    {
        _workDbContext = workDbContext;

        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(128)
            .MustAsync(BeUniqueName).WithMessage("The work type level name already exists.");

        RuleFor(c => c.Description)
            .MaximumLength(1024);
    }

    public async Task<bool> BeUniqueName(string name, CancellationToken cancellationToken)
    {
        var levelNames = await _workDbContext.WorkTypeHierarchies
            .SelectMany(s => s.Levels.Select(l => l.Name))
            .ToListAsync(cancellationToken);
        return levelNames.All(l => l != name);
    }
}

internal sealed class CreateWorkTypeLevelCommandHandler : ICommandHandler<CreateWorkTypeLevelCommand, int>
{
    private readonly IWorkDbContext _workDbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<CreateWorkTypeLevelCommandHandler> _logger;

    public CreateWorkTypeLevelCommandHandler(IWorkDbContext workDbContext, IDateTimeProvider dateTimeProvider, ILogger<CreateWorkTypeLevelCommandHandler> logger)
    {
        _workDbContext = workDbContext;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(CreateWorkTypeLevelCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var hierarchy = await _workDbContext.WorkTypeHierarchies
                .Include(s => s.Levels)
                .FirstOrDefaultAsync(cancellationToken);

            if (hierarchy is null)
                return Result.Failure<int>("The system work type hierarchy does not exist.");

            Instant timestamp = _dateTimeProvider.Now;

            var level = WorkTypeLevel.Create(request.Name, request.Description, WorkTypeTier.Portfolio, Ownership.Owned, request.Order, timestamp);

            var addResult = hierarchy.AddPortfolioWorkTypeLevel(level, timestamp);
            if (addResult.IsFailure)
                return Result.Failure<int>(addResult.Error);

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

