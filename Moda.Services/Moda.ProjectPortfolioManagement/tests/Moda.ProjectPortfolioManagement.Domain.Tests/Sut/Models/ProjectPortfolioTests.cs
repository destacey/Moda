using FluentAssertions;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.ProjectPortfolioManagement.Domain.Models;
using Moda.ProjectPortfolioManagement.Domain.Tests.Data;
using Moda.Tests.Shared;
using NodaTime.Extensions;
using NodaTime.Testing;

namespace Moda.ProjectPortfolioManagement.Domain.Tests.Sut.Models;

public class ProjectPortfolioTests
{
    private readonly TestingDateTimeProvider _dateTimeProvider;
    private readonly ProjectPortfolioFaker _portfolioFaker;
    private readonly ProgramFaker _programFaker;
    private readonly ProjectFaker _projectFaker;

    public ProjectPortfolioTests()
    {
        _dateTimeProvider = new TestingDateTimeProvider(new FakeClock(DateTime.UtcNow.ToInstant()));
        _portfolioFaker = new ProjectPortfolioFaker();
        _programFaker = new ProgramFaker();
        _projectFaker = new ProjectFaker();
    }

    #region Portfolio Creation

    [Fact]
    public void Create_ShouldCreateProposedPortfolioSuccessfully()
    {
        // Arrange
        var name = "Test Portfolio";
        var description = "Test Description";

        // Act
        var portfolio = ProjectPortfolio.Create(name, description);

        // Assert
        portfolio.Should().NotBeNull();
        portfolio.Name.Should().Be(name);
        portfolio.Description.Should().Be(description);
        portfolio.Status.Should().Be(ProjectPortfolioStatus.Proposed);
        portfolio.DateRange.Should().BeNull();
        portfolio.Projects.Should().BeEmpty();
        portfolio.Programs.Should().BeEmpty();
    }

    #endregion Portfolio Creation

    #region Lifecycle Tests

