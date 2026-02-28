using Microsoft.Extensions.Logging;
using Moda.Planning.Application.EstimationScales.Commands;
using Moda.Planning.Application.Tests.Infrastructure;
using Moda.Planning.Domain.Models.PlanningPoker;
using Moda.Planning.Domain.Tests.Data;
using Moq;

namespace Moda.Planning.Application.Tests.EstimationScales.Commands;

public class UpdateEstimationScaleCommandHandlerTests : IDisposable
{
    private readonly FakePlanningDbContext _dbContext;
    private readonly UpdateEstimationScaleCommandHandler _handler;
    private readonly Mock<ILogger<UpdateEstimationScaleCommandHandler>> _mockLogger;

    private readonly EstimationScaleFaker _scaleFaker;

    public UpdateEstimationScaleCommandHandlerTests()
    {
        _dbContext = new FakePlanningDbContext();
        _mockLogger = new Mock<ILogger<UpdateEstimationScaleCommandHandler>>();

        _handler = new UpdateEstimationScaleCommandHandler(_dbContext, _mockLogger.Object);
        _scaleFaker = new EstimationScaleFaker();
    }

    [Fact]
    public async Task Handle_ShouldUpdateScale_WhenValidCommand()
    {
        // Arrange
        var scaleResult = EstimationScale.Create(
            "Original Name",
            "Original Description",
            isPreset: false,
            [("1", 0), ("2", 1), ("3", 2)]);
        var scale = scaleResult.Value;
        _dbContext.AddEstimationScale(scale);

        var command = new UpdateEstimationScaleCommand(
            scale.Id,
            "Updated Name",
            "Updated Description",
            [
                new UpdateEstimationScaleCommand.ScaleValue("S", 0),
                new UpdateEstimationScaleCommand.ScaleValue("M", 1),
                new UpdateEstimationScaleCommand.ScaleValue("L", 2),
                new UpdateEstimationScaleCommand.ScaleValue("XL", 3),
            ]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        scale.Name.Should().Be("Updated Name");
        scale.Description.Should().Be("Updated Description");
        scale.Values.Should().HaveCount(4);
        _dbContext.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenScaleDoesNotExist()
    {
        // Arrange
        var command = new UpdateEstimationScaleCommand(
            999,
            "Name",
            null,
            [
                new UpdateEstimationScaleCommand.ScaleValue("1", 0),
                new UpdateEstimationScaleCommand.ScaleValue("2", 1),
            ]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        _dbContext.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenScaleIsPreset()
    {
        // Arrange
        var scaleResult = EstimationScale.CreatePreset(
            "Fibonacci",
            "Built-in preset",
            [("1", 0), ("2", 1), ("3", 2), ("5", 3)]);
        var scale = scaleResult.Value;
        _dbContext.AddEstimationScale(scale);

        var command = new UpdateEstimationScaleCommand(
            scale.Id,
            "Modified Fibonacci",
            null,
            [
                new UpdateEstimationScaleCommand.ScaleValue("1", 0),
                new UpdateEstimationScaleCommand.ScaleValue("2", 1),
            ]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Preset");
        _dbContext.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenLessThanTwoValues()
    {
        // Arrange
        var scaleResult = EstimationScale.Create(
            "My Scale",
            null,
            isPreset: false,
            [("1", 0), ("2", 1)]);
        var scale = scaleResult.Value;
        _dbContext.AddEstimationScale(scale);

        var command = new UpdateEstimationScaleCommand(
            scale.Id,
            "My Scale",
            null,
            [
                new UpdateEstimationScaleCommand.ScaleValue("1", 0),
            ]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

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
