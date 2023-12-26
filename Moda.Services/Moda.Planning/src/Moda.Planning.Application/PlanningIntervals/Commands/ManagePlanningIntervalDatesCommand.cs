using Ardalis.GuardClauses;

namespace Moda.Planning.Application.PlanningIntervals.Commands;
public sealed record ManagePlanningIntervalDatesCommand : ICommand
{
    public ManagePlanningIntervalDatesCommand(Guid id, LocalDateRange dateRange)
    {
        Id = id;
        DateRange = Guard.Against.Null(dateRange);
    }

    public Guid Id { get; }
    public LocalDateRange DateRange { get; }
}

internal sealed class ManagePlanningIntervalDatesCommandHandler : ICommandHandler<ManagePlanningIntervalDatesCommand>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly ILogger<ManagePlanningIntervalDatesCommandHandler> _logger;

    public ManagePlanningIntervalDatesCommandHandler(IPlanningDbContext planningDbContext, ILogger<ManagePlanningIntervalDatesCommandHandler> logger)
    {
        _planningDbContext = planningDbContext;
        _logger = logger;
    }

    public async Task<Result> Handle(ManagePlanningIntervalDatesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var planningInterval = await _planningDbContext.PlanningIntervals
                .Include(x => x.Iterations)
                .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (planningInterval is null)
            {
                _logger.LogWarning("Planning Interval with Id {PlanningIntervalId} not found.", request.Id);
                return Result.Failure($"Planning Interval with Id {request.Id} not found.");
            }

            var result = planningInterval.ManageDates(request.DateRange);
            if (result.IsFailure)
                return Result.Failure(result.Error);

            await _planningDbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling {CommandName} command.", nameof(ManagePlanningIntervalTeamsCommand));
            return Result.Failure($"Error handling {nameof(ManagePlanningIntervalTeamsCommand)} command.");
        }
    }
}
