using Ardalis.GuardClauses;

namespace Moda.Planning.Application.Roadmaps.Commands;
public sealed record UpdateRoadmapLinksOrderCommand(Guid RoadmapId, Dictionary<Guid,int> RoadmapLinks) : ICommand;

public sealed class UpdateRoadmapLinksOrderCommandValidator : CustomValidator<UpdateRoadmapLinksOrderCommand>
{
    public UpdateRoadmapLinksOrderCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(o => o.RoadmapId)
            .NotEmpty()
            .WithMessage("A roadmap must be selected.");

        RuleFor(o => o.RoadmapLinks)
            .NotEmpty()
            .WithMessage("At least one roadmap link must be provided.");
    }
}

internal sealed class UpdateRoadmapLinksOrderCommandHandler(IPlanningDbContext planningDbContext, ICurrentUser currentUser, ILogger<UpdateRoadmapLinksOrderCommandHandler> logger) : ICommandHandler<UpdateRoadmapLinksOrderCommand>
{
    private const string AppRequestName = nameof(UpdateRoadmapLinksOrderCommand);

    private readonly IPlanningDbContext _planningDbContext = planningDbContext;
    private readonly Guid _currentUserEmployeeId = Guard.Against.NullOrEmpty(currentUser.GetEmployeeId());
    private readonly ILogger<UpdateRoadmapLinksOrderCommandHandler> _logger = logger;

    public async Task<Result> Handle(UpdateRoadmapLinksOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var roadmap = await _planningDbContext.Roadmaps
                .Include(x => x.Managers)
                .Include(x => x.ChildLinks)
                .FirstOrDefaultAsync(r => r.Id == request.RoadmapId, cancellationToken);

            if (roadmap is null)
                return Result.Failure($"Roadmap with id {request.RoadmapId} not found");

            if (roadmap.ChildLinks.Count != request.RoadmapLinks.Count)
            {
                var missingRoadmapLinks = request.RoadmapLinks.Keys.Except(roadmap.ChildLinks.Select(o => o.Id));
                _logger.LogWarning("Not all roadmap links provided were found for roadmap {RoadmapId}. The following roadmap links were not found: {RoadmapLinkIds}", roadmap.Id, missingRoadmapLinks);
                return Result.Failure("Not all roadmap links provided were found.");
            }

            var updateResult = roadmap.SetChildLinksOrder(request.RoadmapLinks, _currentUserEmployeeId);
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