    [Fact]
    public void Activate_ShouldActivateProposedPortfolioSuccessfully()
    {
        // Arrange
        var portfolio = _portfolioFaker.Generate();
        var startDate = _dateTimeProvider.Today;

        // Act
        var result = portfolio.Activate(startDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        portfolio.Status.Should().Be(ProjectPortfolioStatus.Active);
        portfolio.DateRange.Should().NotBeNull();
        portfolio.DateRange!.Start.Should().Be(startDate);
    }

    [Fact]
    public void Complete_ShouldFail_WhenPortfolioHasOpenProjectsOrPrograms()
    {
        // Arrange
        var portfolio = _portfolioFaker.ActivePortfolio(_dateTimeProvider);
        var project = _projectFaker.ActiveProject(_dateTimeProvider, portfolio.Id);
        portfolio.CreateProject(project.Name, project.Description);

        var endDate = _dateTimeProvider.Today.PlusDays(10);

        // Act
        var result = portfolio.Complete(endDate);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("All projects must be completed or canceled before the portfolio can be completed.");
    }

    [Fact]
    public void Complete_ShouldCompletePortfolioSuccessfully()
    {
        // Arrange
        var portfolio = _portfolioFaker.ActivePortfolio(_dateTimeProvider);

        var fakeProject = _projectFaker.ProposedProject(portfolio.Id);
        var createProjectReult = portfolio.CreateProject(fakeProject.Name, fakeProject.Description);
        var project = createProjectReult.Value;

        var endDate = _dateTimeProvider.Today.PlusDays(10);

        var activateProjectResult = project.Activate(_dateTimeProvider.Today);
        activateProjectResult.IsSuccess.Should().BeTrue();
        var completeProjectResult = project.Complete(endDate);
        completeProjectResult.IsSuccess.Should().BeTrue();

        // Act
        var result = portfolio.Complete(endDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        portfolio.Status.Should().Be(ProjectPortfolioStatus.Completed);
        portfolio.DateRange.Should().NotBeNull();
        portfolio.DateRange!.End.Should().Be(endDate);
    }

    [Fact]
    public void Archive_ShouldFail_WhenPortfolioIsNotCompleted()
    {
        // Arrange
        var portfolio = _portfolioFaker.ActivePortfolio(_dateTimeProvider);

        // Act
        var result = portfolio.Archive();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Only completed portfolios can be archived.");
    }

    [Fact]
    public void Archive_ShouldArchiveCompletedPortfolioSuccessfully()
    {
        // Arrange
        var portfolio = _portfolioFaker.CompletedPortfolio(_dateTimeProvider);

        // Act
        var result = portfolio.Archive();

        // Assert
        result.IsSuccess.Should().BeTrue();
        portfolio.Status.Should().Be(ProjectPortfolioStatus.Archived);
    }

    #endregion Lifecycle Tests

    #region Program Management

    [Fact]
    public void CreateProgram_ShouldFail_WhenPortfolioIsNotActiveOrOnHold()
    {
        // Arrange
        var portfolio = _portfolioFaker.Generate();

        // Act
        var result = portfolio.CreateProgram("Test Program", "Test Description");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Programs can only be created in active or on-hold portfolios.");
    }

    [Fact]
    public void CreateProgram_ShouldAddProgramToPortfolio()
    {
        // Arrange
        var portfolio = _portfolioFaker.ActivePortfolio(_dateTimeProvider);

        // Act
        var result = portfolio.CreateProgram("Test Program", "Test Description");

        // Assert
        result.IsSuccess.Should().BeTrue();
        portfolio.Programs.Should().ContainSingle();
        portfolio.Programs.First().Name.Should().Be("Test Program");
    }

    [Fact]
    public void Complete_ShouldFail_WhenPortfolioHasProgramsWithOpenProjects()
    {
        // Arrange
        var portfolio = _portfolioFaker.ActivePortfolio(_dateTimeProvider);
        var program = portfolio.CreateProgram("Test Program", "Description").Value;
        var project = _projectFaker.ActiveProject(_dateTimeProvider, portfolio.Id);

        program.AddProject(project);

        var endDate = _dateTimeProvider.Today.PlusDays(10);

        // Act
        var result = portfolio.Complete(endDate);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("All programs must be completed or canceled before the portfolio can be completed.");
    }


    #endregion Program Management

    #region Project Management

    [Fact]
    public void CreateProject_ShouldAddProjectToPortfolio()
    {
        // Arrange
        var portfolio = _portfolioFaker.ActivePortfolio(_dateTimeProvider);

        // Act
        var result = portfolio.CreateProject("Test Project", "Test Description");

        // Assert
        result.IsSuccess.Should().BeTrue();
        portfolio.Projects.Should().ContainSingle();
        portfolio.Projects.First().Name.Should().Be("Test Project");
    }

    [Fact]
    public void CreateProject_ShouldFail_WhenProgramDoesNotBelongToPortfolio()
    {
        // Arrange
        var portfolio = _portfolioFaker.ActivePortfolio(_dateTimeProvider);
        var program = _programFaker.Generate();

        // Act
        var result = portfolio.CreateProject("Test Project", "Test Description", program.Id);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("The specified program does not belong to this portfolio.");
    }

    [Fact]
    public void CreateProject_ShouldFail_WhenProgramIsNotAcceptingProjects()
    {
        // Arrange
        var portfolio = _portfolioFaker.ActivePortfolio(_dateTimeProvider);
        var fakeProgram = _programFaker.Generate();

        var createProgramResult = portfolio.CreateProgram(fakeProgram.Name, fakeProgram.Description);
        createProgramResult.IsSuccess.Should().BeTrue();
        var program = createProgramResult.Value;

        // Act
        var result = portfolio.CreateProject("Test Project", "Test Description", program.Id);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("The specified program is not in a valid state to accept projects.");
    }

    [Fact]
    public void ChangeProjectProgram_ShouldMoveProjectToNewProgram()
    {
        // Arrange
        var portfolio = _portfolioFaker.PortfolioWithProgramsAndProjects(_dateTimeProvider, 2, 0);

        var program1 = portfolio.Programs.First();
        var program2 = portfolio.Programs.Last();

        var project = portfolio.CreateProject("Test Project", "Description", program1.Id);
        project.IsSuccess.Should().BeTrue();

        // Act
        var result = portfolio.ChangeProjectProgram(project.Value.Id, program2.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.Value.ProgramId.Should().Be(program2.Id);
        program1.Projects.Should().NotContain(project.Value);
        program2.Projects.Should().Contain(project.Value);
    }

    //[Fact]
    //public void ChangeProjectProgram_ShouldRemoveProjectFromProgram()
    //{
    //    // Arrange
    //    var portfolio = _portfolioFaker.ActivePortfolio(_dateTimeProvider);
    //    var program = portfolio.CreateProgram("Test Program", "Description").Value;
    //    var project = portfolio.CreateProject("Test Project", "Description", program.Id).Value;

    //    // Act
    //    var result = portfolio.ChangeProjectProgram(project.Id, null);

    //    // Assert
    //    result.IsSuccess.Should().BeTrue();
    //    project.ProgramId.Should().BeNull();
    //    program.Projects.Should().NotContain(project);
    //}

    [Fact]
    public void ChangeProjectProgram_ShouldFail_WhenProjectAlreadyInProgram()
    {
        // Arrange
        var portfolio = _portfolioFaker.PortfolioWithProgramsAndProjects(_dateTimeProvider, 1, 0);

        var program = portfolio.Programs.First();

        var project = portfolio.CreateProject("Test Project", "Description", program.Id);
        project.IsSuccess.Should().BeTrue();

        // Act
        var result = portfolio.ChangeProjectProgram(project.Value.Id, program.Id);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("The project is already associated with the specified program.");
    }

    [Fact]
    public void ChangeProjectProgram_ShouldFail_WhenProgramNotInPortfolio()
    {
        // Arrange
        var portfolio1 = _portfolioFaker.PortfolioWithProgramsAndProjects(_dateTimeProvider,1,0);

        var program1 = portfolio1.Programs.First();

        var program2 = _programFaker.ActiveProgram(_dateTimeProvider, Guid.NewGuid());

        var projectResult = portfolio1.CreateProject("Test Project", "Description", program1.Id);
        projectResult.IsSuccess.Should().BeTrue();

        // Act
        var result = portfolio1.ChangeProjectProgram(projectResult.Value.Id, program2.Id);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("The specified program does not belong to this portfolio.");
    }

    [Fact]
    public void ChangeProjectProgram_ShouldFail_WhenProjectHasNoProgramAndIsRemovedAgain()
    {
        // Arrange
        var portfolio = _portfolioFaker.ActivePortfolio(_dateTimeProvider);
        var project = portfolio.CreateProject("Test Project", "Description").Value;

        // Act
        var result = portfolio.ChangeProjectProgram(project.Id, null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("The project is not currently assigned to a program.");
    }


    #endregion Project Management
}
