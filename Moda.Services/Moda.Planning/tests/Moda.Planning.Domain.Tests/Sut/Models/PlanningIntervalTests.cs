using Moda.Common.Domain.Enums.Organization;
using Moda.Common.Domain.Enums.Planning;
using Moda.Common.Models;
using Moda.Planning.Domain.Enums;
using Moda.Planning.Domain.Models;
using Moda.Planning.Domain.Tests.Data;
using Moda.Tests.Shared;
using NodaTime.Extensions;
using NodaTime.Testing;

namespace Moda.Planning.Domain.Tests.Sut.Models;
public class PlanningIntervalTests
{
    private readonly TestingDateTimeProvider _dateTimeProvider;
    private readonly PlanningIntervalFaker _planningIntervalFaker = new();

    public PlanningIntervalTests()
    {
        _dateTimeProvider = new(new FakeClock(DateTime.UtcNow.ToInstant()));
    }

    #region StateOn

    [Fact]
    public void StateOn_ShouldReturnCompleted_WhenDateIsPast()
    {
        // Arrange
        var today = _dateTimeProvider.Today;
        var planningIntervalDateRange = new LocalDateRange(today.Plus(Period.FromWeeks(-14)), today.Plus(Period.FromWeeks(-2)));
        var sut = _planningIntervalFaker.WithData(dateRange: planningIntervalDateRange).Generate();

        // Act
        var result = sut.StateOn(today);

        // Assert
        result.Should().Be(IterationState.Completed);
    }

    [Fact]
    public void StateOn_ShouldReturnActive_WhenDateIsWithinRange()
    {
        // Arrange
        var today = _dateTimeProvider.Today;
        var planningIntervalDateRange = new LocalDateRange(today.Plus(Period.FromWeeks(-1)), today.Plus(Period.FromWeeks(11)));
        var sut = _planningIntervalFaker.WithData(dateRange: planningIntervalDateRange).Generate();

        // Act
        var result = sut.StateOn(today);

        // Assert
        result.Should().Be(IterationState.Active);
    }

    [Fact]
    public void StateOn_ShouldReturnFuture_WhenDateIsFuture()
    {
        // Arrange
        var today = _dateTimeProvider.Today;
        var planningIntervalDateRange = new LocalDateRange(today.Plus(Period.FromWeeks(1)), today.Plus(Period.FromWeeks(13)));
        var sut = _planningIntervalFaker.WithData(dateRange: planningIntervalDateRange).Generate();

        // Act
        var result = sut.StateOn(today);

        // Assert
        result.Should().Be(IterationState.Future);
    }

    #endregion StateOn

    #region CalculatePredictability

