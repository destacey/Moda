using Moda.Common.Domain.Events;
using Moda.Health.Tests.Data;
using Moda.Tests.Shared;
using NodaTime.Extensions;
using NodaTime.Testing;

namespace Moda.Health.Tests.Sut.Models;
public class HealthCheckTests
{
    private readonly TestingDateTimeProvider _dateTimeProvider;

    private readonly HealthCheckFaker _healthCheckFaker;

    public HealthCheckTests()
    {
        // TODO: Replace with a FakeClock that can be injected into the constructor.
        _dateTimeProvider = new(new FakeClock(DateTime.UtcNow.ToInstant()));
        _healthCheckFaker = new(_dateTimeProvider.Now);
    }

    #region Constructor

    [Fact]
    public void Constructor_WhenValid_ReturnsHealthCheck()
    {
        // Arrange
        var faker = _healthCheckFaker.Generate();

        // Act
        var result = new HealthCheck(faker.ObjectId, faker.Context, faker.Status, faker.ReportedById, faker.ReportedOn, faker.Expiration, faker.Note);

        // Assert
        result.Should().NotBeNull();
        result.ObjectId.Should().Be(faker.ObjectId);
        result.Context.Should().Be(faker.Context);
        result.Status.Should().Be(faker.Status);
        result.ReportedById.Should().Be(faker.ReportedById);
        result.ReportedOn.Should().Be(faker.ReportedOn);
        result.Expiration.Should().Be(faker.Expiration);
        result.Note.Should().Be(faker.Note);

        result.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ObjectIdIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var objectId = Guid.Empty;
        var faker = _healthCheckFaker.Generate();

        // Act
        Action act = () => new HealthCheck(objectId, faker.Context, faker.Status, faker.ReportedById, faker.ReportedOn, faker.Expiration, faker.Note);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Parameter [objectId] is default value for type Guid (Parameter 'objectId')");
    }

    [Fact]
    public void Constructor_WhenExpirationLessThanTimestamp_ThrowsArgumentException()
    {
        // Arrange
        var timestamp = Instant.FromDateTimeUtc(DateTime.UtcNow);
        var expiration = timestamp.Minus(Duration.FromDays(1));
        var faker = _healthCheckFaker.Generate();

        // Act
        Action act = () => new HealthCheck(faker.ObjectId, faker.Context, faker.Status, faker.ReportedById, timestamp, expiration, faker.Note);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Expiration must be greater than timestamp. (Parameter 'expiration')");
    }

    #endregion Constructor

    #region ChangeExpiration

    [Fact]
    public void ChangeExpiration_WhenValid_UpdatesExpiration()
    {
        // Arrange
        var sut = _healthCheckFaker.Generate();
        var expiration = sut.ReportedOn.Plus(Duration.FromDays(1));

        // Act
        sut.ChangeExpiration(expiration);

        // Assert
        sut.Expiration.Should().Be(expiration);
    }

    [Fact]
    public void ChangeExpiration_WhenExpirationLessThanTimestamp_ThrowsArgumentException()
    {
        // Arrange
        var sut = _healthCheckFaker.Generate();
        var expiration = sut.ReportedOn.Minus(Duration.FromDays(1));

        // Act
        Action act = () => sut.ChangeExpiration(expiration);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Expiration must be greater than timestamp. (Parameter 'Expiration')");
    }

    #endregion ChangeExpiration

    #region IsExpired

    [Fact]
    public void IsExpired_WhenExpirationGreaterThanNow_ReturnsFalse()
    {
        // Arrange
        var sut = _healthCheckFaker.Generate();
        var expiration = sut.ReportedOn.Plus(Duration.FromDays(1));
        sut.ChangeExpiration(expiration);

        // Act
        var result = sut.IsExpired(_dateTimeProvider.Now);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WhenExpirationLessThanNow_ReturnsTrue()
    {
        // Arrange
        var sut = _healthCheckFaker.Generate();
        _dateTimeProvider.Advance(Duration.FromDays(60));

        // Act
        var result = sut.IsExpired(_dateTimeProvider.Now);

        // Assert
        result.Should().BeTrue();
    }

    #endregion IsExpired

    #region Update

    [Fact]
    public void Update_WhenValid_UpdatesHealthCheck()
    {
        // Arrange
        var sut = _healthCheckFaker.Generate();
        var status = HealthStatus.Healthy;
        var expiration = sut.ReportedOn.Plus(Duration.FromDays(1));
        var note = "Updated Note";

        // Act
        var result = sut.Update(status, expiration, note, _dateTimeProvider.Now);

        // Assert
        result.IsSuccess.Should().BeTrue();
        sut.Status.Should().Be(status);
        sut.Expiration.Should().Be(expiration);
        sut.Note.Should().Be(note);

        sut.DomainEvents.Should().NotBeEmpty();
        sut.DomainEvents.Should().ContainSingle(e => e is EntityUpdatedEvent<HealthCheck>);
    }

    [Fact]
    public void Update_WhenNoteIsWhiteSpace_UpdatesHealthCheck()
    {
        // Arrange
        var sut = _healthCheckFaker.Generate();
        var status = HealthStatus.Healthy;
        var expiration = sut.ReportedOn.Plus(Duration.FromDays(1));
        var note = " ";

        // Act
        var result = sut.Update(status, expiration, note, _dateTimeProvider.Now);

        // Assert
        result.IsSuccess.Should().BeTrue();
        sut.Status.Should().Be(status);
        sut.Expiration.Should().Be(expiration);
        sut.Note.Should().BeNull();

        sut.DomainEvents.Should().NotBeEmpty();
        sut.DomainEvents.Should().ContainSingle(e => e is EntityUpdatedEvent<HealthCheck>);
    }

    [Fact]
    public void Update_WhenExpirationLessThanTimestamp_ThrowsArgumentException()
    {
        // Arrange
        var sut = _healthCheckFaker.Generate();
        var status = HealthStatus.Healthy;
        var expiration = sut.ReportedOn.Minus(Duration.FromDays(1));
        var note = "Updated Note";

        // Act
        var result = sut.Update(status, expiration, note, _dateTimeProvider.Now);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Expiration must be greater than timestamp. (Parameter 'Expiration')");
    }

    [Fact]
    public void Update_WhenExpired_FailsWithError()
    {
        // Arrange
        var faker = _healthCheckFaker.Generate();
        var sut = new HealthCheck(faker.ObjectId, faker.Context, faker.Status, faker.ReportedById, faker.ReportedOn.Minus(Duration.FromDays(15)), faker.ReportedOn.Minus(Duration.FromDays(10)), faker.Note);
        var status = HealthStatus.Healthy;
        var expiration = _dateTimeProvider.Now.Plus(Duration.FromDays(1));
        var note = "Updated Note";

        // Act
        var result = sut.Update(status, expiration, note, _dateTimeProvider.Now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Expired health checks cannot be modified.");
        sut.Note.Should().NotBe(note);
    }

    #endregion Update

}
