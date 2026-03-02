using Microsoft.AspNetCore.SignalR;
using Moda.Planning.Application.PokerSessions.Dtos;
using Moda.Planning.Application.PokerSessions.Interfaces;

namespace Moda.Infrastructure.SignalR;

internal sealed class PlanningPokerNotifier(IHubContext<PlanningPokerHub> hubContext) : IPokerSessionNotifier
{
    private readonly IHubContext<PlanningPokerHub> _hubContext = hubContext;

    public async Task NotifySessionUpdated(Guid sessionId) =>
        await _hubContext.Clients.Group(sessionId.ToString())
            .SendAsync("SessionUpdated", sessionId);

    public async Task NotifySessionCompleted(Guid sessionId) =>
        await _hubContext.Clients.Group(sessionId.ToString())
            .SendAsync("SessionCompleted", sessionId);

    public async Task NotifyRoundAdded(Guid sessionId, PokerRoundDto round) =>
        await _hubContext.Clients.Group(sessionId.ToString())
            .SendAsync("RoundAdded", sessionId, round);

    public async Task NotifyRoundRemoved(Guid sessionId, Guid roundId) =>
        await _hubContext.Clients.Group(sessionId.ToString())
            .SendAsync("RoundRemoved", sessionId, roundId);

    public async Task NotifyVotesRevealed(Guid sessionId, Guid roundId, IEnumerable<PokerVoteDto> votes) =>
        await _hubContext.Clients.Group(sessionId.ToString())
            .SendAsync("VotesRevealed", sessionId, roundId, votes);

    public async Task NotifyRoundReset(Guid sessionId, Guid roundId) =>
        await _hubContext.Clients.Group(sessionId.ToString())
            .SendAsync("RoundReset", sessionId, roundId);

    public async Task NotifyConsensusSet(Guid sessionId, Guid roundId, string estimate) =>
        await _hubContext.Clients.Group(sessionId.ToString())
            .SendAsync("ConsensusSet", sessionId, roundId, estimate);

    public async Task NotifyVoteSubmitted(Guid sessionId, Guid roundId, Guid participantId, string participantName) =>
        await _hubContext.Clients.Group(sessionId.ToString())
            .SendAsync("VoteSubmitted", sessionId, roundId, participantId, participantName);

    public async Task NotifyRoundLabelUpdated(Guid sessionId, Guid roundId, string? label) =>
        await _hubContext.Clients.Group(sessionId.ToString())
            .SendAsync("RoundLabelUpdated", sessionId, roundId, label);
}
