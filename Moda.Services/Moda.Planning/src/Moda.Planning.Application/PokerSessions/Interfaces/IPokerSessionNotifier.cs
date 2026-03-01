using Moda.Planning.Application.PokerSessions.Dtos;

namespace Moda.Planning.Application.PokerSessions.Interfaces;

public interface IPokerSessionNotifier
{
    Task NotifySessionCompleted(Guid sessionId);
    Task NotifyRoundAdded(Guid sessionId, PokerRoundDto round);
    Task NotifyRoundRemoved(Guid sessionId, Guid roundId);
    Task NotifyVoteSubmitted(Guid sessionId, Guid roundId, Guid participantId, string participantName);
    Task NotifyVotesRevealed(Guid sessionId, Guid roundId, IEnumerable<PokerVoteDto> votes);
    Task NotifyConsensusSet(Guid sessionId, Guid roundId, string estimate);
    Task NotifyRoundReset(Guid sessionId, Guid roundId);
}
