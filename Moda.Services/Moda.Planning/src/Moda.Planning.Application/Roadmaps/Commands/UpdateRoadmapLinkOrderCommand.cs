using Ardalis.GuardClauses;

namespace Moda.Planning.Application.Roadmaps.Commands;
public sealed record UpdateRoadmapLinkOrderCommand(Guid RoadmapId, Guid RoadmapLinkId, int Order) : ICommand;

public sealed class UpdateRoadmapLinkOrderCommandValidator : CustomValidator<UpdateRoadmapLinkOrderCommand>
{
    public UpdateRoadmapLinkOrderCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(o => o.RoadmapId)
            .NotEmpty()
            .WithMessage("A valid roadmap must be provided.");

        RuleFor(o => o.RoadmapLinkId)
            .NotEmpty()
            .WithMessage("A valid roadmap link must be provided.");

        RuleFor(o => o.Order)
            .GreaterThan(0)
            .WithMessage("Order must be greater than 0.");
    }
}

internal sealed class UpdateRoadmapLinkOrderCommandHandler(IPlanningDbContext planningDbContext, ICurrentUser currentUser, ILogger<UpdateRoadmapLinkOrderCommandHandler> logger) : ICommandHandler<UpdateRoadmapLinkOrderCommand>
{
    private const string AppRequestName = nameof(UpdateRoadmapLinkOrderCommand);

    private readonly IPlanningDbContext _planningDbContext = planningDbContext;
    private readonly Guid _currentUserEmployeeId = Guard.Against.NullOrEmpty(currentUser.GetEmployeeId());
    private readonly ILogger<UpdateRoadmapLinkOrderCommandHandler> _logger = logger;

    public async Task<Result> Handle(UpdateRoadmapLinkOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var roadmap = await _planningDbContext.Roadmaps
                .Include(x => x.Managers)
                .Include(x => x.ChildLinks)
                .FirstOrDefaultAsync(r => r.Id == request.RoadmapId, cancellationToken);

            if (roadmap is null)
                return Result.Failure($"Roadmap with id {request.RoadmapId} not found");

            var roadmapLink = roadmap.ChildLinks.FirstOrDefault(x => x.Id == request.RoadmapLinkId);
            if (roadmapLink is null)
                return Result.Failure($"Roadmap link with id {request.RoadmapLinkId} not found");

            var updateResult = roadmap.SetChildLinksOrder(request.RoadmapLinkId, request.Order, _currentUserEmployeeId);
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


