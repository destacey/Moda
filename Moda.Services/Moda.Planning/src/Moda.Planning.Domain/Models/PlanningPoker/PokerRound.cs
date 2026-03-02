using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Planning.Domain.Enums;
using NodaTime;

namespace Moda.Planning.Domain.Models.PlanningPoker;

public class PokerRound : BaseEntity<Guid>
{
    private readonly List<PokerVote> _votes = [];

    private PokerRound() { }

    internal PokerRound(Guid pokerSessionId, string? label, int order)
    {
        PokerSessionId = pokerSessionId;
        Label = label?.Trim();
        Order = order;
        Status = PokerRoundStatus.Voting;
    }

    public Guid PokerSessionId { get; private set; }

    /// <summary>
    /// Free-text label describing what is being estimated (e.g., a work item key + title, or any descriptive text).
    /// </summary>
    public string? Label { get; private set; }

    public PokerRoundStatus Status { get; private set; }

    /// <summary>
    /// The consensus estimate agreed upon by the team after voting.
    /// </summary>
    public string? ConsensusEstimate { get; private set; }

    /// <summary>
    /// The display order of this round within the session.
    /// </summary>
    public int Order { get; private set; }

    public IReadOnlyCollection<PokerVote> Votes => _votes.AsReadOnly();

    /// <summary>
    /// Reveal all votes for this round.
    /// </summary>
    internal Result Reveal()
    {
        if (Status != PokerRoundStatus.Voting)
            return Result.Failure("Votes can only be revealed when the round is in Voting status.");

        Status = PokerRoundStatus.Revealed;
        return Result.Success();
    }

    /// <summary>
    /// Reset the round to allow re-voting. Clears all existing votes.
    /// </summary>
    internal Result Reset()
    {
        if (Status is not (PokerRoundStatus.Voting or PokerRoundStatus.Revealed))
            return Result.Failure("Round can only be reset from Voting or Revealed status.");

        _votes.Clear();
        Status = PokerRoundStatus.Voting;
        return Result.Success();
    }

    /// <summary>
    /// Set the consensus estimate for this round.
    /// </summary>
    internal Result SetConsensus(string estimate)
    {
        if (Status != PokerRoundStatus.Revealed)
            return Result.Failure("Consensus can only be set after votes are revealed.");

        ConsensusEstimate = Guard.Against.NullOrWhiteSpace(estimate, nameof(estimate)).Trim();
        Status = PokerRoundStatus.Accepted;
        return Result.Success();
    }

    /// <summary>
    /// Update the label for this round.
    /// </summary>
    internal Result UpdateLabel(string? newLabel)
    {
        Label = newLabel?.Trim();
        return Result.Success();
    }

    /// <summary>
    /// Submit or update a vote for this round.
    /// </summary>
    internal Result AddOrUpdateVote(Guid participantId, string value, Instant timestamp)
    {
        if (Status != PokerRoundStatus.Voting)
            return Result.Failure("Votes can only be submitted when the round is in Voting status.");

        var existingVote = _votes.FirstOrDefault(v => v.ParticipantId == participantId);
        if (existingVote is not null)
        {
            existingVote.Update(value, timestamp);
        }
        else
        {
            _votes.Add(new PokerVote(Id, participantId, value, timestamp));
        }

        return Result.Success();
    }
}
