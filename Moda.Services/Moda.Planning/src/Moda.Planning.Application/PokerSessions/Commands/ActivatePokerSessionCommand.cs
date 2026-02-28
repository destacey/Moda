using Moda.Planning.Application.PokerSessions.Interfaces;

namespace Moda.Planning.Application.PokerSessions.Commands;

public sealed record ActivatePokerSessionCommand(Guid Id) : ICommand;

public sealed class ActivatePokerSessionCommandValidator : CustomValidator<ActivatePokerSessionCommand>
{
    public ActivatePokerSessionCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}

internal sealed class ActivatePokerSessionCommandHandler(IPlanningDbContext planningDbContext, IDateTimeProvider dateTimeProvider, IPokerSessionNotifier notifier, ILogger<ActivatePokerSessionCommandHandler> logger) : ICommandHandler<ActivatePokerSessionCommand>
{
    private readonly IPlanningDbContext _planningDbContext = planningDbContext;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly IPokerSessionNotifier _notifier = notifier;
    private readonly ILogger<ActivatePokerSessionCommandHandler> _logger = logger;

    public async Task<Result> Handle(ActivatePokerSessionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var session = await _planningDbContext.PokerSessions
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (session is null)
                return Result.Failure("Poker session not found.");

            var result = session.Activate(_dateTimeProvider.Now);
            if (result.IsFailure)
                return result;

            await _planningDbContext.SaveChangesAsync(cancellationToken);
            await _notifier.NotifySessionActivated(session.Id);

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
