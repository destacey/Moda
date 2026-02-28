using Ardalis.GuardClauses;
using Moda.Common.Domain.Employees;
using NodaTime;

namespace Moda.Planning.Domain.Models.PlanningPoker;

public class PokerVote : BaseEntity<Guid>
{
    private PokerVote() { }

    internal PokerVote(Guid pokerRoundId, Guid participantId, string value, Instant submittedOn)
    {
        Id = Guid.CreateVersion7();
        PokerRoundId = pokerRoundId;
        ParticipantId = participantId;
        Value = Guard.Against.NullOrWhiteSpace(value, nameof(Value)).Trim();
        SubmittedOn = submittedOn;
    }

    public Guid PokerRoundId { get; private set; }

    public Guid ParticipantId { get; private set; }

    public Employee? Participant { get; private set; }

    public string Value { get; private set; } = default!;

    public Instant SubmittedOn { get; private set; }

    internal void Update(string value, Instant timestamp)
    {
        Value = Guard.Against.NullOrWhiteSpace(value, nameof(Value)).Trim();
        SubmittedOn = timestamp;
    }
}
