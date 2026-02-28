namespace Moda.Planning.Application.PokerSessions.Dtos;

public class VotingStatusDto
{
    public Guid ParticipantId { get; set; }
    public string ParticipantName { get; set; } = default!;
    public bool HasVoted { get; set; }
}
