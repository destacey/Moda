using Ardalis.GuardClauses;
using Wayd.Common.Domain.Identity;
using NodaTime;

namespace Wayd.Planning.Domain.Models.PlanningPoker;

public sealed class PokerVote : BaseEntity
{
    private PokerVote() { }

    internal PokerVote(Guid pokerRoundId, string participantId, string value, Instant submittedOn)
    {
        PokerRoundId = pokerRoundId;
        ParticipantId = participantId;
        Value = Guard.Against.NullOrWhiteSpace(value, nameof(Value)).Trim();
        SubmittedOn = submittedOn;
    }

    public Guid PokerRoundId { get; private set; }

    public string ParticipantId { get; private set; } = null!;

    public User? Participant { get; private set; }

    public string Value { get; private set; } = default!;

    public Instant SubmittedOn { get; private set; }

    internal void Update(string value, Instant timestamp)
    {
        Value = Guard.Against.NullOrWhiteSpace(value, nameof(Value)).Trim();
        SubmittedOn = timestamp;
    }
}
