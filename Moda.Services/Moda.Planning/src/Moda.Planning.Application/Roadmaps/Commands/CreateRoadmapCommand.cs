using Ardalis.GuardClauses;
using Moda.Common.Application.Models;
using Moda.Common.Domain.Enums;

namespace Moda.Planning.Application.Roadmaps.Commands;
public sealed record CreateRoadmapCommand(string Name, string? Description, LocalDateRange DateRange, Visibility Visibility, Guid? ParentId) : ICommand<ObjectIdAndKey>;

public sealed class CreateRoadmapCommandValidator : AbstractValidator<CreateRoadmapCommand>
{
    public CreateRoadmapCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.Description)
            .MaximumLength(2048);

        RuleFor(x => x.DateRange)
            .NotNull();

        RuleFor(x => x.Visibility)
            .IsInEnum();

        When(x => x.ParentId.HasValue, () =>
        {
            RuleFor(x => x.ParentId)
                .NotEmpty();
        });
    }
}

internal sealed class CreateRoadmapCommandHandler(IPlanningDbContext planningDbContext, ICurrentUser currentUser, ILogger<CreateRoadmapCommandHandler> logger) : ICommandHandler<CreateRoadmapCommand, ObjectIdAndKey>
{
    private readonly IPlanningDbContext _planningDbContext = planningDbContext;
    private readonly Guid _currentUserEmployeeId = Guard.Against.NullOrEmpty(currentUser.GetEmployeeId());
    private readonly ILogger<CreateRoadmapCommandHandler> _logger = logger;

    public async Task<Result<ObjectIdAndKey>> Handle(CreateRoadmapCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = request.ParentId.HasValue
                ? await CreateChildRoadmap(request, cancellationToken)
                : await CreateRootRoadmap(request, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure<ObjectIdAndKey>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }

    private async Task<Result<ObjectIdAndKey>> CreateChildRoadmap(CreateRoadmapCommand request, CancellationToken cancellationToken)
    {
        var parentRoadmap = await _planningDbContext.Roadmaps
            .Include(r => r.Managers)
            .Include(r => r.Children)
            .FirstOrDefaultAsync(r => r.Id == request.ParentId, cancellationToken);

        if (parentRoadmap is null)
            return Result.Failure<ObjectIdAndKey>($"Parent Roadmap with id {request.ParentId} not found");

        var result = parentRoadmap.CreateChild(
            request.Name,
            request.Description,
            request.DateRange,
            request.Visibility,
            parentRoadmap.Managers.Select(m => m.ManagerId).ToArray(),
            _currentUserEmployeeId
            );

        if (result.IsFailure)
        {
            _logger.LogError("Moda Request: Failure for Request {Name} {@Request}.  Error message: {Error}", request.GetType().Name, request, result.Error);
            return Result.Failure<ObjectIdAndKey>(result.Error);
        }

        await _planningDbContext.SaveChangesAsync(cancellationToken);

        return new ObjectIdAndKey(result.Value.Id, result.Value.Key);
    }

    private async Task<Result<ObjectIdAndKey>> CreateRootRoadmap(CreateRoadmapCommand request, CancellationToken cancellationToken)
    {
        var result = Roadmap.CreateRoot(
                    request.Name,
                    request.Description,
                    request.DateRange,
                    request.Visibility,
                    [_currentUserEmployeeId]
                    );

        if (result.IsFailure)
        {
            _logger.LogError("Moda Request: Failure for Request {Name} {@Request}.  Error message: {Error}", request.GetType().Name, request, result.Error);
            return Result.Failure<ObjectIdAndKey>(result.Error);
        }

        await _planningDbContext.Roadmaps.AddAsync(result.Value, cancellationToken);
        await _planningDbContext.SaveChangesAsync(cancellationToken);

        return new ObjectIdAndKey(result.Value.Id, result.Value.Key);
    }
}
