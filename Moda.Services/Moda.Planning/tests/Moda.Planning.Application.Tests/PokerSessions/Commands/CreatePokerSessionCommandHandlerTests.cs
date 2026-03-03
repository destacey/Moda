using Microsoft.Extensions.Logging;
using Moda.Common.Application.Interfaces;
using Moda.Planning.Application.PokerSessions.Commands;
using Moda.Planning.Application.Tests.Infrastructure;
using Moda.Planning.Domain.Tests.Data;
using Moq;
using NodaTime;

namespace Moda.Planning.Application.Tests.PokerSessions.Commands;

public class CreatePokerSessionCommandHandlerTests : IDisposable
{
    private readonly FakePlanningDbContext _dbContext;
    private readonly CreatePokerSessionCommandHandler _handler;
    private readonly Mock<ILogger<CreatePokerSessionCommandHandler>> _mockLogger;
    private readonly Mock<ICurrentUser> _mockCurrentUser;
    private readonly Mock<IDateTimeProvider> _mockDateTimeProvider;

    private readonly EstimationScaleFaker _estimationScaleFaker;
    private readonly Guid _currentUserId = Guid.NewGuid();

    public CreatePokerSessionCommandHandlerTests()
    {
        _dbContext = new FakePlanningDbContext();
        _mockLogger = new Mock<ILogger<CreatePokerSessionCommandHandler>>();
        _mockCurrentUser = new Mock<ICurrentUser>();
        _mockCurrentUser.Setup(u => u.GetUserId()).Returns(_currentUserId);
        _mockDateTimeProvider = new Mock<IDateTimeProvider>();
        _mockDateTimeProvider.Setup(d => d.Now).Returns(Instant.FromUtc(2026, 1, 15, 10, 0));

        _handler = new CreatePokerSessionCommandHandler(_dbContext, _mockCurrentUser.Object, _mockDateTimeProvider.Object, _mockLogger.Object);
        _estimationScaleFaker = new EstimationScaleFaker();
    }

    [Fact]
    public async Task Handle_ShouldCreatePokerSession_WhenValidCommand()
    {
        // Arrange
        var scale = _estimationScaleFaker.WithId(1).Generate();
        _dbContext.AddEstimationScale(scale);

        var command = new CreatePokerSessionCommand("Sprint Planning", scale.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _dbContext.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenEstimationScaleDoesNotExist()
    {
        // Arrange
        var command = new CreatePokerSessionCommand("Sprint Planning", 999);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Estimation scale not found");
        _dbContext.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldSetFacilitatorToCurrentUser()
    {
        // Arrange
        var scale = _estimationScaleFaker.WithId(1).Generate();
        _dbContext.AddEstimationScale(scale);

        var command = new CreatePokerSessionCommand("Sprint Planning", scale.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockCurrentUser.Verify(u => u.GetUserId(), Times.Once);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
