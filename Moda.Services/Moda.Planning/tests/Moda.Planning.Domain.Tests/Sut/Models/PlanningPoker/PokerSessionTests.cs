using Moda.Planning.Domain.Enums;
using Moda.Planning.Domain.Models.PlanningPoker;
using NodaTime.Extensions;

namespace Moda.Planning.Domain.Tests.Sut.Models.PlanningPoker;

public class PokerSessionTests
{
    private static readonly Instant Now = DateTime.UtcNow.ToInstant();

    private static PokerSession CreateActiveSession()
    {
        var session = PokerSession.Create("Sprint 1 Refinement", 1, Guid.NewGuid()).Value;
        session.Activate(Now);
        return session;
    }

    private static (string Value, int Order)[] FibonacciValues =>
    [
        ("0", 0), ("1", 1), ("2", 2), ("3", 3), ("5", 4), ("8", 5), ("13", 6)
    ];

    #region Create

    [Fact]
    public void Create_ValidParameters_ShouldReturnSuccess()
    {
        // Arrange
        var scaleId = 1;
        var facilitatorId = Guid.NewGuid();

        // Act
        var result = PokerSession.Create("Sprint 1 Refinement", scaleId, facilitatorId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Sprint 1 Refinement");
        result.Value.EstimationScaleId.Should().Be(scaleId);
        result.Value.FacilitatorId.Should().Be(facilitatorId);
        result.Value.Status.Should().Be(PokerSessionStatus.Created);
        result.Value.ActivatedOn.Should().BeNull();
        result.Value.CompletedOn.Should().BeNull();
        result.Value.Rounds.Should().BeEmpty();
    }

    [Fact]
    public void Create_WhitespaceName_ShouldReturnFailure()
    {
        // Act
        var result = PokerSession.Create("  ", 1, Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Activate

    [Fact]
    public void Activate_FromCreated_ShouldReturnSuccess()
    {
        // Arrange
        var session = PokerSession.Create("Test", 1, Guid.NewGuid()).Value;

        // Act
        var result = session.Activate(Now);

        // Assert
        result.IsSuccess.Should().BeTrue();
        session.Status.Should().Be(PokerSessionStatus.Active);
        session.ActivatedOn.Should().Be(Now);
    }

    [Fact]
    public void Activate_FromActive_ShouldReturnFailure()
    {
        // Arrange
        var session = CreateActiveSession();

        // Act
        var result = session.Activate(Now);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Complete

    [Fact]
    public void Complete_FromActive_ShouldReturnSuccess()
    {
        // Arrange
        var session = CreateActiveSession();

        // Act
        var result = session.Complete(Now);

        // Assert
        result.IsSuccess.Should().BeTrue();
        session.Status.Should().Be(PokerSessionStatus.Completed);
        session.CompletedOn.Should().Be(Now);
    }

    [Fact]
    public void Complete_FromCreated_ShouldReturnFailure()
    {
        // Arrange
        var session = PokerSession.Create("Test", 1, Guid.NewGuid()).Value;

        // Act
        var result = session.Complete(Now);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region AddRound

    [Fact]
    public void AddRound_ActiveSession_ShouldReturnSuccess()
    {
        // Arrange
        var session = CreateActiveSession();

        // Act
        var result = session.AddRound("WI-123: Implement login page");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Label.Should().Be("WI-123: Implement login page");
        result.Value.Status.Should().Be(PokerRoundStatus.Pending);
        result.Value.Order.Should().Be(0);
        session.Rounds.Should().HaveCount(1);
    }

    [Fact]
    public void AddRound_CreatedSession_ShouldReturnFailure()
    {
        // Arrange
        var session = PokerSession.Create("Test", 1, Guid.NewGuid()).Value;

        // Act
        var result = session.AddRound("WI-1: Test");

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void AddRound_MultipleRounds_ShouldIncrementOrder()
    {
        // Arrange
        var session = CreateActiveSession();

        // Act
        var round1 = session.AddRound("WI-1: First").Value;
        var round2 = session.AddRound("WI-2: Second").Value;
        var round3 = session.AddRound("WI-3: Third").Value;

        // Assert
        round1.Order.Should().Be(0);
        round2.Order.Should().Be(1);
        round3.Order.Should().Be(2);
    }

    #endregion

    #region RemoveRound

    [Fact]
    public void RemoveRound_PendingRound_ShouldReturnSuccess()
    {
        // Arrange
        var session = CreateActiveSession();
        var round = session.AddRound("WI-1: Test").Value;

        // Act
        var result = session.RemoveRound(round.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        session.Rounds.Should().BeEmpty();
    }

    [Fact]
    public void RemoveRound_VotingRound_ShouldReturnFailure()
    {
        // Arrange
        var session = CreateActiveSession();
        var round = session.AddRound("WI-1: Test").Value;
        session.StartRound(round.Id);

        // Act
        var result = session.RemoveRound(round.Id);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("currently being voted on");
    }

    [Fact]
    public void RemoveRound_NonExistentRound_ShouldReturnFailure()
    {
        // Arrange
        var session = CreateActiveSession();

        // Act
        var result = session.RemoveRound(Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    #endregion

    #region StartRound

    [Fact]
    public void StartRound_PendingRound_ShouldReturnSuccess()
    {
        // Arrange
        var session = CreateActiveSession();
        var round = session.AddRound("WI-1: Test").Value;

        // Act
        var result = session.StartRound(round.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(PokerRoundStatus.Voting);
    }

    #endregion

    #region Vote

    [Fact]
    public void SubmitVote_VotingRound_ShouldReturnSuccess()
    {
        // Arrange
        var session = CreateActiveSession();
        var round = session.AddRound("WI-1: Test").Value;
        session.StartRound(round.Id);
        var participantId = Guid.NewGuid();

        // Act
        var result = session.SubmitVote(round.Id, participantId, "5", Now);

        // Assert
        result.IsSuccess.Should().BeTrue();
        round.Votes.Should().HaveCount(1);
        round.Votes.First().ParticipantId.Should().Be(participantId);
        round.Votes.First().Value.Should().Be("5");
    }

    [Fact]
    public void SubmitVote_SameParticipantTwice_ShouldUpdateExistingVote()
    {
        // Arrange
        var session = CreateActiveSession();
        var round = session.AddRound("WI-1: Test").Value;
        session.StartRound(round.Id);
        var participantId = Guid.NewGuid();
        session.SubmitVote(round.Id, participantId, "3", Now);

        // Act
        var laterTimestamp = Now.Plus(Duration.FromMinutes(1));
        var result = session.SubmitVote(round.Id, participantId, "8", laterTimestamp);

        // Assert
        result.IsSuccess.Should().BeTrue();
        round.Votes.Should().HaveCount(1);
        round.Votes.First().Value.Should().Be("8");
        round.Votes.First().SubmittedOn.Should().Be(laterTimestamp);
    }

    [Fact]
    public void SubmitVote_PendingRound_ShouldReturnFailure()
    {
        // Arrange
        var session = CreateActiveSession();
        var round = session.AddRound("WI-1: Test").Value;

        // Act
        var result = session.SubmitVote(round.Id, Guid.NewGuid(), "5", Now);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region RevealRound

    [Fact]
    public void RevealRound_VotingRound_ShouldReturnSuccess()
    {
        // Arrange
        var session = CreateActiveSession();
        var round = session.AddRound("WI-1: Test").Value;
        session.StartRound(round.Id);
        session.SubmitVote(round.Id, Guid.NewGuid(), "5", Now);

        // Act
        var result = session.RevealRound(round.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(PokerRoundStatus.Revealed);
    }

    [Fact]
    public void RevealRound_PendingRound_ShouldReturnFailure()
    {
        // Arrange
        var session = CreateActiveSession();
        var round = session.AddRound("WI-1: Test").Value;

        // Act
        var result = session.RevealRound(round.Id);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region ResetRound

    [Fact]
    public void ResetRound_RevealedRound_ShouldClearVotesAndReturnToVoting()
    {
        // Arrange
        var session = CreateActiveSession();
        var round = session.AddRound("WI-1: Test").Value;
        session.StartRound(round.Id);
        session.SubmitVote(round.Id, Guid.NewGuid(), "5", Now);
        session.RevealRound(round.Id);

        // Act
        var result = session.ResetRound(round.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(PokerRoundStatus.Voting);
        result.Value.Votes.Should().BeEmpty();
    }

    [Fact]
    public void ResetRound_PendingRound_ShouldReturnFailure()
    {
        // Arrange
        var session = CreateActiveSession();
        var round = session.AddRound("WI-1: Test").Value;

        // Act
        var result = session.ResetRound(round.Id);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region SetConsensus

    [Fact]
    public void SetConsensus_RevealedRound_ShouldReturnSuccess()
    {
        // Arrange
        var session = CreateActiveSession();
        var round = session.AddRound("WI-1: Test").Value;
        session.StartRound(round.Id);
        session.SubmitVote(round.Id, Guid.NewGuid(), "5", Now);
        session.RevealRound(round.Id);

        // Act
        var result = session.SetConsensus(round.Id, "5");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ConsensusEstimate.Should().Be("5");
        result.Value.Status.Should().Be(PokerRoundStatus.Accepted);
    }

    [Fact]
    public void SetConsensus_VotingRound_ShouldReturnFailure()
    {
        // Arrange
        var session = CreateActiveSession();
        var round = session.AddRound("WI-1: Test").Value;
        session.StartRound(round.Id);

        // Act
        var result = session.SetConsensus(round.Id, "5");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("revealed");
    }

    #endregion

    #region Full Lifecycle

    [Fact]
    public void FullLifecycle_CreateActivateVoteRevealConsensusComplete_ShouldSucceed()
    {
        // Create session
        var session = PokerSession.Create("Sprint 1", 1, Guid.NewGuid()).Value;
        session.Status.Should().Be(PokerSessionStatus.Created);

        // Activate
        session.Activate(Now).IsSuccess.Should().BeTrue();
        session.Status.Should().Be(PokerSessionStatus.Active);

        // Add rounds
        var round1 = session.AddRound("WI-1: Story 1").Value;
        var round2 = session.AddRound("WI-2: Story 2").Value;

        // Start voting on round 1
        session.StartRound(round1.Id).IsSuccess.Should().BeTrue();

        // Multiple participants vote
        var participant1 = Guid.NewGuid();
        var participant2 = Guid.NewGuid();
        var participant3 = Guid.NewGuid();
        session.SubmitVote(round1.Id, participant1, "5", Now).IsSuccess.Should().BeTrue();
        session.SubmitVote(round1.Id, participant2, "8", Now).IsSuccess.Should().BeTrue();
        session.SubmitVote(round1.Id, participant3, "5", Now).IsSuccess.Should().BeTrue();

        // Reveal votes
        session.RevealRound(round1.Id).IsSuccess.Should().BeTrue();
        round1.Votes.Should().HaveCount(3);

        // Set consensus
        session.SetConsensus(round1.Id, "5").IsSuccess.Should().BeTrue();
        round1.ConsensusEstimate.Should().Be("5");
        round1.Status.Should().Be(PokerRoundStatus.Accepted);

        // Estimate round 2
        session.StartRound(round2.Id).IsSuccess.Should().BeTrue();
        session.SubmitVote(round2.Id, participant1, "3", Now).IsSuccess.Should().BeTrue();
        session.SubmitVote(round2.Id, participant2, "3", Now).IsSuccess.Should().BeTrue();
        session.RevealRound(round2.Id).IsSuccess.Should().BeTrue();
        session.SetConsensus(round2.Id, "3").IsSuccess.Should().BeTrue();

        // Complete session
        session.Complete(Now).IsSuccess.Should().BeTrue();
        session.Status.Should().Be(PokerSessionStatus.Completed);
        session.Rounds.Should().HaveCount(2);
    }

    #endregion
}
