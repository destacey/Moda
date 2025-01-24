﻿using FluentAssertions;
using Moda.Common.Models;
using Moda.StrategicManagement.Domain.Enums;
using Moda.StrategicManagement.Domain.Models;
using Moda.StrategicManagement.Domain.Tests.Data;
using Moda.Tests.Shared;
using NodaTime;
using NodaTime.Extensions;
using NodaTime.Testing;

namespace Moda.StrategicManagement.Domain.Tests.Sut.Models;

public class VisionTests
{
    private readonly TestingDateTimeProvider _dateTimeProvider;
    private readonly VisionFaker _faker;

    public VisionTests()
    {
        _dateTimeProvider = new(new FakeClock(DateTime.UtcNow.ToInstant()));
        _faker = new VisionFaker();
    }

    [Fact]
    public void Create_ShouldCreateVisionSuccessfully()
    {
        // Arrange
        var fakeVision = _faker.Generate();

        // Act
        var vision = Vision.Create(fakeVision.Description);

        // Assert
        vision.Should().NotBeNull();
        vision.Description.Should().Be(fakeVision.Description);
        vision.State.Should().Be(VisionState.Proposed);
        vision.Dates.Should().BeNull();
    }

    [Fact]
    public void Update_ShouldUpdateVisionSuccessfully_StateIsProposed()
    {
        // Arrange
        var fakeVision = _faker.Generate();
        var vision = Vision.Create(fakeVision.Description);
        var expectedState = vision.State;
        var expectedDates = vision.Dates;

        string updatedDescription = "Do amazing things.";

        // Act
        var result = vision.Update(updatedDescription);

        // Assert
        result.IsSuccess.Should().BeTrue();
        vision.Description.Should().Be(updatedDescription);
        vision.State.Should().Be(expectedState);
        vision.Dates.Should().Be(expectedDates);
    }

    [Fact]
    public void Update_ShouldUpdateVisionSuccessfully_WhenStateIsActive()
    {
        // Arrange
        var vision = _faker.WithData(
            state: VisionState.Active, 
            dates: new FlexibleInstantRange(_dateTimeProvider.Now)
            ).Generate();
        var expectedState = vision.State;
        var expectedDates = vision.Dates;

        string updatedDescription = "Do amazing things.";

        // Act
        var result = vision.Update(updatedDescription);

        // Assert
        result.IsSuccess.Should().BeTrue();
        vision.Description.Should().Be(updatedDescription);
        vision.State.Should().Be(expectedState);
        vision.Dates.Should().Be(expectedDates);
    }

    [Fact]
    public void Update_ShouldFail_WhenStateIsArchived()
    {
        // Arrange
        var vision = _faker.WithData(
            state: VisionState.Archived, 
            dates: new FlexibleInstantRange(_dateTimeProvider.Now.Plus(Duration.FromDays(-10)), _dateTimeProvider.Now)
            ).Generate();

        string updatedDescription = "Do amazing things.";

        // Act
        var result = vision.Update(updatedDescription);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("The vision is archived and cannot be updated.");
    }

    [Fact]
    public void Activate_ShouldActivateVisionSuccessfully()
    {
        // Arrange
        var fakeVision = _faker.Generate();
        var vision = Vision.Create(fakeVision.Description);
        var activationDate = _dateTimeProvider.Now;

        // Act
        var result = vision.Activate(activationDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        vision.State.Should().Be(VisionState.Active);
        vision.Dates.Should().NotBeNull();
        vision.Dates!.Start.Should().Be(activationDate);
        vision.Dates.End.Should().BeNull();
    }

    [Fact]
    public void Activate_ShouldFail_WhenVisionIsNotProposed()
    {
        // Arrange
        var fakeVision = _faker.Generate();
        var vision = Vision.Create(fakeVision.Description);
        var activationDate = _dateTimeProvider.Now;
        vision.Activate(activationDate);

        // Act
        var result = vision.Activate(activationDate);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("The vision must be in the Proposed state to activate it.");
    }

    [Fact]
    public void Archive_ShouldArchiveVisionSuccessfully()
    {
        // Arrange
        var fakeVision = _faker.Generate();
        var vision = Vision.Create(fakeVision.Description);
        var activationDate = _dateTimeProvider.Now.Plus(Duration.FromDays(-10));
        vision.Activate(activationDate);
        var archiveDate = activationDate.Plus(Duration.FromDays(5));

        // Act
        var result = vision.Archive(archiveDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        vision.State.Should().Be(VisionState.Archived);
        vision.Dates.Should().NotBeNull();
        vision.Dates!.Start.Should().Be(activationDate);
        vision.Dates.End.Should().Be(archiveDate);
    }

    [Fact]
    public void Archive_ShouldFail_WhenStateIsProposed()
    {
        // Arrange
        var fakeVision = _faker.Generate();
        var vision = Vision.Create(fakeVision.Description);
        var archiveDate = _dateTimeProvider.Now;

        // Act
        var result = vision.Archive(archiveDate);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("The vision must be in the Active state to archive it.");
    }

    [Fact]
    public void Archive_ShouldFail_WhenStateIsArchived()
    {
        // Arrange
        var vision = _faker.WithData(
            state: VisionState.Archived,
            dates: new FlexibleInstantRange(_dateTimeProvider.Now.Plus(Duration.FromDays(-10)), _dateTimeProvider.Now)
            ).Generate();
        var archiveDate = _dateTimeProvider.Now;

        // Act
        var result = vision.Archive(archiveDate);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("The vision must be in the Active state to archive it.");
    }

    [Fact]
    public void Archive_ShouldFail_WhenEndIsEarlierThanStart()
    {
        // Arrange
        var vision = _faker.WithData(
            state: VisionState.Active,
            dates: new FlexibleInstantRange(_dateTimeProvider.Now)
            ).Generate();
        var archiveDate = _dateTimeProvider.Now.Plus(Duration.FromDays(-5));

        // Act
        var result = vision.Archive(archiveDate);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("The end date must be on or after the start date.");
    }
}

