using Microsoft.Extensions.Logging;
using Wayd.Planning.Application.EstimationScales.Commands;
using Wayd.Planning.Application.Tests.Infrastructure;
using Moq;

namespace Wayd.Planning.Application.Tests.Sut.EstimationScales.Commands;

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
            ["1", "2", "3", "5", "8"]);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _dbContext.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldCreateScale_WhenNoDescription()
    {
        // Arrange
        var command = new CreateEstimationScaleCommand(
            "Custom Scale",
            null,
            ["S", "M", "L"]);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

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
            ["1"]);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("at least 2 values");
        _dbContext.SaveChangesCallCount.Should().Be(0);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
