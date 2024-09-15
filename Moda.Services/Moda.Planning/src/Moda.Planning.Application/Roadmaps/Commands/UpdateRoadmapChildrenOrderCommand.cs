using Ardalis.GuardClauses;

namespace Moda.Planning.Application.Roadmaps.Commands;
public sealed record UpdateRoadmapChildrenOrderCommand(Guid RoadmapId, Dictionary<Guid,int> ChildrenOrder) : ICommand;

public sealed class UpdateRoadmapChildrenOrderCommandValidator : CustomValidator<UpdateRoadmapChildrenOrderCommand>
{
    public UpdateRoadmapChildrenOrderCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(o => o.RoadmapId)
            .NotEmpty()
            .WithMessage("A roadmap must be selected.");

        RuleFor(o => o.ChildrenOrder)
            .NotEmpty()
            .WithMessage("At least one child roadmap must be provided.");
    }
}

internal sealed class UpdateRoadmapChildrenOrderCommandHandler(IPlanningDbContext planningDbContext, ICurrentUser currentUser, ILogger<UpdateRoadmapChildrenOrderCommandHandler> logger) : ICommandHandler<UpdateRoadmapChildrenOrderCommand>
{
    private const string AppRequestName = nameof(UpdateRoadmapChildrenOrderCommand);

    private readonly IPlanningDbContext _planningDbContext = planningDbContext;
    private readonly Guid _currentUserEmployeeId = Guard.Against.NullOrEmpty(currentUser.GetEmployeeId());
    private readonly ILogger<UpdateRoadmapChildrenOrderCommandHandler> _logger = logger;

    public async Task<Result> Handle(UpdateRoadmapChildrenOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var roadmap = await _planningDbContext.Roadmaps
                .Include(x => x.RoadmapManagers)
                .Include(x => x.Children)
                .FirstOrDefaultAsync(r => r.Id == request.RoadmapId, cancellationToken);

            if (roadmap is null)
                return Result.Failure($"Roadmap with id {request.RoadmapId} not found");

            if (roadmap.Children.Count != request.ChildrenOrder.Count)
            {
                var missingRoadmapLinks = request.ChildrenOrder.Keys.Except(roadmap.Children.Select(o => o.Id));
                _logger.LogWarning("Not all roadmap links provided were found for roadmap {RoadmapId}. The following roadmap links were not found: {RoadmapLinkIds}", roadmap.Id, missingRoadmapLinks);
                return Result.Failure("Not all roadmap links provided were found.");
            }

            var updateResult = roadmap.SetChildrenOrder(request.ChildrenOrder, _currentUserEmployeeId);
            if (updateResult.IsFailure)
            {
                // Reset the entity
                await _planningDbContext.Entry(roadmap).ReloadAsync(cancellationToken);
                roadmap.ClearDomainEvents();

                _logger.LogError("Failure for Request {CommandName} {@Request}.  Error message: {Error}", AppRequestName, request, updateResult.Error);
                return Result.Failure(updateResult.Error);
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


