namespace Moda.Planning.Application.PlanningIntervals.Commands;

public sealed record ManagePlanningIntervalTeamsCommand(Guid Id, IEnumerable<Guid> TeamIds) : ICommand;

internal sealed class ManagePlanningIntervalTeamsCommandHandler : ICommandHandler<ManagePlanningIntervalTeamsCommand>
{
    private const string AppRequestName = nameof(ManagePlanningIntervalTeamsCommand);

    private readonly IPlanningDbContext _planningDbContext;
    private readonly ILogger<ManagePlanningIntervalTeamsCommandHandler> _logger;

    public ManagePlanningIntervalTeamsCommandHandler(IPlanningDbContext planningDbContext, ILogger<ManagePlanningIntervalTeamsCommandHandler> logger)
    {
        _planningDbContext = planningDbContext;
        _logger = logger;
    }

    public async Task<Result> Handle(ManagePlanningIntervalTeamsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var planningInterval = await _planningDbContext.PlanningIntervals
                .Include(x => x.Teams)
                .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (planningInterval == null)
            {
                _logger.LogWarning("Planning Interval with Id {PlanningIntervalId} not found.", request.Id);
                return Result.Failure($"Planning Interval with Id {request.Id} not found.");
            }

            // TODO validate teams exist, currently they are in a different bounded context

            var result = planningInterval.ManageTeams(request.TeamIds);
            if (result.IsFailure)
                return Result.Failure(result.Error);

            await _planningDbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception handling {CommandName} command for request {@Request}.", AppRequestName, request);
            return Result.Failure($"Error handling {AppRequestName} command.");
        }
    }
}