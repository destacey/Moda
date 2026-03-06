using Microsoft.Extensions.Logging;
using Moda.Planning.Application.EstimationScales.Commands;
using Moda.Planning.Application.Tests.Infrastructure;
using Moda.Planning.Domain.Tests.Data;
using Moq;

namespace Moda.Planning.Application.Tests.EstimationScales.Commands;

public class DeleteEstimationScaleCommandHandlerTests : IDisposable
{
    private readonly FakePlanningDbContext _dbContext;
    private readonly DeleteEstimationScaleCommandHandler _handler;
    private readonly Mock<ILogger<DeleteEstimationScaleCommandHandler>> _mockLogger;

    public DeleteEstimationScaleCommandHandlerTests()
    {
        _dbContext = new FakePlanningDbContext();
        _mockLogger = new Mock<ILogger<DeleteEstimationScaleCommandHandler>>();

        _handler = new DeleteEstimationScaleCommandHandler(_dbContext, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeleteScale_WhenScaleExistsAndNotInUse()
    {
        // Arrange
        var scale = new EstimationScaleFaker().WithId(1).Generate();
        var otherScale = new EstimationScaleFaker().WithId(2).Generate();
        _dbContext.AddEstimationScale(scale);
        _dbContext.AddEstimationScale(otherScale);

        var command = new DeleteEstimationScaleCommand(scale.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _dbContext.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldDeleteInactiveScale_WhenActiveScalesExist()
    {
        // Arrange
        var inactiveScale = new EstimationScaleFaker().WithId(1).WithIsActive(false).Generate();
        var activeScale = new EstimationScaleFaker().WithId(2).Generate();
        _dbContext.AddEstimationScale(inactiveScale);
        _dbContext.AddEstimationScale(activeScale);

        var command = new DeleteEstimationScaleCommand(inactiveScale.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _dbContext.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenScaleDoesNotExist()
    {
        // Arrange
        var command = new DeleteEstimationScaleCommand(999);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        _dbContext.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenScaleIsInUseByPokerSession()
    {
        // Arrange
        var scale = new EstimationScaleFaker().WithId(1).Generate();
        var otherScale = new EstimationScaleFaker().WithId(2).Generate();
        _dbContext.AddEstimationScale(scale);
        _dbContext.AddEstimationScale(otherScale);

        var session = new PokerSessionFaker().WithEstimationScaleId(scale.Id).Generate();
        _dbContext.AddPokerSession(session);

        var command = new DeleteEstimationScaleCommand(scale.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("in use");
        _dbContext.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenDeletingLastActiveScale()
    {
        // Arrange
        var scale = new EstimationScaleFaker().WithId(1).Generate();
        var inactiveScale = new EstimationScaleFaker().WithId(2).WithIsActive(false).Generate();
        _dbContext.AddEstimationScale(scale);
        _dbContext.AddEstimationScale(inactiveScale);

        var command = new DeleteEstimationScaleCommand(scale.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("last active");
        _dbContext.SaveChangesCallCount.Should().Be(0);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
