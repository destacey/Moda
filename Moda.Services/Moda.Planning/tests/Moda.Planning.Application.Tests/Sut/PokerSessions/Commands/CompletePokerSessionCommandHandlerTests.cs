using Microsoft.Extensions.Logging;
using Moda.Common.Application.Interfaces;
using Moda.Planning.Application.PokerSessions.Commands;
using Moda.Planning.Application.PokerSessions.Interfaces;
using Moda.Planning.Application.Tests.Infrastructure;
using Moda.Planning.Domain.Enums;
using Moda.Planning.Domain.Tests.Data;
using Moq;
using NodaTime;

namespace Moda.Planning.Application.Tests.Sut.PokerSessions.Commands;

public class CompletePokerSessionCommandHandlerTests : IDisposable
{
    private readonly FakePlanningDbContext _dbContext;
    private readonly CompletePokerSessionCommandHandler _handler;
    private readonly Mock<ILogger<CompletePokerSessionCommandHandler>> _mockLogger;
    private readonly Mock<IPokerSessionNotifier> _mockNotifier;
    private readonly Mock<IDateTimeProvider> _mockDateTimeProvider;

    private readonly PokerSessionFaker _sessionFaker;

    public CompletePokerSessionCommandHandlerTests()
    {
        _dbContext = new FakePlanningDbContext();
        _mockLogger = new Mock<ILogger<CompletePokerSessionCommandHandler>>();
        _mockNotifier = new Mock<IPokerSessionNotifier>();
        _mockDateTimeProvider = new Mock<IDateTimeProvider>();
        _mockDateTimeProvider.Setup(d => d.Now).Returns(Instant.FromUtc(2026, 1, 15, 10, 0));

        _handler = new CompletePokerSessionCommandHandler(_dbContext, _mockDateTimeProvider.Object, _mockNotifier.Object, _mockLogger.Object);
        _sessionFaker = new PokerSessionFaker();
    }

    [Fact]
    public async Task Handle_ShouldCompleteSession_WhenSessionIsActive()
    {
        // Arrange
        var session = _sessionFaker.WithStatus(PokerSessionStatus.Active).Generate();
        _dbContext.AddPokerSession(session);

        var command = new CompletePokerSessionCommand(session.Id);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        session.Status.Should().Be(PokerSessionStatus.Completed);
        session.CompletedOn.Should().NotBeNull();
        _dbContext.SaveChangesCallCount.Should().Be(1);
        _mockNotifier.Verify(n => n.NotifySessionCompleted(session.Id), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenSessionDoesNotExist()
    {
        // Arrange
        var command = new CompletePokerSessionCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        _dbContext.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenSessionIsAlreadyCompleted()
    {
        // Arrange
        var session = _sessionFaker.WithStatus(PokerSessionStatus.Completed).Generate();
        _dbContext.AddPokerSession(session);

        var command = new CompletePokerSessionCommand(session.Id);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        _dbContext.SaveChangesCallCount.Should().Be(0);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
