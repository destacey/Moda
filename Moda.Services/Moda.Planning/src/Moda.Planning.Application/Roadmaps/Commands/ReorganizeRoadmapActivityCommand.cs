using Ardalis.GuardClauses;
using Moda.Planning.Domain.Models.Roadmaps;

namespace Moda.Planning.Application.Roadmaps.Commands;

public sealed record ReorganizeRoadmapActivityCommand(Guid RoadmapId, Guid? ParentActivityId, Guid ActivityId, int Order) : ICommand;

public sealed class ReorganizeRoadmapActivityCommandValidator : CustomValidator<ReorganizeRoadmapActivityCommand>
{
    private readonly IPlanningDbContext _planningDbContext;

    public ReorganizeRoadmapActivityCommandValidator(IPlanningDbContext planningDbContext)
    {
        _planningDbContext = planningDbContext;

        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(o => o.RoadmapId)
            .NotEmpty()
            .WithMessage("A valid roadmap id must be provided.");

        When(o => o.ParentActivityId.HasValue, () =>
        {
            RuleFor(o => o.ParentActivityId)
                .NotEmpty()
                .WithMessage("A valid parent activity id must be provided.");
        });

        RuleFor(o => o.ActivityId)
            .NotEmpty()
                .WithMessage("A valid activity id must be provided.")
            .MustAsync(BeActivityType)
                .WithMessage("The activity id provided is not a valid activity.  Only roadmap activities can be reorganized.");

        RuleFor(o => o.Order)
            .GreaterThan(0)
            .WithMessage("Order must be greater than 0.");
    }

    public async Task<bool> BeActivityType(ReorganizeRoadmapActivityCommand command, Guid activityId, CancellationToken cancellationToken)
    {
        var roadmap = await _planningDbContext.Roadmaps
            .Include(x => x.Items)
            .SingleOrDefaultAsync(r => r.Id == command.RoadmapId);

        if (roadmap is null)
            return false;

        var activity = roadmap.Items.OfType<RoadmapActivity>().FirstOrDefault(x => x.Id == activityId);

        return activity is not null;
    }
}

internal sealed class ReorganizeRoadmapActivityCommandHandler(IPlanningDbContext planningDbContext, ICurrentUser currentUser, ILogger<ReorganizeRoadmapActivityCommandHandler> logger) : ICommandHandler<ReorganizeRoadmapActivityCommand>
{
    private const string AppRequestName = nameof(ReorganizeRoadmapActivityCommand);

    private readonly IPlanningDbContext _planningDbContext = planningDbContext;
    private readonly Guid _currentUserEmployeeId = Guard.Against.NullOrEmpty(currentUser.GetEmployeeId());
    private readonly ILogger<ReorganizeRoadmapActivityCommandHandler> _logger = logger;

    public async Task<Result> Handle(ReorganizeRoadmapActivityCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var roadmap = await _planningDbContext.Roadmaps
                .Include(x => x.RoadmapManagers)
                .Include(x => x.Items)
                .FirstOrDefaultAsync(r => r.Id == request.RoadmapId, cancellationToken);

            if (roadmap is null)
            {
                _logger.LogDebug("Roadmap with id {RoadmapId} not found", request.RoadmapId);
                return Result.Failure($"Roadmap with id {request.RoadmapId} not found");
            }

            var activity = roadmap.Items.OfType<RoadmapActivity>().FirstOrDefault(x => x.Id == request.ActivityId);
            if (activity is null)
            {
                _logger.LogDebug("Roadmap Activity with id {RoadmapActivityId} not found", request.RoadmapId);
                return Result.Failure($"Roadmap activity with id {request.ActivityId} not found");
            }

            Result reorganizeResult = Result.Success();
            if (activity.ParentId == request.ParentActivityId)
            {
                reorganizeResult = roadmap.SetActivityOrder(activity.Id, request.Order, _currentUserEmployeeId);
                _logger.LogInformation("Reorganized roadmap activity {RoadmapActivityId} to order {Order}.", request.ActivityId, request.Order);
            }
            else
            {
                reorganizeResult = roadmap.MoveActivity(activity.Id, request.ParentActivityId, request.Order, _currentUserEmployeeId);
                _logger.LogInformation("Moved roadmap activity {RoadmapActivityId} to parent {ParentActivityId} and order {Order}.", request.ActivityId, request.ParentActivityId, request.Order);
            }

            if (reorganizeResult.IsFailure)
            {
                _logger.LogError("Unable to reorganize roadmap activity {RoadmapActivityId} for request {@Request}.  Error message: {Error}", request.ActivityId, request, reorganizeResult.Error);
                return Result.Failure(reorganizeResult.Error);
            }

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
