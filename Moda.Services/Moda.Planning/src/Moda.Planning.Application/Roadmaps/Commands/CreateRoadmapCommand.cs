using Ardalis.GuardClauses;
using Moda.Common.Application.Models;
using Moda.Common.Domain.Enums;

namespace Moda.Planning.Application.Roadmaps.Commands;
public sealed record CreateRoadmapCommand(string Name, string? Description, LocalDateRange DateRange, Visibility Visibility) : ICommand<ObjectIdAndKey>;

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
            var result = Roadmap.Create(
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
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure<ObjectIdAndKey>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}
