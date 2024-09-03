using Ardalis.GuardClauses;
using Moda.Common.Domain.Enums;

namespace Moda.Planning.Application.Roadmaps.Commands;
public sealed record UpdateRoadmapCommand(Guid Id, string Name, string? Description, LocalDateRange DateRange, Visibility Visibility) : ICommand;

public sealed class UpdateRoadmapCommandValidator : AbstractValidator<UpdateRoadmapCommand>
{
    public UpdateRoadmapCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

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

internal sealed class UpdateRoadmapCommandHandler(IPlanningDbContext planningDbContext, ICurrentUser currentUser, ILogger<UpdateRoadmapCommandHandler> logger) : ICommandHandler<UpdateRoadmapCommand>
{
    private readonly IPlanningDbContext _planningDbContext = planningDbContext;
    private readonly Guid _currentUserEmployeeId = Guard.Against.NullOrEmpty(currentUser.GetEmployeeId());
    private readonly ILogger<UpdateRoadmapCommandHandler> _logger = logger;

    public async Task<Result> Handle(UpdateRoadmapCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var roadmap = await _planningDbContext.Roadmaps
                .Include(x => x.Managers)
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (roadmap is null)
                return Result.Failure($"Roadmap with id {request.Id} not found");

            var updateResult = roadmap.Update(
                request.Name,
                request.Description,
                request.DateRange,
                request.Visibility,
                _currentUserEmployeeId
                );

            if (updateResult.IsFailure)
            {
                // Reset the entity
                await _planningDbContext.Entry(roadmap).ReloadAsync(cancellationToken);
                roadmap.ClearDomainEvents();

                var requestName = request.GetType().Name;
                _logger.LogError("Moda Request: Failure for Request {Name} {@Request}.  Error message: {Error}", requestName, request, updateResult.Error);
                return Result.Failure(updateResult.Error);
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
