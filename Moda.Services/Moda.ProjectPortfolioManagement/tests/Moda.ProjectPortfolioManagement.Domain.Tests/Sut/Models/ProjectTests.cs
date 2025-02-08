using FluentAssertions;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.ProjectPortfolioManagement.Domain.Models;
using Moda.ProjectPortfolioManagement.Domain.Tests.Data;
using Moda.Tests.Shared;
using NodaTime.Extensions;
using NodaTime.Testing;

namespace Moda.ProjectPortfolioManagement.Domain.Tests.Sut.Models;

public class ProjectTests
{
    private readonly TestingDateTimeProvider _dateTimeProvider;
    private readonly ProjectFaker _projectFaker;

    public ProjectTests()
    {
        _dateTimeProvider = new TestingDateTimeProvider(new FakeClock(DateTime.UtcNow.ToInstant()));
        _projectFaker = new ProjectFaker();
    }

    #region Project Create and Update

    [Fact]
    public void Create_ShouldCreateProposedProjectSuccessfully()
    {
        // Arrange
        var name = "Test Project";
        var description = "Test Description";
        var portfolioId = Guid.NewGuid();

        // Act
        var project = Project.Create(name, description, portfolioId);

        // Assert
        project.Should().NotBeNull();
        project.Name.Should().Be(name);
        project.Description.Should().Be(description);
        project.Status.Should().Be(ProjectStatus.Proposed);
        project.PortfolioId.Should().Be(portfolioId);
        project.ProgramId.Should().BeNull();
        project.DateRange.Should().BeNull();
    }

    [Fact]
    public void UpdateDetails_ShouldFail_WhenNameIsEmpty()
    {
        // Arrange
        var program = _projectFaker.Generate();

        // Act
        Action action = () => program.UpdateDetails("", "Valid Description");

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("Required input Name was empty. (Parameter 'Name')");
    }

    [Fact]
    public void UpdateDetails_ShouldFail_WhenDescriptionIsEmpty()
    {
        // Arrange
        var program = _projectFaker.Generate();

        // Act
        Action action = () => program.UpdateDetails("Valid Name", "");

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("Required input Description was empty. (Parameter 'Description')");
    }

    #endregion Project Create and Update

    #region Lifecycle Tests

    [Fact]
    public void Activate_ShouldActivateProposedProjectSuccessfully()
    {
        // Arrange
        var project = _projectFaker.Generate();
        var startDate = _dateTimeProvider.Today;

        // Act
        var result = project.Activate(startDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.Status.Should().Be(ProjectStatus.Active);
        project.DateRange.Should().NotBeNull();
        project.DateRange!.Start.Should().Be(startDate);
        project.DateRange.End.Should().BeNull();
    }

    [Fact]
    public void Activate_ShouldFail_WhenProjectIsNotProposed()
    {
        // Arrange
        var project = _projectFaker.ActiveProject(_dateTimeProvider, Guid.NewGuid());

        // Act
        var result = project.Activate(_dateTimeProvider.Today);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Only proposed projects can be activated.");
    }

    [Fact]
    public void Complete_ShouldCompleteActiveProjectSuccessfully()
    {
        // Arrange
        var project = _projectFaker.ActiveProject(_dateTimeProvider, Guid.NewGuid());
        var endDate = _dateTimeProvider.Today.PlusDays(10);

        // Act
        var result = project.Complete(endDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.Status.Should().Be(ProjectStatus.Completed);
        project.DateRange.Should().NotBeNull();
        project.DateRange!.End.Should().Be(endDate);
    }

    [Fact]
    public void Complete_ShouldFail_WhenProjectIsNotActive()
    {
        // Arrange
        var project = _projectFaker.Generate();
        var endDate = _dateTimeProvider.Today.PlusDays(10);

        // Act
        var result = project.Complete(endDate);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Only active projects can be completed.");
    }

    [Fact]
    public void Complete_ShouldFail_WhenEndDateIsBeforeStartDate()
    {
        // Arrange
        var project = _projectFaker.ActiveProject(_dateTimeProvider, Guid.NewGuid());
        var endDate = project.DateRange!.Start.PlusDays(-1);

        // Act
        var result = project.Complete(endDate);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("The end date cannot be earlier than the start date.");
    }

    [Fact]
    public void Cancel_ShouldCancelActiveProjectSuccessfully()
    {
        // Arrange
        var project = _projectFaker.ActiveProject(_dateTimeProvider, Guid.NewGuid());
        var endDate = _dateTimeProvider.Today.PlusDays(10);

        // Act
        var result = project.Cancel(endDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.Status.Should().Be(ProjectStatus.Cancelled);
        project.DateRange.Should().NotBeNull();
        project.DateRange!.End.Should().Be(endDate);
    }

    [Fact]
    public void Cancel_ShouldFail_WhenProjectIsAlreadyCompletedOrCancelled()
    {
        // Arrange
        var project = _projectFaker.CancelledProject(_dateTimeProvider, Guid.NewGuid());
        var endDate = _dateTimeProvider.Today.PlusDays(10);

        // Act
        var result = project.Cancel(endDate);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("The project is already completed or cancelled.");
    }

    [Fact]
    public void Cancel_ShouldFail_WhenEndDateIsBeforeStartDate()
    {
        // Arrange
        var project = _projectFaker.ActiveProject(_dateTimeProvider, Guid.NewGuid());
        var endDate = project.DateRange!.Start.PlusDays(-1);

        // Act
        var result = project.Cancel(endDate);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("The end date cannot be earlier than the start date.");
    }

    #endregion Lifecycle Tests

    #region Program Association Tests

    [Fact]
    public void UpdateProgram_ShouldAssociateProjectWithProgramSuccessfully()
    {
        // Arrange
        var project = _projectFaker.Generate();
        var program = Program.Create("Test Program", "Description", project.PortfolioId);

        // Act
        var result = project.UpdateProgram(program);

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.ProgramId.Should().Be(program.Id);
    }

    [Fact]
    public void UpdateProgram_ShouldFail_WhenProgramIsInDifferentPortfolio()
    {
        // Arrange
        var project = _projectFaker.Generate();
        var portfolioId = Guid.NewGuid();
        var program = Program.Create("Test Program", "Description", portfolioId);

        // Act
        var result = project.UpdateProgram(program);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("The project must belong to the same portfolio as the program.");
    }

    [Fact]
    public void UpdateProgram_ShouldRemoveProgramAssociation_WhenNullProgramPassed()
    {
        // Arrange
        var project = _projectFaker.WithData(programId: Guid.NewGuid()).Generate();

        // Act
        var result = project.UpdateProgram(null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.ProgramId.Should().BeNull();
    }

    #endregion Program Association Tests
}
