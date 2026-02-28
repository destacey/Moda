using Microsoft.Extensions.Logging;
using Moda.Planning.Application.EstimationScales.Commands;
using Moda.Planning.Application.Tests.Infrastructure;
using Moq;

namespace Moda.Planning.Application.Tests.EstimationScales.Commands;

public class CreateEstimationScaleCommandHandlerTests : IDisposable
{
    private readonly FakePlanningDbContext _dbContext;
    private readonly CreateEstimationScaleCommandHandler _handler;
    private readonly Mock<ILogger<CreateEstimationScaleCommandHandler>> _mockLogger;

    public CreateEstimationScaleCommandHandlerTests()
    {
        _dbContext = new FakePlanningDbContext();
        _mockLogger = new Mock<ILogger<CreateEstimationScaleCommandHandler>>();

        _handler = new CreateEstimationScaleCommandHandler(_dbContext, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateEstimationScale_WhenValidCommand()
    {
        // Arrange
        var command = new CreateEstimationScaleCommand(
            "Fibonacci",
            "Fibonacci sequence for estimation",
            [
                new CreateEstimationScaleCommand.ScaleValue("1", 0),
                new CreateEstimationScaleCommand.ScaleValue("2", 1),
                new CreateEstimationScaleCommand.ScaleValue("3", 2),
                new CreateEstimationScaleCommand.ScaleValue("5", 3),
                new CreateEstimationScaleCommand.ScaleValue("8", 4),
            ]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _dbContext.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldCreateScaleAsNonPreset()
    {
        // Arrange
        var command = new CreateEstimationScaleCommand(
            "Custom Scale",
            null,
            [
                new CreateEstimationScaleCommand.ScaleValue("S", 0),
                new CreateEstimationScaleCommand.ScaleValue("M", 1),
                new CreateEstimationScaleCommand.ScaleValue("L", 2),
            ]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _dbContext.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenLessThanTwoValues()
    {
        // Arrange
        var command = new CreateEstimationScaleCommand(
            "Bad Scale",
            null,
            [
                new CreateEstimationScaleCommand.ScaleValue("1", 0),
            ]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("at least 2 values");
        _dbContext.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenDuplicateOrderValues()
    {
        // Arrange
        var command = new CreateEstimationScaleCommand(
            "Bad Scale",
            null,
            [
                new CreateEstimationScaleCommand.ScaleValue("1", 0),
                new CreateEstimationScaleCommand.ScaleValue("2", 0),
            ]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("unique order");
        _dbContext.SaveChangesCallCount.Should().Be(0);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
