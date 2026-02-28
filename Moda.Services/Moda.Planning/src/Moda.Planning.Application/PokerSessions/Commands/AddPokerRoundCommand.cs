using Moda.Planning.Application.PokerSessions.Dtos;
using Moda.Planning.Application.PokerSessions.Interfaces;

namespace Moda.Planning.Application.PokerSessions.Commands;

public sealed record AddPokerRoundCommand(Guid SessionId, string Label) : ICommand<PokerRoundDto>;

public sealed class AddPokerRoundCommandValidator : CustomValidator<AddPokerRoundCommand>
{
    public AddPokerRoundCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.SessionId).NotEmpty();
        RuleFor(c => c.Label).NotEmpty().MaximumLength(512);
    }
}

internal sealed class AddPokerRoundCommandHandler(IPlanningDbContext planningDbContext, IPokerSessionNotifier notifier, ILogger<AddPokerRoundCommandHandler> logger) : ICommandHandler<AddPokerRoundCommand, PokerRoundDto>
{
    private readonly IPlanningDbContext _planningDbContext = planningDbContext;
    private readonly IPokerSessionNotifier _notifier = notifier;
    private readonly ILogger<AddPokerRoundCommandHandler> _logger = logger;

    public async Task<Result<PokerRoundDto>> Handle(AddPokerRoundCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var session = await _planningDbContext.PokerSessions
                .Include(s => s.Rounds)
                .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

            if (session is null)
                return Result.Failure<PokerRoundDto>("Poker session not found.");

            var result = session.AddRound(request.Label);
            if (result.IsFailure)
                return Result.Failure<PokerRoundDto>(result.Error);

            await _planningDbContext.SaveChangesAsync(cancellationToken);

            var roundDto = result.Value.Adapt<PokerRoundDto>();
            await _notifier.NotifyRoundAdded(session.Id, roundDto);

            return roundDto;
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;
            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);
            return Result.Failure<PokerRoundDto>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}
