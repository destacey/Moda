using Ardalis.GuardClauses;

namespace Moda.Planning.Application.Roadmaps.Commands;
public sealed record CreateRoadmapLinkCommand(Guid ParentId, Guid ChildId) : ICommand;
public sealed class CreateRoadmapLinkCommandValidator : AbstractValidator<CreateRoadmapLinkCommand>
{
    public CreateRoadmapLinkCommandValidator()
    {
        RuleFor(x => x.ParentId)
            .NotEmpty();
        RuleFor(x => x.ChildId)
            .NotEmpty();
    }
}

internal sealed class CreateRoadmapLinkCommandHandler(IPlanningDbContext planningDbContext, ICurrentUser currentUser, ILogger<CreateRoadmapLinkCommandHandler> logger) : ICommandHandler<CreateRoadmapLinkCommand>
{
    private readonly IPlanningDbContext _planningDbContext = planningDbContext;
    private readonly Guid _currentUserEmployeeId = Guard.Against.NullOrEmpty(currentUser.GetEmployeeId());
    private readonly ILogger<CreateRoadmapLinkCommandHandler> _logger = logger;

    public async Task<Result> Handle(CreateRoadmapLinkCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var roadmap = await _planningDbContext.Roadmaps
                .Include(x => x.Managers)
                .Include(x => x.ChildLinks)
                .FirstOrDefaultAsync(r => r.Id == request.ParentId, cancellationToken);

            if (roadmap is null)
                return Result.Failure($"Roadmap with id {request.ParentId} not found");

            var childRoadmapExists = await _planningDbContext.Roadmaps
                .AnyAsync(r => r.Id == request.ChildId, cancellationToken);
            if (!childRoadmapExists)
                return Result.Failure($"Child Roadmap with id {request.ChildId} not found");

            var addLinkResult = roadmap.AddChildLink(request.ChildId, _currentUserEmployeeId);
            if (addLinkResult.IsFailure)
            {
                _logger.LogError("Moda Request: Failure for Request {Name} {@Request}.  Error message: {Error}", request.GetType().Name, request, addLinkResult.Error);
                return Result.Failure(addLinkResult.Error);
            }

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
