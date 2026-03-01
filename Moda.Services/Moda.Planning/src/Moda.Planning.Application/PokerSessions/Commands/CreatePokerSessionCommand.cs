using Ardalis.GuardClauses;
using Moda.Common.Application.Models;
using Moda.Planning.Domain.Models.PlanningPoker;

namespace Moda.Planning.Application.PokerSessions.Commands;

public sealed record CreatePokerSessionCommand(string Name, int EstimationScaleId) : ICommand<ObjectIdAndKey>;

public sealed class CreatePokerSessionCommandValidator : CustomValidator<CreatePokerSessionCommand>
{
    public CreatePokerSessionCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(c => c.EstimationScaleId)
            .GreaterThan(0);
    }
}

internal sealed class CreatePokerSessionCommandHandler(IPlanningDbContext planningDbContext, ICurrentUser currentUser, IDateTimeProvider dateTimeProvider, ILogger<CreatePokerSessionCommandHandler> logger) : ICommandHandler<CreatePokerSessionCommand, ObjectIdAndKey>
{
    private readonly IPlanningDbContext _planningDbContext = planningDbContext;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly ILogger<CreatePokerSessionCommandHandler> _logger = logger;
    private readonly Guid _currentUserEmployeeId = Guard.Against.NullOrEmpty(currentUser.GetEmployeeId());

    public async Task<Result<ObjectIdAndKey>> Handle(CreatePokerSessionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var scaleExists = await _planningDbContext.EstimationScales
                .AnyAsync(s => s.Id == request.EstimationScaleId, cancellationToken);

            if (!scaleExists)
                return Result.Failure<ObjectIdAndKey>("Estimation scale not found.");

            var sessionResult = PokerSession.Create(
                request.Name,
                request.EstimationScaleId,
                _currentUserEmployeeId,
                _dateTimeProvider.Now);

            if (sessionResult.IsFailure)
                return Result.Failure<ObjectIdAndKey>(sessionResult.Error);

            await _planningDbContext.PokerSessions.AddAsync(sessionResult.Value, cancellationToken);
            await _planningDbContext.SaveChangesAsync(cancellationToken);

            return new ObjectIdAndKey(sessionResult.Value.Id, sessionResult.Value.Key);
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;
            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);
            return Result.Failure<ObjectIdAndKey>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}
