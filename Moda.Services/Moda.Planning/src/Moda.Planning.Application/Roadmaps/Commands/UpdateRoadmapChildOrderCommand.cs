using Ardalis.GuardClauses;

namespace Moda.Planning.Application.Roadmaps.Commands;
public sealed record UpdateRoadmapChildOrderCommand(Guid RoadmapId, Guid ChildRoadmapId, int Order) : ICommand;

public sealed class UpdateRoadmapChildOrderCommandValidator : CustomValidator<UpdateRoadmapChildOrderCommand>
{
    public UpdateRoadmapChildOrderCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(o => o.RoadmapId)
            .NotEmpty()
            .WithMessage("A valid roadmap id must be provided.");

        RuleFor(o => o.ChildRoadmapId)
            .NotEmpty()
            .WithMessage("A valid child roadmap id must be provided.");

        RuleFor(o => o.Order)
            .GreaterThan(0)
            .WithMessage("Order must be greater than 0.");
    }
}

internal sealed class UpdateRoadmapChildOrderCommandHandler(IPlanningDbContext planningDbContext, ICurrentUser currentUser, ILogger<UpdateRoadmapChildOrderCommandHandler> logger) : ICommandHandler<UpdateRoadmapChildOrderCommand>
{
    private const string AppRequestName = nameof(UpdateRoadmapChildOrderCommand);

    private readonly IPlanningDbContext _planningDbContext = planningDbContext;
    private readonly Guid _currentUserEmployeeId = Guard.Against.NullOrEmpty(currentUser.GetEmployeeId());
    private readonly ILogger<UpdateRoadmapChildOrderCommandHandler> _logger = logger;

    public async Task<Result> Handle(UpdateRoadmapChildOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var roadmap = await _planningDbContext.Roadmaps
                .Include(x => x.Managers)
                .Include(x => x.Children)
                .FirstOrDefaultAsync(r => r.Id == request.RoadmapId, cancellationToken);

            if (roadmap is null)
                return Result.Failure($"Roadmap with id {request.RoadmapId} not found");

            var updateResult = roadmap.SetChildrenOrder(request.ChildRoadmapId, request.Order, _currentUserEmployeeId);
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


