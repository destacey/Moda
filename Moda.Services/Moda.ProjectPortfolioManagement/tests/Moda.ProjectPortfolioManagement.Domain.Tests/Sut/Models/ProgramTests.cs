using FluentAssertions;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.ProjectPortfolioManagement.Domain.Models;
using Moda.ProjectPortfolioManagement.Domain.Tests.Data;
using Moda.Tests.Shared;
using NodaTime.Extensions;
using NodaTime.Testing;

namespace Moda.ProjectPortfolioManagement.Domain.Tests.Sut.Models;

public class ProgramTests
{
    private readonly TestingDateTimeProvider _dateTimeProvider;
    private readonly ProgramFaker _programFaker;
    private readonly ProjectFaker _projectFaker;
    private readonly StrategicThemeFaker _themeFaker;

    public ProgramTests()
    {
        _dateTimeProvider = new TestingDateTimeProvider(new FakeClock(DateTime.UtcNow.ToInstant()));
        _programFaker = new ProgramFaker();
        _projectFaker = new ProjectFaker();
        _themeFaker = new StrategicThemeFaker();
    }

    #region Program Create and Update

    [Fact]
    public void Create_ShouldCreateProposedProgramSuccessfully()
    {
        // Arrange
        var name = "Test Program";
        var description = "Test Description";
        var portfolioId = Guid.NewGuid();

        // Act
        var program = Program.Create(name, description, portfolioId);

        // Assert
        program.Should().NotBeNull();
        program.Name.Should().Be(name);
        program.Description.Should().Be(description);
        program.Status.Should().Be(ProgramStatus.Proposed);
        program.PortfolioId.Should().Be(portfolioId);
        program.DateRange.Should().BeNull();
        program.Projects.Should().BeEmpty();
    }

    [Fact]
    public void UpdateDetails_ShouldFail_WhenNameIsEmpty()
    {
        // Arrange
        var program = _programFaker.Generate();

        // Act
        Action action = () => program.UpdateDetails("", "Valid Description");

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("Required input Name was empty. (Parameter 'Name')");
    }

    [Fact]
    public void UpdateDetails_ShouldFail_WhenDescriptionIsEmpty()
    {
        // Arrange
        var program = _programFaker.Generate();

        // Act
        Action action = () => program.UpdateDetails("Valid Name", "");

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("Required input Description was empty. (Parameter 'Description')");
    }


    #endregion Program Create and Update

    #region Lifecycle Tests

