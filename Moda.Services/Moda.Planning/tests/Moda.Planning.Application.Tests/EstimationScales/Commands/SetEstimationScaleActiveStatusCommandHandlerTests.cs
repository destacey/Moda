using Microsoft.Extensions.Logging;
using Moda.Planning.Application.EstimationScales.Commands;
using Moda.Planning.Application.Tests.Infrastructure;
using Moda.Planning.Domain.Tests.Data;
using Moq;

namespace Moda.Planning.Application.Tests.EstimationScales.Commands;

public class SetEstimationScaleActiveStatusCommandHandlerTests : IDisposable
{
    private readonly FakePlanningDbContext _dbContext;
    private readonly SetEstimationScaleActiveStatusCommandHandler _handler;
    private readonly Mock<ILogger<SetEstimationScaleActiveStatusCommandHandler>> _mockLogger;

    public SetEstimationScaleActiveStatusCommandHandlerTests()
    {
        _dbContext = new FakePlanningDbContext();
        _mockLogger = new Mock<ILogger<SetEstimationScaleActiveStatusCommandHandler>>();

        _handler = new SetEstimationScaleActiveStatusCommandHandler(_dbContext, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeactivate_WhenOtherActiveScalesExist()
    {
        // Arrange
        var scale = new EstimationScaleFaker().WithId(1).Generate();
        var otherScale = new EstimationScaleFaker().WithId(2).Generate();
        _dbContext.AddEstimationScale(scale);
        _dbContext.AddEstimationScale(otherScale);

        var command = new SetEstimationScaleActiveStatusCommand(scale.Id, false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        scale.IsActive.Should().BeFalse();
        _dbContext.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldActivate_WhenScaleIsInactive()
    {
        // Arrange
        var inactiveScale = new EstimationScaleFaker().WithId(1).WithIsActive(false).Generate();
        _dbContext.AddEstimationScale(inactiveScale);

        var command = new SetEstimationScaleActiveStatusCommand(inactiveScale.Id, true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        inactiveScale.IsActive.Should().BeTrue();
        _dbContext.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenScaleDoesNotExist()
    {
        // Arrange
        var command = new SetEstimationScaleActiveStatusCommand(999, false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        _dbContext.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenDeactivatingLastActiveScale()
    {
        // Arrange
        var scale = new EstimationScaleFaker().WithId(1).Generate();
        var inactiveScale = new EstimationScaleFaker().WithId(2).WithIsActive(false).Generate();
        _dbContext.AddEstimationScale(scale);
        _dbContext.AddEstimationScale(inactiveScale);

        var command = new SetEstimationScaleActiveStatusCommand(scale.Id, false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("last active");
        scale.IsActive.Should().BeTrue();
        _dbContext.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenAlreadyActive()
    {
        // Arrange
        var scale = new EstimationScaleFaker().WithId(1).Generate();
        _dbContext.AddEstimationScale(scale);

        var command = new SetEstimationScaleActiveStatusCommand(scale.Id, true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("already active");
        _dbContext.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenAlreadyInactive()
    {
        // Arrange
        var inactiveScale = new EstimationScaleFaker().WithId(1).WithIsActive(false).Generate();
        var otherScale = new EstimationScaleFaker().WithId(2).Generate();
        _dbContext.AddEstimationScale(inactiveScale);
        _dbContext.AddEstimationScale(otherScale);

        var command = new SetEstimationScaleActiveStatusCommand(inactiveScale.Id, false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("already inactive");
        _dbContext.SaveChangesCallCount.Should().Be(0);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
