using Microsoft.Extensions.Logging;
using Moda.Common.Application.Interfaces;
using Moda.Planning.Application.PokerSessions.Commands;
using Moda.Planning.Application.PokerSessions.Interfaces;
using Moda.Planning.Application.Tests.Infrastructure;
using Moda.Planning.Domain.Enums;
using Moda.Planning.Domain.Tests.Data;
using Moq;
using NodaTime;

namespace Moda.Planning.Application.Tests.PokerSessions.Commands;

public class ActivatePokerSessionCommandHandlerTests : IDisposable
{
    private readonly FakePlanningDbContext _dbContext;
    private readonly ActivatePokerSessionCommandHandler _handler;
    private readonly Mock<ILogger<ActivatePokerSessionCommandHandler>> _mockLogger;
    private readonly Mock<IPokerSessionNotifier> _mockNotifier;
    private readonly Mock<IDateTimeProvider> _mockDateTimeProvider;

    private readonly PokerSessionFaker _sessionFaker;

    public ActivatePokerSessionCommandHandlerTests()
    {
        _dbContext = new FakePlanningDbContext();
        _mockLogger = new Mock<ILogger<ActivatePokerSessionCommandHandler>>();
        _mockNotifier = new Mock<IPokerSessionNotifier>();
        _mockDateTimeProvider = new Mock<IDateTimeProvider>();
        _mockDateTimeProvider.Setup(d => d.Now).Returns(Instant.FromUtc(2026, 1, 15, 10, 0));

        _handler = new ActivatePokerSessionCommandHandler(_dbContext, _mockDateTimeProvider.Object, _mockNotifier.Object, _mockLogger.Object);
        _sessionFaker = new PokerSessionFaker();
    }

    [Fact]
    public async Task Handle_ShouldActivateSession_WhenSessionIsCreated()
    {
        // Arrange
        var session = _sessionFaker.WithStatus(PokerSessionStatus.Created).Generate();
        _dbContext.AddPokerSession(session);

        var command = new ActivatePokerSessionCommand(session.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        session.Status.Should().Be(PokerSessionStatus.Active);
        session.ActivatedOn.Should().NotBeNull();
        _dbContext.SaveChangesCallCount.Should().Be(1);
        _mockNotifier.Verify(n => n.NotifySessionActivated(session.Id), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenSessionDoesNotExist()
    {
        // Arrange
        var command = new ActivatePokerSessionCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        _dbContext.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenSessionIsAlreadyActive()
    {
        // Arrange
        var session = _sessionFaker.WithStatus(PokerSessionStatus.Active).Generate();
        _dbContext.AddPokerSession(session);

        var command = new ActivatePokerSessionCommand(session.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        _dbContext.SaveChangesCallCount.Should().Be(0);
        _mockNotifier.Verify(n => n.NotifySessionActivated(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenSessionIsCompleted()
    {
        // Arrange
        var session = _sessionFaker.WithStatus(PokerSessionStatus.Completed).Generate();
        _dbContext.AddPokerSession(session);

        var command = new ActivatePokerSessionCommand(session.Id);

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
