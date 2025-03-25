using Moda.Common.Domain.Tests.Data;
using Moda.Common.Domain.Tests.Data.Models;
using Moda.Tests.Shared;
using NodaTime.Extensions;
using NodaTime.Testing;

namespace Moda.Common.Domain.Tests.Sut.Models.KeyPerformanceIndicators;

public sealed class KpiCheckpointTests
{
    private readonly TestingDateTimeProvider _dateTimeProvider;
    private readonly TestKpiCheckpointFaker _checkpointFaker;

    public KpiCheckpointTests()
    {
        _dateTimeProvider = new TestingDateTimeProvider(new FakeClock(DateTime.UtcNow.ToInstant()));
        _checkpointFaker = new TestKpiCheckpointFaker(_dateTimeProvider);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenValidData()
    {
        // Arrange
        var expected = _checkpointFaker.Generate();

        // Act
        var checkpoint = TestKpiCheckpoint.Create(expected.KpiId, expected.TargetValue, expected.CheckpointDate);

        // Assert
        checkpoint.KpiId.Should().Be(expected.KpiId);
        checkpoint.TargetValue.Should().Be(expected.TargetValue);
        checkpoint.CheckpointDate.Should().Be(expected.CheckpointDate);
    }

    [Fact]
    public void Update_ShouldReturnSuccess_WhenValidData()
    {
        // Arrange
        var expected = _checkpointFaker.Generate();
        var checkpoint = TestKpiCheckpoint.Create(expected.KpiId, expected.TargetValue, expected.CheckpointDate);

        // Act
        var result = checkpoint.Update(expected.TargetValue, expected.CheckpointDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        checkpoint.TargetValue.Should().Be(expected.TargetValue);
        checkpoint.CheckpointDate.Should().Be(expected.CheckpointDate);
    }
}
