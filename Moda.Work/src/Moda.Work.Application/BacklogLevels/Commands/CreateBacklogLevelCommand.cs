using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moda.Common.Domain.Enums;
using NodaTime;

namespace Moda.Work.Application.BacklogLevels.Commands;
public sealed record CreateBacklogLevelCommand : ICommand<int>
{
    public CreateBacklogLevelCommand(string name, string? description, int rank)
    {
        Name = name;
        Description = description;
        Rank = rank;
    }

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

public sealed class CreateBacklogLevelCommandValidator : CustomValidator<CreateBacklogLevelCommand>
{
    private readonly IWorkDbContext _workDbContext;

    public CreateBacklogLevelCommandValidator(IWorkDbContext workDbContext)
    {
        _workDbContext = workDbContext;

        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(256)
            .MustAsync(BeUniqueName).WithMessage("The backlog level name already exists.");

        RuleFor(c => c.Description)
            .MaximumLength(1024);
    }

    public async Task<bool> BeUniqueName(string name, CancellationToken cancellationToken)
    {
        var backlogLevelNames = await _workDbContext.BacklogLevelSchemes
            .SelectMany(s => s.BacklogLevels.Select(l => l.Name))
            .ToListAsync(cancellationToken);
        return backlogLevelNames.All(l => l != name);
    }
}

internal sealed class CreateBacklogLevelCommandHandler : ICommandHandler<CreateBacklogLevelCommand, int>
{
    private readonly IWorkDbContext _workDbContext;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<CreateBacklogLevelCommandHandler> _logger;

    public CreateBacklogLevelCommandHandler(IWorkDbContext workDbContext, IDateTimeService dateTimeService, ILogger<CreateBacklogLevelCommandHandler> logger)
    {
        _workDbContext = workDbContext;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(CreateBacklogLevelCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var scheme = await _workDbContext.BacklogLevelSchemes
                .Include(s => s.BacklogLevels)
                .FirstOrDefaultAsync(cancellationToken);

            if (scheme is null)
                return Result.Failure<int>("The system backlog level scheme does not exist.");

            Instant timestamp = _dateTimeService.Now;

            var backlogLevel = BacklogLevel.Create(request.Name, request.Description, BacklogCategory.Portfolio, Ownership.Owned, request.Rank, timestamp);

            var addResult = scheme.AddPortfolioBacklogLevel(backlogLevel, timestamp);
            if (addResult.IsFailure)
                return Result.Failure<int>(addResult.Error);

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

