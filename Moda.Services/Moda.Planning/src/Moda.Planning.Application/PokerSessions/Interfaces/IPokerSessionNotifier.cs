using Moda.Planning.Application.PokerSessions.Dtos;

namespace Moda.Planning.Application.PokerSessions.Interfaces;

public interface IPokerSessionNotifier
{
    Task NotifySessionActivated(Guid sessionId);
    Task NotifySessionCompleted(Guid sessionId);
    Task NotifyRoundAdded(Guid sessionId, PokerRoundDto round);
    Task NotifyRoundRemoved(Guid sessionId, Guid roundId);
    Task NotifyRoundStarted(Guid sessionId, Guid roundId, string label);
    Task NotifyVoteSubmitted(Guid sessionId, Guid roundId, Guid participantId, string participantName);
    Task NotifyVotesRevealed(Guid sessionId, Guid roundId, IEnumerable<VoteDto> votes);
    Task NotifyConsensusSet(Guid sessionId, Guid roundId, string estimate);
    Task NotifyRoundReset(Guid sessionId, Guid roundId);
}
