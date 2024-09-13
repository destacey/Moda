using Ardalis.GuardClauses;

namespace Moda.Planning.Application.Roadmaps.Commands;
public sealed record DeleteRoadmapCommand (Guid Id) : ICommand;
public sealed class DeleteRoadmapCommandValidator : AbstractValidator<DeleteRoadmapCommand>
{
    public DeleteRoadmapCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}

internal sealed class DeleteRoadmapCommandHandler(IPlanningDbContext planningDbContext, ICurrentUser currentUser, ILogger<DeleteRoadmapCommandHandler> logger) : ICommandHandler<DeleteRoadmapCommand>
{
    private readonly IPlanningDbContext _planningDbContext = planningDbContext;
    private readonly Guid _currentUserEmployeeId = Guard.Against.NullOrEmpty(currentUser.GetEmployeeId());
    private readonly ILogger<DeleteRoadmapCommandHandler> _logger = logger;

    public async Task<Result> Handle(DeleteRoadmapCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var roadmap = await _planningDbContext.Roadmaps
                .Include(x => x.Managers)
                .Include(x => x.Children)
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (roadmap is null)
                return Result.Failure($"Roadmap with id {request.Id} not found");

            var deleteResult = roadmap.CanDelete(_currentUserEmployeeId);
            if (deleteResult.IsFailure)
            {
                _logger.LogError("Moda Request: Failure for Request {Name} {@Request}.  Error message: {Error}", request.GetType().Name, request, deleteResult.Error);
                return Result.Failure(deleteResult.Error);
            }

            var childrenToRemove = roadmap.Children.ToList();
            foreach (var child in childrenToRemove)
            {
                var removeChildResult = roadmap.RemoveChild(child.Id, _currentUserEmployeeId);
                if (removeChildResult.IsFailure)
                {
                    // Reset the entity
                    await _planningDbContext.Entry(roadmap).ReloadAsync(cancellationToken);
                    roadmap.ClearDomainEvents();

                    _logger.LogError("Unable to remove child {ChildRoadmapId} from roadmap {RoadmapId}. Error message: {Error}", child.Id, roadmap.Id, removeChildResult.Error);
                    return Result.Failure(removeChildResult.Error);
                }
            }

            _planningDbContext.Roadmaps.Remove(roadmap);
            await _planningDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}