    [Fact]
    public void Activate_ShouldActivateProposedProgramSuccessfully()
    {
        // Arrange
        var program = _programFaker.Generate();
        var startDate = _dateTimeProvider.Today;

        // Act
        var result = program.Activate(startDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        program.Status.Should().Be(ProgramStatus.Active);
        program.DateRange.Should().NotBeNull();
        program.DateRange!.Start.Should().Be(startDate);
    }

    [Fact]
    public void Activate_ShouldFail_WhenProgramIsAlreadyActive()
    {
        // Arrange
        var program = _programFaker.ActiveProgram(_dateTimeProvider);

        // Act
        var result = program.Activate(_dateTimeProvider.Today);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Only proposed programs can be activated.");
    }

    [Fact]
    public void Complete_ShouldCompleteActiveProgramSuccessfully()
    {
        // Arrange
        var program = _programFaker.ActiveProgram(_dateTimeProvider);
        var endDate = _dateTimeProvider.Today.PlusDays(10);

        // Act
        var result = program.Complete(endDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        program.Status.Should().Be(ProgramStatus.Completed);
        program.DateRange.Should().NotBeNull();
        program.DateRange!.End.Should().Be(endDate);
    }

    [Fact]
    public void Complete_ShouldFail_WhenProgramIsAlreadyCompleted()
    {
        // Arrange
        var program = _programFaker.CompletedProgram(_dateTimeProvider);

        // Act
        var result = program.Complete(_dateTimeProvider.Today.PlusDays(5));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Only active programs can be completed.");
    }

    [Fact]
    public void Cancel_ShouldCancelActiveProgramSuccessfully()
    {
        // Arrange
        var program = _programFaker.ActiveProgram(_dateTimeProvider);
        var endDate = _dateTimeProvider.Today.PlusDays(10);

        // Act
        var result = program.Cancel(endDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        program.Status.Should().Be(ProgramStatus.Cancelled);
        program.DateRange.Should().NotBeNull();
        program.DateRange!.End.Should().Be(endDate);
    }

    [Fact]
    public void Cancel_ShouldFail_WhenProgramHasActiveProjects()
    {
        // Arrange
        var program = _programFaker.ActiveProgram(_dateTimeProvider);
        var project = _projectFaker.ActiveProject(_dateTimeProvider, program.PortfolioId);
        program.AddProject(project);

        var endDate = _dateTimeProvider.Today.PlusDays(10);

        // Act
        var result = program.Cancel(endDate);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("All projects must be completed or canceled before the program can be cancelled.");
    }

    [Fact]
    public void Cancel_ShouldFail_WhenEndDateIsBeforeStartDate()
    {
        // Arrange
        var program = _programFaker.ActiveProgram(_dateTimeProvider);
        var endDate = program.DateRange!.Start.PlusDays(-1);

        // Act
        var result = program.Cancel(endDate);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("The end date cannot be earlier than the start date.");
    }

    [Fact]
    public void Cancel_ShouldFail_WhenProgramIsAlreadyCancelled()
    {
        // Arrange
        var program = _programFaker.CancelledProgram(_dateTimeProvider);

        // Act
        var result = program.Cancel(_dateTimeProvider.Today.PlusDays(5));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("The program is already completed or cancelled.");
    }


    #endregion Lifecycle Tests

    #region Project Management

    [Fact]
    public void AddProject_ShouldAddProjectToProgramSuccessfully()
    {
        // Arrange
        Guid portfolioId = Guid.NewGuid();
        var program = _programFaker.ActiveProgram(_dateTimeProvider, portfolioId);
        var project = _projectFaker.WithData(portfolioId: portfolioId).Generate();

        // Act
        var result = program.AddProject(project);

        // Assert
        result.IsSuccess.Should().BeTrue();
        program.Projects.Should().ContainSingle();
        program.Projects.First().Should().Be(project);
    }

    [Fact]
    public void AddProject_ShouldFail_WhenProgramIsNotAcceptingProjects()
    {
        // Arrange
        Guid portfolioId = Guid.NewGuid();
        var program = _programFaker.CompletedProgram(_dateTimeProvider, portfolioId);
        var project = _projectFaker.WithData(portfolioId: portfolioId).Generate();

        // Act
        var result = program.AddProject(project);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("The program is not accepting new projects.");
    }

    [Fact]
    public void AddProject_ShouldFail_WhenProjectBelongsToDifferentPortfolio()
    {
        // Arrange
        Guid portfolioId1 = Guid.NewGuid();
        Guid portfolioId2 = Guid.NewGuid();
        var program = _programFaker.ActiveProgram(_dateTimeProvider, portfolioId1);
        var project = _projectFaker.WithData(portfolioId: portfolioId2).Generate();

        // Act
        var result = program.AddProject(project);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("The project must belong to the same portfolio as the program.");
    }

    [Fact]
    public void AddProject_ShouldFail_WhenProjectIsAlreadyInProgram()
    {
        // Arrange
        Guid portfolioId = Guid.NewGuid();
        var program = _programFaker.ActiveProgram(_dateTimeProvider, portfolioId);
        var project = _projectFaker.WithData(portfolioId: portfolioId).Generate();

        program.AddProject(project);

        // Act
        var result = program.AddProject(project);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("The project is already part of this program.");
    }

    [Fact]
    public void RemoveProject_ShouldFail_WhenProjectIsNotInProgram()
    {
        // Arrange
        var program = _programFaker.ActiveProgram(_dateTimeProvider);
        var project = _projectFaker.Generate();

        // Act
        var result = program.RemoveProject(project);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("The project is not part of this program.");
    }

    #endregion Project Management

    #region Strategic Theme Management

    [Fact]
    public void UpdateStrategicThemes_ShouldUpdateThemesSuccessfully()
    {
        // Arrange
        var program = _programFaker.Generate();
        var themes = _themeFaker.Generate(3); // Generate 3 unique themes

        // Act
        var result = program.UpdateStrategicThemes(themes);

        // Assert
        result.IsSuccess.Should().BeTrue();
        program.StrategicThemes.Should().HaveCount(3);
        program.StrategicThemes.Should().BeEquivalentTo(themes);
    }

    [Fact]
    public void UpdateStrategicThemes_ShouldRemoveExistingThemes_WhenNewThemesAreAdded()
    {
        // Arrange
        var program = _programFaker.Generate();
        var initialThemes = _themeFaker.Generate(2);
        program.UpdateStrategicThemes(initialThemes);

        var newThemes = _themeFaker.Generate(3); // Replace with different themes

        // Act
        var result = program.UpdateStrategicThemes(newThemes);

        // Assert
        result.IsSuccess.Should().BeTrue();
        program.StrategicThemes.Should().HaveCount(3);
        program.StrategicThemes.Should().BeEquivalentTo(newThemes);
    }

    [Fact]
    public void UpdateStrategicThemes_ShouldFail_WhenNoChangesAreMade()
    {
        // Arrange
        var program = _programFaker.Generate();
        var themes = _themeFaker.Generate(2);
        program.UpdateStrategicThemes(themes);

        // Act
        var result = program.UpdateStrategicThemes(themes); // Same themes

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("No changes detected in strategic themes.");
    }

    [Fact]
    public void UpdateStrategicThemes_ShouldHandleEmptyListCorrectly()
    {
        // Arrange
        var program = _programFaker.Generate();
        var initialThemes = _themeFaker.Generate(2);
        program.UpdateStrategicThemes(initialThemes);

        // Act
        var result = program.UpdateStrategicThemes([]);

        // Assert
        result.IsSuccess.Should().BeTrue();
        program.StrategicThemes.Should().BeEmpty();
    }

    #endregion Strategic Theme Management
}