    [Fact]
    public void CalculatePredictability_WhenStartedAndNoObjectives_ReturnsNull()
    {
        // Arrange
        var sut = _planningIntervalFaker.Generate();

        // Act
        var result = sut.CalculatePredictability(_dateTimeProvider.Today);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CalculatePredictability_WhenFuture_ReturnsNull()
    {
        // Arrange
        var sut = _planningIntervalFaker.Generate();
        sut.Update(sut.Name, sut.Description, false);

        // Act
        var result = sut.CalculatePredictability(_dateTimeProvider.Today);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CalculatePredictability_WhenNoCompletedObjectives_Returns0()
    {
        // Arrange
        var team = new PlanningTeamFaker(TeamType.Team).Generate();
        var sut = _planningIntervalFaker.WithObjectives(team, 2).Generate();

        // Act
        var result = sut.CalculatePredictability(_dateTimeProvider.Today);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void CalculatePredictability_WhenHalfOfObjectivesCompleted_Returns50()
    {
        // Arrange
        var team = new PlanningTeamFaker(TeamType.Team).Generate();
        var sut = _planningIntervalFaker.WithObjectives(team, 6).Generate();

        var objectiveIds = sut.Objectives.Select(o => o.Id).ToArray();
        for (int i = 0; i < 3; i++)
        {
            sut.UpdateObjective(objectiveIds[i], Enums.ObjectiveStatus.Completed, false);
        }

        // Act
        var result = sut.CalculatePredictability(_dateTimeProvider.Today);

        // Assert
        result.Should().Be(50);
    }

    [Fact]
    public void CalculatePredictability_WithStretchAndWhenAllCompleted_Returns100()
    {
        // Arrange
        var objectiveCount = 6;
        var team = new PlanningTeamFaker(TeamType.Team).Generate();
        var sut = new PlanningIntervalFaker().WithObjectives(team, objectiveCount).Generate();

        var objectiveIds = sut.Objectives.Select(o => o.Id).ToArray();
        for (int i = 0; i < objectiveCount; i++)
        {
            var isStretch = i >= objectiveCount - 2;
            sut.UpdateObjective(objectiveIds[i], Enums.ObjectiveStatus.Completed, isStretch);
        }

        // Act
        var result = sut.CalculatePredictability(_dateTimeProvider.Today);

        // Assert
        result.Should().Be(100);
    }

    [Fact]
    public void CalculatePredictability_WhenAllButOneNonStretchCompleted_Returns100()
    {
        // Arrange
        var objectiveCount = 6;
        var team = new PlanningTeamFaker(TeamType.Team).Generate();
        var sut = _planningIntervalFaker.WithObjectives(team, objectiveCount).Generate();

        var objectiveIds = sut.Objectives.Select(o => o.Id).ToArray();
        for (int i = 1; i < objectiveCount; i++) // skip the first one so it is still open
        {
            var isStretch = i >= objectiveCount - 2;
            sut.UpdateObjective(objectiveIds[i], Enums.ObjectiveStatus.Completed, isStretch);
        }

        // Act
        var result = sut.CalculatePredictability(_dateTimeProvider.Today);

        // Assert
        result.Should().Be(100);
    }

    [Fact]
    public void CalculatePredictability_WhenAllCompletedExceptStretch_ReturnsOneHundred()
    {
        // Arrange
        var objectiveCount = 6;
        var team = new PlanningTeamFaker(TeamType.Team).Generate();
        var sut = _planningIntervalFaker.WithObjectives(team, objectiveCount).Generate();

        var objectiveIds = sut.Objectives.Select(o => o.Id).ToArray();
        for (int i = 0; i < objectiveCount; i++)
        {
            var isStretch = i >= objectiveCount - 2;
            var status = isStretch ? Enums.ObjectiveStatus.InProgress : Enums.ObjectiveStatus.Completed;
            sut.UpdateObjective(objectiveIds[i], status, isStretch);
        }

        // Act
        var result = sut.CalculatePredictability(_dateTimeProvider.Today);

        // Assert
        result.Should().Be(100);
    }

    [Fact]
    public void CalculatePredictability_WhenThreeNonStretchComplete_Returns75()
    {
        // Arrange
        var objectiveCount = 6;
        var team = new PlanningTeamFaker(TeamType.Team).Generate();
        var sut = _planningIntervalFaker.WithObjectives(team, objectiveCount).Generate();

        var objectiveIds = sut.Objectives.Select(o => o.Id).ToArray();
        for (int i = 0; i < objectiveCount; i++)
        {
            var isStretch = i >= objectiveCount - 2;
            var isComplete = i < 3;
            var status = isComplete ? Enums.ObjectiveStatus.Completed : Enums.ObjectiveStatus.InProgress;
            sut.UpdateObjective(objectiveIds[i], status, isStretch);
        }

        // Act
        var result = sut.CalculatePredictability(_dateTimeProvider.Today);

        // Assert
        // 3/4 of the non-stretch objectives are complete and 0/2 of the stretch objectives are complete, so 75% predictability
        result.Should().Be(75);
    }

    [Fact]
    public void CalculatePredictability_WhenOnlyStretchComplete_Returns50()
    {
        // Arrange
        var objectiveCount = 6;
        var team = new PlanningTeamFaker(TeamType.Team).Generate();
        var sut = _planningIntervalFaker.WithObjectives(team, objectiveCount).Generate();

        var objectiveIds = sut.Objectives.Select(o => o.Id).ToArray();
        for (int i = 0; i < objectiveCount; i++)
        {
            var isStretch = i >= objectiveCount - 2;
            var isComplete = i < 3;
            var status = isStretch ? Enums.ObjectiveStatus.Completed : Enums.ObjectiveStatus.InProgress;
            sut.UpdateObjective(objectiveIds[i], status, isStretch);
        }

        // Act
        var result = sut.CalculatePredictability(_dateTimeProvider.Today);

        // Assert
        // 3/4 of the non-stretch objectives are complete and 0/2 of the stretch objectives are complete, so 75% predictability
        result.Should().Be(50);
    }

    #endregion CalculatePredictability

    #region Initialize Iterations

    [Fact]
    public void InitializeIterations_WhenNoIterationsExist_ReturnsSuccess()
    {
        // Arrange
        var sut = _planningIntervalFaker.Generate();
        sut.ManageDates(new LocalDateRange(new LocalDate(2023, 10, 2), new LocalDate(2023, 11, 26)), []);
        var expectedIterations = 4;

        // Act
        var result = sut.InitializeIterations(2, "Iteration ");
        var iterations = sut.Iterations.ToList();

        // Assert
        result.IsSuccess.Should().BeTrue();
        iterations.Count.Should().Be(expectedIterations);
        iterations[0].Name.Should().Be("Iteration 1");
        iterations[0].Category.Should().Be(IterationCategory.Development);
        iterations[0].DateRange.Start.Should().Be(new LocalDate(2023, 10, 2));
        iterations[0].DateRange.End.Should().Be(new LocalDate(2023, 10, 15));
        iterations[1].Name.Should().Be("Iteration 2");
        iterations[1].Category.Should().Be(IterationCategory.Development);
        iterations[1].DateRange.Start.Should().Be(new LocalDate(2023, 10, 16));
        iterations[1].DateRange.End.Should().Be(new LocalDate(2023, 10, 29));
        iterations[2].Name.Should().Be("Iteration 3");
        iterations[2].Category.Should().Be(IterationCategory.Development);
        iterations[2].DateRange.Start.Should().Be(new LocalDate(2023, 10, 30));
        iterations[2].DateRange.End.Should().Be(new LocalDate(2023, 11, 12));
        iterations[3].Name.Should().Be("Iteration 4");
        iterations[3].Category.Should().Be(IterationCategory.InnovationAndPlanning);
        iterations[3].DateRange.Start.Should().Be(new LocalDate(2023, 11, 13));
        iterations[3].DateRange.End.Should().Be(new LocalDate(2023, 11, 26));
    }

    [Fact]
    public void InitializeIterations_WhenNoIterationsExistAndUnevenDates_ReturnsSuccess()
    {
        // Arrange
        var sut = _planningIntervalFaker.Generate();
        sut.ManageDates(new LocalDateRange(new LocalDate(2023, 10, 2), new LocalDate(2023, 11, 27)), []);
        var expectedIterations = 5;

        // Act
        var result = sut.InitializeIterations(2, "Iteration ");
        var iterations = sut.Iterations.ToList();

        // Assert
        result.IsSuccess.Should().BeTrue();
        iterations.Count.Should().Be(expectedIterations);
        iterations[0].Name.Should().Be("Iteration 1");
        iterations[0].Category.Should().Be(IterationCategory.Development);
        iterations[0].DateRange.Start.Should().Be(new LocalDate(2023, 10, 2));
        iterations[0].DateRange.End.Should().Be(new LocalDate(2023, 10, 15));
        iterations[1].Name.Should().Be("Iteration 2");
        iterations[1].Category.Should().Be(IterationCategory.Development);
        iterations[1].DateRange.Start.Should().Be(new LocalDate(2023, 10, 16));
        iterations[1].DateRange.End.Should().Be(new LocalDate(2023, 10, 29));
        iterations[2].Name.Should().Be("Iteration 3");
        iterations[2].Category.Should().Be(IterationCategory.Development);
        iterations[2].DateRange.Start.Should().Be(new LocalDate(2023, 10, 30));
        iterations[2].DateRange.End.Should().Be(new LocalDate(2023, 11, 12));
        iterations[3].Name.Should().Be("Iteration 4");
        iterations[3].Category.Should().Be(IterationCategory.Development);
        iterations[3].DateRange.Start.Should().Be(new LocalDate(2023, 11, 13));
        iterations[3].DateRange.End.Should().Be(new LocalDate(2023, 11, 26));
        iterations[4].Name.Should().Be("Iteration 5");
        iterations[4].Category.Should().Be(IterationCategory.InnovationAndPlanning);
        iterations[4].DateRange.Start.Should().Be(new LocalDate(2023, 11, 27));
        iterations[4].DateRange.End.Should().Be(new LocalDate(2023, 11, 27));
    }

    [Fact]
    public void InitializeIterations_WhenIterationsAlreadyExist_ReturnsFailure()
    {
        // Arrange
        var sut = _planningIntervalFaker.Generate();

        List<UpsertPlanningIntervalIteration> iterations = new();
        iterations.Add(UpsertPlanningIntervalIteration.Create(null, "Iteration 1", IterationCategory.Development, new LocalDateRange(new LocalDate(2023, 1, 1), new LocalDate(2023, 1, 31))));
        iterations.Add(UpsertPlanningIntervalIteration.Create(null, "Iteration 2", IterationCategory.Development, new LocalDateRange(new LocalDate(2023, 2, 1), new LocalDate(2023, 2, 28))));
        iterations.Add(UpsertPlanningIntervalIteration.Create(null, "Iteration 3", IterationCategory.InnovationAndPlanning, new LocalDateRange(new LocalDate(2023, 3, 1), new LocalDate(2023, 3, 31))));

        sut.ManageDates(new LocalDateRange(new LocalDate(2023, 1, 1), new LocalDate(2023, 3, 31)), iterations);

        // Act
        var result = sut.InitializeIterations(4, "Iteration ");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Unable to generate new iterations for a Planning Interval that has iterations.");
    }

    #endregion Initialize Iterations

    #region Add Iteration

    [Fact]
    public void AddIteration_WhenAddingValidIteration_ReturnsSuccess()
    {
        // Arrange
        var piDates = new LocalDateRange(new LocalDate(2023, 1, 1), new LocalDate(2023, 3, 31));
        var sut = _planningIntervalFaker.WithData(dateRange: piDates).Generate();

        // Act
        var result = sut.AddIteration("Iteration 1", IterationCategory.Development, new LocalDateRange(new LocalDate(2023, 1, 1), new LocalDate(2023, 1, 31)));
        var iterations = sut.Iterations.ToList();

        // Assert
        result.IsSuccess.Should().BeTrue();
        iterations.Count.Should().Be(1);
        iterations[0].Name.Should().Be("Iteration 1");
        iterations[0].Category.Should().Be(IterationCategory.Development);
        iterations[0].DateRange.Start.Should().Be(new LocalDate(2023, 1, 1));
        iterations[0].DateRange.End.Should().Be(new LocalDate(2023, 1, 31));
    }

    [Fact]
    public void AddIteration_WithDuplicateName_ReturnsFailure()
    {
        // Arrange
        var piDates = new LocalDateRange(new LocalDate(2023, 1, 1), new LocalDate(2023, 3, 31));
        var sut = _planningIntervalFaker.WithData(dateRange: piDates).Generate();

        sut.AddIteration("Iteration 1", IterationCategory.Development, new LocalDateRange(new LocalDate(2023, 1, 1), new LocalDate(2023, 1, 31)));

        // Act
        var result = sut.AddIteration("Iteration 1", IterationCategory.Development, new LocalDateRange(new LocalDate(2023, 1, 1), new LocalDate(2023, 1, 31)));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Iteration name already exists.");
    }

    [Fact]
    public void AddIteration_WithOverlappingDateRange_ReturnsFailure()
    {
        // Arrange
        var piDates = new LocalDateRange(new LocalDate(2023, 1, 1), new LocalDate(2023, 3, 31));
        var sut = _planningIntervalFaker.WithData(dateRange: piDates).Generate();

        sut.AddIteration("Iteration 1", IterationCategory.Development, new LocalDateRange(new LocalDate(2023, 1, 1), new LocalDate(2023, 1, 31)));

        // Act
        var result = sut.AddIteration("Iteration 2", IterationCategory.Development, new LocalDateRange(new LocalDate(2023, 1, 31), new LocalDate(2023, 2, 15)));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Iteration date range overlaps with existing iteration date range.");
    }

    [Fact]
    public void AddIteration_WithStartDateBeforePIStartDate_ReturnsFailure()
    {
        // Arrange
        var piDates = new LocalDateRange(new LocalDate(2023, 1, 1), new LocalDate(2023, 3, 31));
        var sut = _planningIntervalFaker.WithData(dateRange: piDates).Generate();

        // Act
        var result = sut.AddIteration("Iteration 1", IterationCategory.Development, new LocalDateRange(new LocalDate(2020, 12, 31), new LocalDate(2023, 1, 30)));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Iteration date range cannot start before the Planning Interval date range.");
    }

    [Fact]
    public void AddIteration_WithEndDateAfterPIEndDate_ReturnsFailure()
    {
        // Arrange
        var piDates = new LocalDateRange(new LocalDate(2023, 1, 1), new LocalDate(2023, 3, 31));
        var sut = _planningIntervalFaker.WithData(dateRange: piDates).Generate();

        // Act
        var result = sut.AddIteration("Iteration 1", IterationCategory.Development, new LocalDateRange(new LocalDate(2023, 3, 20), new LocalDate(2023, 4, 1)));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Iteration date range cannot end after the Planning Interval date range.");
    }

    #endregion Add Iteration

    #region ManageDates

    [Fact]
    public void ManageDates_WhenDatesAreValidAndNoIterations_ReturnsSuccess()
    {
        // Arrange
        var sut = _planningIntervalFaker.Generate();
        var expectedStartDate = new LocalDate(2023, 1, 1);
        var expectedEndDate = new LocalDate(2023, 3, 31);

        // Act
        var result = sut.ManageDates(new LocalDateRange(expectedStartDate, expectedEndDate), new List<UpsertPlanningIntervalIteration>());
        var iterations = sut.Iterations.ToList();

        // Assert
        result.IsSuccess.Should().BeTrue();
        sut.DateRange.Start.Should().Be(expectedStartDate);
        sut.DateRange.End.Should().Be(expectedEndDate);
        iterations.Count.Should().Be(0);
    }

    [Fact]
    public void ManageDates_WhenAddingInitailIterations_ReturnsSuccess()
    {
        // Arrange
        var sut = _planningIntervalFaker.Generate();
        var expectedStartDate = new LocalDate(2023, 1, 1);
        var expectedEndDate = new LocalDate(2023, 3, 31);

        var iterations = new List<UpsertPlanningIntervalIteration>
        {
            UpsertPlanningIntervalIteration.Create(null, "Iteration 1", IterationCategory.Development, new LocalDateRange(new LocalDate(2023, 1, 1), new LocalDate(2023, 1, 31))),
            UpsertPlanningIntervalIteration.Create(null, "Iteration 2", IterationCategory.Development, new LocalDateRange(new LocalDate(2023, 2, 1), new LocalDate(2023, 2, 28))),
            UpsertPlanningIntervalIteration.Create(null, "Iteration 3", IterationCategory.InnovationAndPlanning, new LocalDateRange(new LocalDate(2023, 3, 1), new LocalDate(2023, 3, 31)))
        };

        // Act
        var result = sut.ManageDates(new LocalDateRange(expectedStartDate, expectedEndDate), iterations);
        var updatedIterations = sut.Iterations.ToList();

        // Assert
        result.IsSuccess.Should().BeTrue();
        sut.DateRange.Start.Should().Be(expectedStartDate);
        sut.DateRange.End.Should().Be(expectedEndDate);
        updatedIterations.Count.Should().Be(3);
        updatedIterations[0].Name.Should().Be("Iteration 1");
        updatedIterations[0].Category.Should().Be(IterationCategory.Development);
        updatedIterations[0].DateRange.Start.Should().Be(new LocalDate(2023, 1, 1));
        updatedIterations[0].DateRange.End.Should().Be(new LocalDate(2023, 1, 31));
        updatedIterations[1].Name.Should().Be("Iteration 2");
        updatedIterations[1].Category.Should().Be(IterationCategory.Development);
        updatedIterations[1].DateRange.Start.Should().Be(new LocalDate(2023, 2, 1));
        updatedIterations[1].DateRange.End.Should().Be(new LocalDate(2023, 2, 28));
        updatedIterations[2].Name.Should().Be("Iteration 3");
        updatedIterations[2].Category.Should().Be(IterationCategory.InnovationAndPlanning);
    }

    [Fact]
    public void ManageDates_WhenDuplicateNames_ReturnsFailure()
    {
        // Arrange
        var startDate = new LocalDate(2023, 1, 1);
        var endDate = new LocalDate(2023, 3, 31);

        var sut = _planningIntervalFaker
            .WithIterations(new LocalDateRange(startDate, endDate), 2, "Iteration ")
            .Generate();

        var iterations = sut.Iterations
            .Select(i => UpsertPlanningIntervalIteration.Create(i.Id, i.Name, i.Category, i.DateRange))
            .ToList();

        iterations.Last().Name = iterations.First().Name;

        // Act
        var result = sut.ManageDates(new LocalDateRange(sut.DateRange.Start, sut.DateRange.End), iterations);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Iteration names must be unique within the PI.");
    }

    [Fact]
    public void ManageDates_WhenStartingBeforePI_ReturnsFailure()
    {
        // Arrange
        var startDate = new LocalDate(2023, 1, 1);
        var endDate = new LocalDate(2023, 3, 31);

        var sut = _planningIntervalFaker
            .WithIterations(new LocalDateRange(startDate, endDate), 2, "Iteration ")
            .Generate();

        var iterations = sut.Iterations
            .Select(i => UpsertPlanningIntervalIteration.Create(i.Id, i.Name, i.Category, i.DateRange))
            .ToList();

        iterations.First().DateRange = new LocalDateRange(startDate.Plus(Period.FromDays(-1)), iterations.First().DateRange.End);

        // Act
        var result = sut.ManageDates(new LocalDateRange(sut.DateRange.Start, sut.DateRange.End), iterations);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Iteration date ranges cannot start before the Planning Interval date range.");
    }

    [Fact]
    public void ManageDates_WhenEndingAfterPI_ReturnsFailure()
    {
        // Arrange
        var startDate = new LocalDate(2023, 1, 1);
        var endDate = new LocalDate(2023, 3, 31);

        var sut = _planningIntervalFaker
            .WithIterations(new LocalDateRange(startDate, endDate), 2, "Iteration ")
            .Generate();

        var iterations = sut.Iterations
            .Select(i => UpsertPlanningIntervalIteration.Create(i.Id, i.Name, i.Category, i.DateRange))
            .ToList();

        iterations.Last().DateRange = new LocalDateRange(iterations.Last().DateRange.Start, endDate.Plus(Period.FromDays(1)));

        // Act
        var result = sut.ManageDates(new LocalDateRange(sut.DateRange.Start, sut.DateRange.End), iterations);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Iteration date ranges cannot end after the Planning Interval date range.");
    }

    [Fact]
    public void ManageDates_WhenOverlappingDates_ReturnsFailure()
    {
        // Arrange
        var startDate = new LocalDate(2023, 1, 1);
        var endDate = new LocalDate(2023, 3, 31);

        var sut = _planningIntervalFaker
            .WithIterations(new LocalDateRange(startDate, endDate), 2, "Iteration ")
            .Generate();

        var iterations = sut.Iterations
            .Select(i => UpsertPlanningIntervalIteration.Create(i.Id, i.Name, i.Category, i.DateRange))
            .ToList();

        var secondIteration = iterations[1];

        secondIteration.DateRange = new LocalDateRange(secondIteration.DateRange.Start.Plus(Period.FromDays(-1)), secondIteration.DateRange.End);

        // Act
        var result = sut.ManageDates(new LocalDateRange(sut.DateRange.Start, sut.DateRange.End), iterations);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Iteration date ranges cannot overlap.");
    }

    // TODO: Add tests updating and removing iterations

    #endregion ManageDates
}
