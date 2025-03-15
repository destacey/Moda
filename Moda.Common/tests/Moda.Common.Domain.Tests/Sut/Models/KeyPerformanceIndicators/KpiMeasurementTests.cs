using Moda.Common.Domain.Tests.Data;
using Moda.Common.Domain.Tests.Data.Models;
using Moda.Tests.Shared;
using NodaTime.Extensions;
using NodaTime.Testing;

namespace Moda.Common.Domain.Tests.Sut.Models.KeyPerformanceIndicators;

public sealed class KpiMeasurementTests
{
    private readonly TestingDateTimeProvider _dateTimeProvider;
    private readonly TestKpiMeasurementFaker _measurementFaker;

    public KpiMeasurementTests()
    {
        _dateTimeProvider = new TestingDateTimeProvider(new FakeClock(DateTime.UtcNow.ToInstant()));
        _measurementFaker = new TestKpiMeasurementFaker(_dateTimeProvider);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenValidData()
    {
        // Arrange
        var expected = _measurementFaker.Generate();

        // Act
        var measurement = TestKpiMeasurement.Create(expected.KpiId, expected.ActualValue, expected.MeasurementDate, expected.MeasuredById, expected.Note);

        // Assert
        measurement.KpiId.Should().Be(expected.KpiId);
        measurement.ActualValue.Should().Be(expected.ActualValue);
        measurement.MeasurementDate.Should().Be(expected.MeasurementDate);
        measurement.MeasuredById.Should().Be(expected.MeasuredById);
        measurement.Note.Should().Be(expected.Note);
    }
}
