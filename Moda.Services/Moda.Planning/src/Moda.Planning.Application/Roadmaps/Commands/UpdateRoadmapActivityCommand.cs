using Ardalis.GuardClauses;
using Moda.Planning.Domain.Interfaces.Roadmaps;

namespace Moda.Planning.Application.Roadmaps.Commands;
public sealed record UpdateRoadmapActivityCommand(Guid RoadmapId, Guid ActivityId, string Name, string? Description, Guid? ParentId, LocalDateRange DateRange, string? Color) : ICommand, IUpsertRoadmapActivity;

public sealed class UpdateRoadmapActivityCommandValidator : AbstractValidator<UpdateRoadmapActivityCommand>
{
    public UpdateRoadmapActivityCommandValidator()
    {
        RuleFor(x => x.RoadmapId)
            .NotEmpty();

        RuleFor(x => x.ActivityId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.Description)
            .MaximumLength(2048);

        When(x => x.ParentId.HasValue, () =>
        {
            RuleFor(x => x.ParentId)
                .NotEmpty();
        });

        RuleFor(x => x.DateRange)
            .NotNull();

        When(x => x.Color != null, () => RuleFor(x => x.Color)
            .Matches("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
            .WithMessage("Color must be a valid hex color code."));
    }
}

internal sealed class UpdateRoadmapActivityCommandHandler(IPlanningDbContext planningDbContext, ICurrentUser currentUser, ILogger<UpdateRoadmapActivityCommandHandler> logger) : ICommandHandler<UpdateRoadmapActivityCommand>
{
    private readonly IPlanningDbContext _planningDbContext = planningDbContext;
    private readonly Guid _currentUserEmployeeId = Guard.Against.NullOrEmpty(currentUser.GetEmployeeId());
    private readonly ILogger<UpdateRoadmapActivityCommandHandler> _logger = logger;

    public async Task<Result> Handle(UpdateRoadmapActivityCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var roadmap = await _planningDbContext.Roadmaps
                .Include(x => x.RoadmapManagers)
                .Include(x => x.Items)
                .FirstOrDefaultAsync(r => r.Id == request.RoadmapId, cancellationToken);

            if (roadmap is null)
                return Result.Failure<Guid>($"Roadmap with id {request.RoadmapId} not found");

            var result = roadmap.UpdateRoadmapActivity(
                request.ActivityId,
                request,
                _currentUserEmployeeId
                );

            if (result.IsFailure)
            {
                _logger.LogError("Moda Request: Failure for Request {Name} {@Request}.  Error message: {Error}", request.GetType().Name, request, result.Error);
                return Result.Failure<Guid>(result.Error);
            }

            await _planningDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure<Guid>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}
