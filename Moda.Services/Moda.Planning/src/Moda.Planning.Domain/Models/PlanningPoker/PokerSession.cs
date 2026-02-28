using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Domain.Employees;
using Moda.Common.Domain.Interfaces;
using Moda.Planning.Domain.Enums;
using NodaTime;

namespace Moda.Planning.Domain.Models.PlanningPoker;

public class PokerSession : BaseEntity<Guid>, ISystemAuditable, IHasIdAndKey
{
    private readonly List<PokerRound> _rounds = [];

    private PokerSession() { }

    private PokerSession(string name, int estimationScaleId, Guid facilitatorId)
    {
        Name = name;
        EstimationScaleId = estimationScaleId;
        FacilitatorId = facilitatorId;
        Status = PokerSessionStatus.Created;
    }

    /// <summary>
    /// The unique key of the Poker Session. This is an alternate key to the Id.
    /// </summary>
    public int Key { get; private init; }

    /// <summary>
    /// The name of the Poker Session.
    /// </summary>
    public string Name
    {
        get;
        private set => field = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    } = default!;

    /// <summary>
    /// The estimation scale used for voting in this session.
    /// </summary>
    public int EstimationScaleId { get; private set; }

    public EstimationScale? EstimationScale { get; private set; }

    /// <summary>
    /// The employee who facilitates this session.
    /// </summary>
    public Guid FacilitatorId { get; private set; }

    public Employee? Facilitator { get; private set; }

    /// <summary>
    /// The current status of the poker session.
    /// </summary>
    public PokerSessionStatus Status { get; private set; }

    /// <summary>
    /// The timestamp when the session was activated.
    /// </summary>
    public Instant? ActivatedOn { get; private set; }

    /// <summary>
    /// The timestamp when the session was completed.
    /// </summary>
    public Instant? CompletedOn { get; private set; }

    /// <summary>
    /// The rounds in this poker session, ordered by their display order.
    /// </summary>
    public IReadOnlyCollection<PokerRound> Rounds => _rounds.OrderBy(r => r.Order).ToList().AsReadOnly();

    /// <summary>
    /// Activate the session to begin accepting rounds and votes.
    /// </summary>
    public Result Activate(Instant timestamp)
    {
        if (Status != PokerSessionStatus.Created)
            return Result.Failure("Session can only be activated from Created status.");

        Status = PokerSessionStatus.Active;
        ActivatedOn = timestamp;
        return Result.Success();
    }

    /// <summary>
    /// Complete the session.
    /// </summary>
    public Result Complete(Instant timestamp)
    {
        if (Status != PokerSessionStatus.Active)
            return Result.Failure("Session can only be completed from Active status.");

        Status = PokerSessionStatus.Completed;
        CompletedOn = timestamp;
        return Result.Success();
    }

    /// <summary>
    /// Add a new round to this session.
    /// </summary>
    public Result<PokerRound> AddRound(string label)
    {
        if (Status != PokerSessionStatus.Active)
            return Result.Failure<PokerRound>("Rounds can only be added to an active session.");

        try
        {
            int nextOrder = _rounds.Count > 0 ? _rounds.Max(r => r.Order) + 1 : 0;
            var round = new PokerRound(Id, label, nextOrder);
            _rounds.Add(round);
            return Result.Success(round);
        }
        catch (Exception ex)
        {
            return Result.Failure<PokerRound>(ex.ToString());
        }
    }

    /// <summary>
    /// Remove a round from the session.
    /// </summary>
    public Result RemoveRound(Guid roundId)
    {
        if (Status != PokerSessionStatus.Active)
            return Result.Failure("Rounds can only be removed from an active session.");

        var round = _rounds.FirstOrDefault(r => r.Id == roundId);
        if (round is null)
            return Result.Failure("Round not found.");

        if (round.Status == PokerRoundStatus.Voting)
            return Result.Failure("Cannot remove a round that is currently being voted on.");

        _rounds.Remove(round);
        return Result.Success();
    }

    /// <summary>
    /// Start voting for a specific round.
    /// </summary>
    public Result<PokerRound> StartRound(Guid roundId)
    {
        if (Status != PokerSessionStatus.Active)
            return Result.Failure<PokerRound>("Cannot start a round when the session is not active.");

        var round = _rounds.FirstOrDefault(r => r.Id == roundId);
        if (round is null)
            return Result.Failure<PokerRound>("Round not found.");

        var result = round.Start();
        return result.IsFailure
            ? Result.Failure<PokerRound>(result.Error)
            : Result.Success(round);
    }

    /// <summary>
    /// Reveal votes for a specific round.
    /// </summary>
    public Result<PokerRound> RevealRound(Guid roundId)
    {
        if (Status != PokerSessionStatus.Active)
            return Result.Failure<PokerRound>("Cannot reveal votes when the session is not active.");

        var round = _rounds.FirstOrDefault(r => r.Id == roundId);
        if (round is null)
            return Result.Failure<PokerRound>("Round not found.");

        var result = round.Reveal();
        return result.IsFailure
            ? Result.Failure<PokerRound>(result.Error)
            : Result.Success(round);
    }

    /// <summary>
    /// Reset a round to allow re-voting.
    /// </summary>
    public Result<PokerRound> ResetRound(Guid roundId)
    {
        if (Status != PokerSessionStatus.Active)
            return Result.Failure<PokerRound>("Cannot reset a round when the session is not active.");

        var round = _rounds.FirstOrDefault(r => r.Id == roundId);
        if (round is null)
            return Result.Failure<PokerRound>("Round not found.");

        var result = round.Reset();
        return result.IsFailure
            ? Result.Failure<PokerRound>(result.Error)
            : Result.Success(round);
    }

    /// <summary>
    /// Set the consensus estimate for a round.
    /// </summary>
    public Result<PokerRound> SetConsensus(Guid roundId, string estimate)
    {
        if (Status != PokerSessionStatus.Active)
            return Result.Failure<PokerRound>("Cannot set consensus when the session is not active.");

        var round = _rounds.FirstOrDefault(r => r.Id == roundId);
        if (round is null)
            return Result.Failure<PokerRound>("Round not found.");

        var result = round.SetConsensus(estimate);
        return result.IsFailure
            ? Result.Failure<PokerRound>(result.Error)
            : Result.Success(round);
    }

    /// <summary>
    /// Submit a vote for a specific round.
    /// </summary>
    public Result SubmitVote(Guid roundId, Guid participantId, string value, Instant timestamp)
    {
        if (Status != PokerSessionStatus.Active)
            return Result.Failure("Cannot vote when the session is not active.");

        var round = _rounds.FirstOrDefault(r => r.Id == roundId);
        if (round is null)
            return Result.Failure("Round not found.");

        return round.AddOrUpdateVote(participantId, value, timestamp);
    }

    /// <summary>
    /// Create a new poker session.
    /// </summary>
    public static Result<PokerSession> Create(string name, int estimationScaleId, Guid facilitatorId)
    {
        try
        {
            var session = new PokerSession(name, estimationScaleId, facilitatorId);
            return Result.Success(session);
        }
        catch (Exception ex)
        {
            return Result.Failure<PokerSession>(ex.ToString());
        }
    }
}
