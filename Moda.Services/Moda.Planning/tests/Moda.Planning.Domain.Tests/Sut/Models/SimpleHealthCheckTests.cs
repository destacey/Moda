using Moda.Common.Domain.Enums;
using Moda.Planning.Domain.Models;
using Moda.Planning.Domain.Tests.Data;
using Moda.Tests.Shared;
using NodaTime.Extensions;
using NodaTime.Testing;

namespace Moda.Planning.Domain.Tests.Sut.Models;
public class SimpleSimpleHealthCheckTests
{
    private readonly TestingDateTimeService _dateTimeService;

    private readonly SimpleHealthCheckFaker _healthCheckFaker;

    public SimpleSimpleHealthCheckTests()
    {
        // TODO: Replace with a FakeClock that can be injected into the constructor.
        _dateTimeService = new(new FakeClock(DateTime.UtcNow.ToInstant()));
        _healthCheckFaker = new(_dateTimeService.Now);
    }

    #region Constructor

    [Fact]
    public void Constructor_WhenValid_ReturnsSimpleHealthCheck()
    {
        // Arrange
        var faker = _healthCheckFaker.UsePrivateConstructor().Generate();

        // Act
        var result = new SimpleHealthCheck(faker.ObjectId, faker.Id, faker.Status, faker.ReportedOn, faker.Expiration);

        // Assert
        result.Should().NotBeNull();
        result.ObjectId.Should().Be(faker.ObjectId);
        result.Status.Should().Be(faker.Status);
        result.ReportedOn.Should().Be(faker.ReportedOn);
        result.Expiration.Should().Be(faker.Expiration);

        result.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ObjectIdIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var objectId = Guid.Empty;
        var faker = _healthCheckFaker.UsePrivateConstructor().Generate();

        // Act
        Action act = () => new SimpleHealthCheck(objectId, faker.Id, faker.Status, faker.ReportedOn, faker.Expiration);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Parameter [objectId] is default value for type Guid (Parameter 'objectId')");
    }

    #endregion Constructor

    #region IsExpired

    [Fact]
    public void IsExpired_WhenExpirationGreaterThanNow_ReturnsFalse()
    {
        // Arrange
        var faker = _healthCheckFaker.UsePrivateConstructor().Generate();

        // Act
        var sut = new SimpleHealthCheck(faker.ObjectId, faker.Id, faker.Status, faker.ReportedOn, _dateTimeService.Now.Plus(Duration.FromDays(2)));
        var result = sut.IsExpired(_dateTimeService.Now);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WhenExpirationLessThanNow_ReturnsTrue()
    {
        // Arrange
        var sut = _healthCheckFaker.UsePrivateConstructor().Generate();
        _dateTimeService.Advance(Duration.FromDays(60));

        // Act
        var result = sut.IsExpired(_dateTimeService.Now);

        // Assert
        result.Should().BeTrue();
    }

    #endregion IsExpired

    #region Update

    [Fact]
    public void Update_WhenUpdatingValid_UpdatesSimpleHealthCheck()
    {
        // Arrange
        var sut = _healthCheckFaker.UsePrivateConstructor().Generate();
        var id = sut.Id;
        var status = sut.Status;
        var reportedOn = sut.ReportedOn;
        var expiration = sut.Expiration.Plus(Duration.FromDays(2));

        // Act
        var result = sut.Update(id, status, reportedOn, expiration);

        // Assert
        result.IsSuccess.Should().BeTrue();
        sut.Id.Should().Be(id);
        sut.Status.Should().Be(status);
        sut.ReportedOn.Should().Be(reportedOn);
        sut.Expiration.Should().Be(expiration);

        sut.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Update_WhenReplacingValid_UpdatesSimpleHealthCheck()
    {
        // Arrange
        var sut = _healthCheckFaker.UsePrivateConstructor().Generate();
        var id = Guid.NewGuid();
        var status = HealthStatus.AtRisk;
        var reportedOn = sut.ReportedOn.Plus(Duration.FromDays(2));
        var expiration = reportedOn.Plus(Duration.FromDays(8));

        // Act
        var result = sut.Update(id, status, reportedOn, expiration);

        // Assert
        result.IsSuccess.Should().BeTrue();
        sut.Id.Should().Be(id);
        sut.Status.Should().Be(status);
        sut.ReportedOn.Should().Be(reportedOn);
        sut.Expiration.Should().Be(expiration);

        sut.DomainEvents.Should().BeEmpty();
    }

    #endregion Update
}
