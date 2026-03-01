using Microsoft.Extensions.Logging;
using Moda.Planning.Application.PokerSessions.Commands;
using Moda.Planning.Application.PokerSessions.Interfaces;
using Moda.Planning.Application.Tests.Infrastructure;
using Moda.Planning.Domain.Enums;
using Moda.Planning.Domain.Tests.Data;
using Moq;
using NodaTime;

namespace Moda.Planning.Application.Tests.PokerSessions.Commands;

public class ResetPokerRoundCommandHandlerTests : IDisposable
{
    private readonly FakePlanningDbContext _dbContext;
    private readonly ResetPokerRoundCommandHandler _handler;
    private readonly Mock<ILogger<ResetPokerRoundCommandHandler>> _mockLogger;
    private readonly Mock<IPokerSessionNotifier> _mockNotifier;

    private readonly PokerSessionFaker _sessionFaker;

    public ResetPokerRoundCommandHandlerTests()
    {
        _dbContext = new FakePlanningDbContext();
        _mockLogger = new Mock<ILogger<ResetPokerRoundCommandHandler>>();
        _mockNotifier = new Mock<IPokerSessionNotifier>();

        _handler = new ResetPokerRoundCommandHandler(_dbContext, _mockNotifier.Object, _mockLogger.Object);
        _sessionFaker = new PokerSessionFaker();
    }

    [Fact]
    public async Task Handle_ShouldResetRound_WhenRoundIsRevealed()
    {
        // Arrange
        var session = _sessionFaker.WithStatus(PokerSessionStatus.Active).Generate();
        _dbContext.AddPokerSession(session);

        session.AddRound("Story");
        var round = session.Rounds.First();
        session.SubmitVote(round.Id, Guid.NewGuid(), "5", Instant.FromUtc(2026, 1, 15, 10, 0));
        session.RevealRound(round.Id);

        var command = new ResetPokerRoundCommand(session.Id, round.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        round.Status.Should().Be(PokerRoundStatus.Voting);
        round.Votes.Should().BeEmpty();
        _dbContext.SaveChangesCallCount.Should().Be(1);
        _mockNotifier.Verify(n => n.NotifyRoundReset(session.Id, round.Id), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldResetRound_WhenRoundIsVoting()
    {
        // Arrange
        var session = _sessionFaker.WithStatus(PokerSessionStatus.Active).Generate();
        _dbContext.AddPokerSession(session);

        session.AddRound("Story");
        var round = session.Rounds.First();


        var command = new ResetPokerRoundCommand(session.Id, round.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        round.Status.Should().Be(PokerRoundStatus.Voting);
        _dbContext.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenSessionDoesNotExist()
    {
        // Arrange
        var command = new ResetPokerRoundCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        _dbContext.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenRoundIsAccepted()
    {
        // Arrange
        var session = _sessionFaker.WithStatus(PokerSessionStatus.Active).Generate();
        _dbContext.AddPokerSession(session);

        session.AddRound("Story");
        var round = session.Rounds.First();
        session.SubmitVote(round.Id, Guid.NewGuid(), "5", Instant.FromUtc(2026, 1, 15, 10, 0));
        session.RevealRound(round.Id);
        session.SetConsensus(round.Id, "5");

        var command = new ResetPokerRoundCommand(session.Id, round.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        _dbContext.SaveChangesCallCount.Should().Be(0);
        _mockNotifier.Verify(n => n.NotifyRoundReset(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenRoundDoesNotExist()
    {
        // Arrange
        var session = _sessionFaker.WithStatus(PokerSessionStatus.Active).Generate();
        _dbContext.AddPokerSession(session);

        var command = new ResetPokerRoundCommand(session.Id, Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        _dbContext.SaveChangesCallCount.Should().Be(0);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
