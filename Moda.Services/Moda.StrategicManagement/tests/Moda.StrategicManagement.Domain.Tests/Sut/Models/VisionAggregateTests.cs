﻿using Bogus;
using FluentAssertions;
using Moda.Common.Models;
using Moda.StrategicManagement.Domain.Enums;
using Moda.StrategicManagement.Domain.Models;
using Moda.StrategicManagement.Domain.Tests.Data;
using Moda.Tests.Shared;
using NodaTime;
using NodaTime.Extensions;
using NodaTime.Testing;

namespace Moda.StrategicManagement.Domain.Tests.Sut.Models;
public sealed class VisionAggregateTests
{
    private readonly TestingDateTimeProvider _dateTimeProvider;
    private readonly VisionFaker _faker;

    public VisionAggregateTests()
    {
        _dateTimeProvider = new(new FakeClock(DateTime.UtcNow.ToInstant()));
        _faker = new VisionFaker();
    }

    #region Activate

    [Fact]
    public void Activate_ShouldActivateSuccessfully_WhenNoActiveVisionExists()
    {
        // Arrange
        var proposedVision = _faker.Generate();
        var visionAggregate = new VisionAggregate([proposedVision]);

        // Act
        var result = visionAggregate.Activate(proposedVision.Id, _dateTimeProvider.Now);

        // Assert
        result.IsSuccess.Should().BeTrue();
        proposedVision.State.Should().Be(VisionState.Active);
        proposedVision.Dates.Should().NotBeNull();
        proposedVision.Dates!.Start.Should().Be(_dateTimeProvider.Now);
        proposedVision.Dates.End.Should().BeNull();
    }

    [Fact]
    public void Activate_ShouldFail_WhenNoMatchingVisionExists()
    {
        // Arrange
        var proposedVisions = _faker.Generate(5);
        var visionAggregate = new VisionAggregate([.. proposedVisions]);

        // Act
        var result = visionAggregate.Activate(Guid.NewGuid(), _dateTimeProvider.Now);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Vision not found.");
    }

    [Fact]
    public void Activate_ShouldFail_WhenAnotherVisionIsActive()
    {
        // Arrange
        var activeVision = _faker.ActiveVision(_dateTimeProvider);

        var proposedVision = _faker.Generate();
        var visionAggregate = new VisionAggregate([activeVision, proposedVision]);

        // Act
        var result = visionAggregate.Activate(proposedVision.Id, _dateTimeProvider.Now);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("An active vision already exists. Only one active vision is allowed.");
    }

    [Fact]
    public void Activate_ShouldFail_WhenOverlappingWithArchivedVision()
    {
        // Arrange
        var archivedVision = _faker.ArchivedVision(_dateTimeProvider);

        var proposedVision = _faker.Generate();
        var visionAggregate = new VisionAggregate([archivedVision, proposedVision]);

        var overlappingDate = _dateTimeProvider.Now.Plus(Duration.FromDays(-15));

        // Act
        var result = visionAggregate.Activate(proposedVision.Id, overlappingDate);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("The vision cannot be activated because it overlaps with another archived vision.");
    }

    #endregion Activate

    #region Archive

    [Fact]
    public void Archive_ShouldArchiveSuccessfully_WhenNoExistingArchived()
    {
        // Arrange
        var activeVision = _faker.ActiveVision(_dateTimeProvider);
        var expectedActiveDate = activeVision.Dates!.Start;

        var visionAggregate = new VisionAggregate([activeVision]);

        var archiveDate = _dateTimeProvider.Now;

        // Act
        var result = visionAggregate.Archive(activeVision.Id, archiveDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        activeVision.State.Should().Be(VisionState.Archived);
        activeVision.Dates.Should().NotBeNull();
        activeVision.Dates!.Start.Should().Be(expectedActiveDate);
        activeVision.Dates.End.Should().Be(archiveDate);
    }

    [Fact]
    public void Archive_ShouldArchiveSuccessfully_WhenAfterExistingArchived()
    {
        // Arrange
        var activeVision = _faker.ActiveVision(_dateTimeProvider);
        var expectedActiveDate = activeVision.Dates!.Start;

        var archivedVision = _faker.ArchivedVision(_dateTimeProvider);

        var visionAggregate = new VisionAggregate([activeVision, archivedVision]);

        var archiveDate = _dateTimeProvider.Now;

        // Act
        var result = visionAggregate.Archive(activeVision.Id, archiveDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        activeVision.State.Should().Be(VisionState.Archived);
        activeVision.Dates.Should().NotBeNull();
        activeVision.Dates!.Start.Should().Be(expectedActiveDate);
        activeVision.Dates.End.Should().Be(archiveDate);
    }

    [Fact]
    public void Archive_ShouldArchiveSuccessfully_WhenEarlierThanExistingArchived()
    {
        // Arrange
        var activeVision = _faker.WithData(
            state: VisionState.Active,
            dates: new FlexibleInstantRange(_dateTimeProvider.Now.Plus(Duration.FromDays(-50)))
            ).Generate();
        var expectedActiveDate = activeVision.Dates!.Start;

        var archivedVision = _faker.ArchivedVision(_dateTimeProvider);

        var visionAggregate = new VisionAggregate([activeVision, archivedVision]);

        var archiveDate = expectedActiveDate.Plus(Duration.FromDays(10));

        // Act
        var result = visionAggregate.Archive(activeVision.Id, archiveDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        activeVision.State.Should().Be(VisionState.Archived);
        activeVision.Dates.Should().NotBeNull();
        activeVision.Dates!.Start.Should().Be(expectedActiveDate);
        activeVision.Dates.End.Should().Be(archiveDate);
    }

    [Fact]
    public void Archive_ShouldFail_WhenNoMatchingVisionExists()
    {
        // Arrange
        var activeVision = _faker.ActiveVision(_dateTimeProvider);
        var visionAggregate = new VisionAggregate([activeVision]);

        // Act
        var result = visionAggregate.Archive(Guid.NewGuid(), _dateTimeProvider.Now);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Vision not found.");
    }

    [Fact]
    public void Archive_ShouldFail_WhenVisionIsNotActive()
    {
        // Arrange
        var archivedVision = _faker.ArchivedVision(_dateTimeProvider);

        var visionAggregate = new VisionAggregate([archivedVision]);

        var archiveDate = archivedVision.Dates!.Start.Plus(Duration.FromDays(5));

        // Act
        var result = visionAggregate.Archive(archivedVision.Id, archiveDate);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("The vision must be in the Active state to archive it.");
    }

    [Fact]
    public void Archive_ShouldFail_WhenOverlappingWithAnotherArchivedVision()
    {
        // Arrange
        var archivedVision = _faker.ArchivedVision(_dateTimeProvider);

        var activeVision = _faker.WithData(
            state: VisionState.Active,
            dates: new FlexibleInstantRange(_dateTimeProvider.Now.Plus(Duration.FromDays(-15)))
        ).Generate();

        var visionAggregate = new VisionAggregate([archivedVision, activeVision]);

        var archiveDate = activeVision.Dates!.Start.Plus(Duration.FromDays(10));

        // Act
        var result = visionAggregate.Archive(activeVision.Id, archiveDate);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("The vision cannot be archived because it overlaps with another archived vision.");
    }

    #endregion Archive
}