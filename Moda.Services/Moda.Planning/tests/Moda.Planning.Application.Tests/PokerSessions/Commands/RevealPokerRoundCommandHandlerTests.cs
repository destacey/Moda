using Microsoft.Extensions.Logging;
using Moda.Planning.Application.PokerSessions.Commands;
using Moda.Planning.Application.PokerSessions.Dtos;
using Moda.Planning.Application.PokerSessions.Interfaces;
using Moda.Planning.Application.Tests.Infrastructure;
using Moda.Planning.Domain.Enums;
using Moda.Planning.Domain.Tests.Data;
using Moq;
using NodaTime;

namespace Moda.Planning.Application.Tests.PokerSessions.Commands;

public class RevealPokerRoundCommandHandlerTests : IDisposable
{
    private readonly FakePlanningDbContext _dbContext;
    private readonly RevealPokerRoundCommandHandler _handler;
    private readonly Mock<ILogger<RevealPokerRoundCommandHandler>> _mockLogger;
    private readonly Mock<IPokerSessionNotifier> _mockNotifier;

    private readonly PokerSessionFaker _sessionFaker;

    public RevealPokerRoundCommandHandlerTests()
    {
        _dbContext = new FakePlanningDbContext();
        _mockLogger = new Mock<ILogger<RevealPokerRoundCommandHandler>>();
        _mockNotifier = new Mock<IPokerSessionNotifier>();

        _handler = new RevealPokerRoundCommandHandler(_dbContext, _mockNotifier.Object, _mockLogger.Object);
        _sessionFaker = new PokerSessionFaker();
    }

    [Fact]
    public async Task Handle_ShouldRevealVotes_WhenRoundIsVoting()
    {
        // Arrange
        var session = _sessionFaker.WithStatus(PokerSessionStatus.Active).Generate();
        _dbContext.AddPokerSession(session);

        session.AddRound("Story to estimate");
        var round = session.Rounds.First();
        session.SubmitVote(round.Id, Guid.NewGuid(), "5", Instant.FromUtc(2026, 1, 15, 10, 0));

        var command = new RevealPokerRoundCommand(session.Id, round.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        round.Status.Should().Be(PokerRoundStatus.Revealed);
        _dbContext.SaveChangesCallCount.Should().Be(1);
        _mockNotifier.Verify(n => n.NotifyVotesRevealed(session.Id, round.Id, It.IsAny<IEnumerable<PokerVoteDto>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenSessionDoesNotExist()
    {
        // Arrange
        var command = new RevealPokerRoundCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        _dbContext.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenRoundIsAlreadyRevealed()
    {
        // Arrange
        var session = _sessionFaker.WithStatus(PokerSessionStatus.Active).Generate();
        _dbContext.AddPokerSession(session);

        session.AddRound("Story");
        var round = session.Rounds.First();
        session.SubmitVote(round.Id, Guid.NewGuid(), "5", Instant.FromUtc(2026, 1, 15, 10, 0));
        session.RevealRound(round.Id);

        var command = new RevealPokerRoundCommand(session.Id, round.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        _dbContext.SaveChangesCallCount.Should().Be(0);
        _mockNotifier.Verify(n => n.NotifyVotesRevealed(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IEnumerable<PokerVoteDto>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenRoundDoesNotExist()
    {
        // Arrange
        var session = _sessionFaker.WithStatus(PokerSessionStatus.Active).Generate();
        _dbContext.AddPokerSession(session);

        var command = new RevealPokerRoundCommand(session.Id, Guid.NewGuid());

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
