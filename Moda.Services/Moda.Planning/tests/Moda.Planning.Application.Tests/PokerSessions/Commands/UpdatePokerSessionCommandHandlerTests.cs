using Microsoft.Extensions.Logging;
using Moda.Planning.Application.PokerSessions.Commands;
using Moda.Planning.Application.PokerSessions.Interfaces;
using Moda.Planning.Application.Tests.Infrastructure;
using Moda.Planning.Domain.Enums;
using Moda.Planning.Domain.Tests.Data;
using Moq;

namespace Moda.Planning.Application.Tests.PokerSessions.Commands;

public class UpdatePokerSessionCommandHandlerTests : IDisposable
{
    private readonly FakePlanningDbContext _dbContext;
    private readonly UpdatePokerSessionCommandHandler _handler;
    private readonly Mock<ILogger<UpdatePokerSessionCommandHandler>> _mockLogger;
    private readonly Mock<IPokerSessionNotifier> _mockNotifier;

    private readonly PokerSessionFaker _sessionFaker;
    private readonly EstimationScaleFaker _scaleFaker;

    public UpdatePokerSessionCommandHandlerTests()
    {
        _dbContext = new FakePlanningDbContext();
        _mockLogger = new Mock<ILogger<UpdatePokerSessionCommandHandler>>();
        _mockNotifier = new Mock<IPokerSessionNotifier>();

        _handler = new UpdatePokerSessionCommandHandler(_dbContext, _mockNotifier.Object, _mockLogger.Object);
        _sessionFaker = new PokerSessionFaker();
        _scaleFaker = new EstimationScaleFaker();
    }

    [Fact]
    public async Task Handle_ShouldUpdateNameAndScale_WhenNoRoundsExist()
    {
        // Arrange
        var originalScale = _scaleFaker.WithId(1).Generate();
        var newScale = _scaleFaker.WithId(2).Generate();
        _dbContext.AddEstimationScale(originalScale);
        _dbContext.AddEstimationScale(newScale);

        var session = _sessionFaker
            .WithStatus(PokerSessionStatus.Active)
            .WithEstimationScaleId(originalScale.Id)
            .Generate();
        _dbContext.AddPokerSession(session);

        var command = new UpdatePokerSessionCommand(session.Id, "Updated Name", newScale.Id);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        session.Name.Should().Be("Updated Name");
        session.EstimationScaleId.Should().Be(newScale.Id);
        _dbContext.SaveChangesCallCount.Should().Be(1);
        _mockNotifier.Verify(n => n.NotifySessionUpdated(session.Id), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdateNameOnly_WhenRoundsExistAndScaleUnchanged()
    {
        // Arrange
        var scale = _scaleFaker.WithId(1).Generate();
        _dbContext.AddEstimationScale(scale);

        var session = _sessionFaker
            .WithStatus(PokerSessionStatus.Active)
            .WithEstimationScaleId(scale.Id)
            .Generate();
        _dbContext.AddPokerSession(session);

        session.AddRound("Round 1");

        var command = new UpdatePokerSessionCommand(session.Id, "Updated Name", scale.Id);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        session.Name.Should().Be("Updated Name");
        session.EstimationScaleId.Should().Be(scale.Id);
        _dbContext.SaveChangesCallCount.Should().Be(1);
        _mockNotifier.Verify(n => n.NotifySessionUpdated(session.Id), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenChangingScaleWithExistingRounds()
    {
        // Arrange
        var originalScale = _scaleFaker.WithId(1).Generate();
        var newScale = _scaleFaker.WithId(2).Generate();
        _dbContext.AddEstimationScale(originalScale);
        _dbContext.AddEstimationScale(newScale);

        var session = _sessionFaker
            .WithStatus(PokerSessionStatus.Active)
            .WithEstimationScaleId(originalScale.Id)
            .Generate();
        _dbContext.AddPokerSession(session);

        session.AddRound("Round 1");

        var command = new UpdatePokerSessionCommand(session.Id, "Updated Name", newScale.Id);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("scale");
        session.EstimationScaleId.Should().Be(originalScale.Id);
        _dbContext.SaveChangesCallCount.Should().Be(0);
        _mockNotifier.Verify(n => n.NotifySessionUpdated(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenSessionDoesNotExist()
    {
        // Arrange
        var command = new UpdatePokerSessionCommand(Guid.NewGuid(), "Name", 1);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        _dbContext.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenEstimationScaleDoesNotExist()
    {
        // Arrange
        var originalScale = _scaleFaker.WithId(1).Generate();
        _dbContext.AddEstimationScale(originalScale);

        var session = _sessionFaker
            .WithStatus(PokerSessionStatus.Active)
            .WithEstimationScaleId(originalScale.Id)
            .Generate();
        _dbContext.AddPokerSession(session);

        var command = new UpdatePokerSessionCommand(session.Id, "Updated Name", 999);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        _dbContext.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenSessionIsCompleted()
    {
        // Arrange
        var scale = _scaleFaker.WithId(1).Generate();
        _dbContext.AddEstimationScale(scale);

        var session = _sessionFaker
            .WithStatus(PokerSessionStatus.Completed)
            .WithEstimationScaleId(scale.Id)
            .Generate();
        _dbContext.AddPokerSession(session);

        var command = new UpdatePokerSessionCommand(session.Id, "Updated Name", scale.Id);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        _dbContext.SaveChangesCallCount.Should().Be(0);
        _mockNotifier.Verify(n => n.NotifySessionUpdated(It.IsAny<Guid>()), Times.Never);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
