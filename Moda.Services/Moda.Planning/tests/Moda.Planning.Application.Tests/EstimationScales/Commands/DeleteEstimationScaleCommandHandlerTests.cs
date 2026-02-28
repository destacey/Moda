using Microsoft.Extensions.Logging;
using Moda.Planning.Application.EstimationScales.Commands;
using Moda.Planning.Application.Tests.Infrastructure;
using Moda.Planning.Domain.Models.PlanningPoker;
using Moda.Planning.Domain.Tests.Data;
using Moq;

namespace Moda.Planning.Application.Tests.EstimationScales.Commands;

public class DeleteEstimationScaleCommandHandlerTests : IDisposable
{
    private readonly FakePlanningDbContext _dbContext;
    private readonly DeleteEstimationScaleCommandHandler _handler;
    private readonly Mock<ILogger<DeleteEstimationScaleCommandHandler>> _mockLogger;

    private readonly EstimationScaleFaker _scaleFaker;
    private readonly PokerSessionFaker _sessionFaker;

    public DeleteEstimationScaleCommandHandlerTests()
    {
        _dbContext = new FakePlanningDbContext();
        _mockLogger = new Mock<ILogger<DeleteEstimationScaleCommandHandler>>();

        _handler = new DeleteEstimationScaleCommandHandler(_dbContext, _mockLogger.Object);
        _scaleFaker = new EstimationScaleFaker();
        _sessionFaker = new PokerSessionFaker();
    }

    [Fact]
    public async Task Handle_ShouldDeleteScale_WhenScaleExistsAndNotInUse()
    {
        // Arrange
        var scale = _scaleFaker.WithId(1).Generate();
        _dbContext.AddEstimationScale(scale);

        var command = new DeleteEstimationScaleCommand(scale.Id);

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
    public async Task Handle_ShouldFail_WhenScaleIsPreset()
    {
        // Arrange
        var scale = _scaleFaker.WithId(1).AsPreset().Generate();
        _dbContext.AddEstimationScale(scale);

        var command = new DeleteEstimationScaleCommand(scale.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Preset");
        _dbContext.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenScaleIsInUseByPokerSession()
    {
        // Arrange
        var scale = _scaleFaker.WithId(1).Generate();
        _dbContext.AddEstimationScale(scale);

        var session = _sessionFaker.WithEstimationScaleId(scale.Id).Generate();
        _dbContext.AddPokerSession(session);

        var command = new DeleteEstimationScaleCommand(scale.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("in use");
        _dbContext.SaveChangesCallCount.Should().Be(0);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
