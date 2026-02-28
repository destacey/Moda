using Moda.Planning.Application.PokerSessions.Interfaces;

namespace Moda.Planning.Application.PokerSessions.Commands;

public sealed record StartPokerRoundCommand(Guid SessionId, Guid RoundId) : ICommand;

public sealed class StartPokerRoundCommandValidator : CustomValidator<StartPokerRoundCommand>
{
    public StartPokerRoundCommandValidator()
    {
        RuleFor(c => c.SessionId).NotEmpty();
        RuleFor(c => c.RoundId).NotEmpty();
    }
}

internal sealed class StartPokerRoundCommandHandler(IPlanningDbContext planningDbContext, IPokerSessionNotifier notifier, ILogger<StartPokerRoundCommandHandler> logger) : ICommandHandler<StartPokerRoundCommand>
{
    private readonly IPlanningDbContext _planningDbContext = planningDbContext;
    private readonly IPokerSessionNotifier _notifier = notifier;
    private readonly ILogger<StartPokerRoundCommandHandler> _logger = logger;

    public async Task<Result> Handle(StartPokerRoundCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var session = await _planningDbContext.PokerSessions
                .Include(s => s.Rounds)
                .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

            if (session is null)
                return Result.Failure("Poker session not found.");

            var result = session.StartRound(request.RoundId);
            if (result.IsFailure)
                return Result.Failure(result.Error);

            await _planningDbContext.SaveChangesAsync(cancellationToken);
            await _notifier.NotifyRoundStarted(session.Id, result.Value.Id, result.Value.Label);

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
