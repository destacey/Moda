﻿using Moda.Common.Models;
using Moda.Organization.Domain.Enums;
using Moda.Planning.Domain.Tests.Data;
using Moda.Tests.Shared;
using NodaTime.Extensions;
using NodaTime.Testing;

namespace Moda.Planning.Domain.Tests.Sut.Models;
public class PlanningIntervalTests
{
    private readonly TestingDateTimeService _dateTimeService;
    private readonly PlanningIntervalFaker _planningIntervalFaker = new();

    public PlanningIntervalTests()
    {
        _dateTimeService = new(new FakeClock(DateTime.UtcNow.ToInstant()));
    }

    #region CalculatePredictability

    [Fact]
    public void CalculatePredictability_WhenStartedAndNoObjectives_ReturnsNull()
    {
        // Arrange
        var sut = _planningIntervalFaker.UsePrivateConstructor().Generate();

        // Act
        var result = sut.CalculatePredictability(_dateTimeService.Today);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CalculatePredictability_WhenFuture_ReturnsNull()
    {
        // Arrange
        var sut = _planningIntervalFaker.UsePrivateConstructor().Generate();
        sut.Update(sut.Name, sut.Description, false);

        // Act
        var result = sut.CalculatePredictability(_dateTimeService.Today);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CalculatePredictability_WhenNoCompletedObjectives_Returns0()
    {
        // Arrange
        var team = new PlanningTeamFaker(TeamType.Team).UsePrivateConstructor().Generate();
        var sut = _planningIntervalFaker.WithObjectives(team, 2).UsePrivateConstructor().Generate();

        // Act
        var result = sut.CalculatePredictability(_dateTimeService.Today);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void CalculatePredictability_WhenHalfOfObjectivesCompleted_Returns50()
    {
        // Arrange
        var team = new PlanningTeamFaker(TeamType.Team).UsePrivateConstructor().Generate();
        var sut = _planningIntervalFaker.WithObjectives(team, 6).UsePrivateConstructor().Generate();

        var objectiveIds = sut.Objectives.Select(o => o.Id).ToArray();
        for (int i = 0; i < 3; i++)
        {
            sut.UpdateObjective(objectiveIds[i], Enums.ObjectiveStatus.Completed, false);
        }

        // Act
        var result = sut.CalculatePredictability(_dateTimeService.Today);

        // Assert
        result.Should().Be(50);
    }

    [Fact]
    public void CalculatePredictability_WithStretchAndWhenAllCompleted_Returns100()
    {
        // Arrange
        var objectiveCount = 6;
        var team = new PlanningTeamFaker(TeamType.Team).UsePrivateConstructor().Generate();
        var sut = new PlanningIntervalFaker().WithObjectives(team, objectiveCount).UsePrivateConstructor().Generate();

        var objectiveIds = sut.Objectives.Select(o => o.Id).ToArray();
        for (int i = 0; i < objectiveCount; i++)
        {
            var isStretch = i >= objectiveCount - 2;
            sut.UpdateObjective(objectiveIds[i], Enums.ObjectiveStatus.Completed, isStretch);
        }

        // Act
        var result = sut.CalculatePredictability(_dateTimeService.Today);

        // Assert
        result.Should().Be(100);
    }

    [Fact]
    public void CalculatePredictability_WhenAllButOneNonStretchCompleted_Returns100()
    {
        // Arrange
        var objectiveCount = 6;
        var team = new PlanningTeamFaker(TeamType.Team).UsePrivateConstructor().Generate();
        var sut = _planningIntervalFaker.WithObjectives(team, objectiveCount).UsePrivateConstructor().Generate();

        var objectiveIds = sut.Objectives.Select(o => o.Id).ToArray();
        for (int i = 1; i < objectiveCount; i++) // skip the first one so it is still open
        {
            var isStretch = i >= objectiveCount - 2;
            sut.UpdateObjective(objectiveIds[i], Enums.ObjectiveStatus.Completed, isStretch);
        }

        // Act
        var result = sut.CalculatePredictability(_dateTimeService.Today);

        // Assert
        result.Should().Be(100);
    }

    [Fact]
    public void CalculatePredictability_WhenAllCompletedExceptStretch_ReturnsOneHundred()
    {
        // Arrange
        var objectiveCount = 6;
        var team = new PlanningTeamFaker(TeamType.Team).UsePrivateConstructor().Generate();
        var sut = _planningIntervalFaker.WithObjectives(team, objectiveCount).UsePrivateConstructor().Generate();

        var objectiveIds = sut.Objectives.Select(o => o.Id).ToArray();
        for (int i = 0; i < objectiveCount; i++)
        {
            var isStretch = i >= objectiveCount - 2;
            var status = isStretch ? Enums.ObjectiveStatus.InProgress : Enums.ObjectiveStatus.Completed;
            sut.UpdateObjective(objectiveIds[i], status, isStretch);
        }

        // Act
        var result = sut.CalculatePredictability(_dateTimeService.Today);

        // Assert
        result.Should().Be(100);
    }

    [Fact]
    public void CalculatePredictability_WhenThreeNonStretchComplete_Returns75()
    {
        // Arrange
        var objectiveCount = 6;
        var team = new PlanningTeamFaker(TeamType.Team).UsePrivateConstructor().Generate();
        var sut = _planningIntervalFaker.WithObjectives(team, objectiveCount).UsePrivateConstructor().Generate();

        var objectiveIds = sut.Objectives.Select(o => o.Id).ToArray();
        for (int i = 0; i < objectiveCount; i++)
        {
            var isStretch = i >= objectiveCount - 2;
            var isComplete = i < 3;
            var status = isComplete ? Enums.ObjectiveStatus.Completed : Enums.ObjectiveStatus.InProgress;
            sut.UpdateObjective(objectiveIds[i], status, isStretch);
        }

        // Act
        var result = sut.CalculatePredictability(_dateTimeService.Today);

        // Assert
        // 3/4 of the non-stretch objectives are complete and 0/2 of the stretch objectives are complete, so 75% predictability
        result.Should().Be(75);
    }

    [Fact]
    public void CalculatePredictability_WhenOnlyStretchComplete_Returns50()
    {
        // Arrange
        var objectiveCount = 6;
        var team = new PlanningTeamFaker(TeamType.Team).UsePrivateConstructor().Generate();
        var sut = _planningIntervalFaker.WithObjectives(team, objectiveCount).UsePrivateConstructor().Generate();

        var objectiveIds = sut.Objectives.Select(o => o.Id).ToArray();
        for (int i = 0; i < objectiveCount; i++)
        {
            var isStretch = i >= objectiveCount - 2;
            var isComplete = i < 3;
            var status = isStretch ? Enums.ObjectiveStatus.Completed : Enums.ObjectiveStatus.InProgress;
            sut.UpdateObjective(objectiveIds[i], status, isStretch);
        }

        // Act
        var result = sut.CalculatePredictability(_dateTimeService.Today);

        // Assert
        // 3/4 of the non-stretch objectives are complete and 0/2 of the stretch objectives are complete, so 75% predictability
        result.Should().Be(50);
    }

    #endregion CalculatePredictability
}
