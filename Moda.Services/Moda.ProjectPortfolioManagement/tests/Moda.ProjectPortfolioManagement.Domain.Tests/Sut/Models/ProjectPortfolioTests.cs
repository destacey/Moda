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
    private readonly ProjectPortfolioFaker _faker;

    public ProjectPortfolioTests()
    {
        _dateTimeProvider = new(new FakeClock(DateTime.UtcNow.ToInstant()));
        _faker = new ProjectPortfolioFaker();
    }

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
    }

    #region Lifecycle

    [Fact]
    public void Activate_ShouldActivateProposedPortfolioSuccessfully()
    {
        // Arrange
        var portfolio = _faker.Generate();
        var startDate = _dateTimeProvider.Today;

        // Act
        var result = portfolio.Activate(startDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        portfolio.Status.Should().Be(ProjectPortfolioStatus.Active);
        portfolio.DateRange.Should().NotBeNull();
        portfolio.DateRange!.Start.Should().Be(startDate);
        portfolio.DateRange.End.Should().BeNull();
    }

    [Fact]
    public void Activate_ShouldFail_WhenPortfolioIsNotProposed()
    {
        // Arrange
        var portfolio = _faker.ActivePortfolio(_dateTimeProvider);
        var startDate = _dateTimeProvider.Today;

        // Act
        var result = portfolio.Activate(startDate);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Only proposed portfolios can be activated.");
    }

    [Fact]
    public void Complete_ShouldCompleteActivePortfolioSuccessfully()
    {
        // Arrange
        var portfolio = _faker.ActivePortfolio(_dateTimeProvider);
        var endDate = _dateTimeProvider.Today.PlusDays(10);

        // Act
        var result = portfolio.Complete(endDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        portfolio.Status.Should().Be(ProjectPortfolioStatus.Completed);
        portfolio.DateRange.Should().NotBeNull();
        portfolio.DateRange!.End.Should().Be(endDate);
    }

    [Fact]
    public void Complete_ShouldFail_WhenPortfolioIsNotActive()
    {
        // Arrange
        var portfolio = _faker.WithData(status: ProjectPortfolioStatus.Proposed).Generate();
        var endDate = _dateTimeProvider.Today.PlusDays(10);

        // Act
        var result = portfolio.Complete(endDate);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Only active portfolios can be completed.");
    }

    [Fact]
    public void Complete_ShouldFail_WhenEndDateIsBeforeStartDate()
    {
        // Arrange
        var portfolio = _faker.ActivePortfolio(_dateTimeProvider);
        var endDate = portfolio.DateRange!.Start.PlusDays(-1);

        // Act
        var result = portfolio.Complete(endDate);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("The end date cannot be earlier than the start date.");
    }

    [Fact]
    public void Archive_ShouldArchiveCompletedPortfolioSuccessfully()
    {
        // Arrange
        var portfolio = _faker.CompletedPortfolio(_dateTimeProvider);

        // Act
        var result = portfolio.Archive();

        // Assert
        result.IsSuccess.Should().BeTrue();
        portfolio.Status.Should().Be(ProjectPortfolioStatus.Archived);
    }

    [Fact]
    public void Archive_ShouldFail_WhenPortfolioIsNotCompleted()
    {
        // Arrange
        var portfolio = _faker.ActivePortfolio(_dateTimeProvider);

        // Act
        var result = portfolio.Archive();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Only completed portfolios can be archived.");
    }

    [Fact]
    public void Pause_ShouldPauseActivePortfolioSuccessfully()
    {
        // Arrange
        var portfolio = _faker.ActivePortfolio(_dateTimeProvider);

        // Act
        var result = portfolio.Pause();

        // Assert
        result.IsSuccess.Should().BeTrue();
        portfolio.Status.Should().Be(ProjectPortfolioStatus.OnHold);
    }

    [Fact]
    public void Pause_ShouldFail_WhenPortfolioIsNotActive()
    {
        // Arrange
        var portfolio = _faker.WithData(status: ProjectPortfolioStatus.Proposed).Generate();

        // Act
        var result = portfolio.Pause();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Only active portfolios can be put on hold.");
    }

    [Fact]
    public void Resume_ShouldResumeOnHoldPortfolioSuccessfully()
    {
        // Arrange
        var portfolio = _faker.WithData(status: ProjectPortfolioStatus.OnHold).Generate();

        // Act
        var result = portfolio.Resume();

        // Assert
        result.IsSuccess.Should().BeTrue();
        portfolio.Status.Should().Be(ProjectPortfolioStatus.Active);
    }

    [Fact]
    public void Resume_ShouldFail_WhenPortfolioIsNotOnHold()
    {
        // Arrange
        var portfolio = _faker.ActivePortfolio(_dateTimeProvider);

        // Act
        var result = portfolio.Resume();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Only portfolios on hold can be resumed.");
    }

    #endregion Lifecycle

    #region Projects

    [Fact]
    public void CreateProject_ShouldCreateProjectSuccessfully_WhenPortfolioIsActive()
    {
        // Arrange
        var portfolio = _faker.ActivePortfolio(_dateTimeProvider);

        // Act
        var result = portfolio.CreateProject("Test Project", "Test Description");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Test Project");
        result.Value.Description.Should().Be("Test Description");
        result.Value.PortfolioId.Should().Be(portfolio.Id);
        portfolio.Projects.Should().Contain(result.Value);
    }

    [Fact]
    public void CreateProject_ShouldFail_WhenPortfolioIsNotActive()
    {
        // Arrange
        var portfolio = _faker.ProposedPortfolio();

        // Act
        var result = portfolio.CreateProject("Test Project", "Test Description");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Projects can only be created when the portfolio is active.");
        portfolio.Projects.Should().BeEmpty();
    }

    [Fact]
    public void CreateProject_ShouldFail_WhenProjectNameIsEmpty()
    {
        // Arrange
        var portfolio = _faker.ActivePortfolio(_dateTimeProvider);

        // Act
        Action action = () => portfolio.CreateProject(string.Empty, "Test Description");

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("Required input name was empty. (Parameter 'Name')");
        portfolio.Projects.Should().BeEmpty();
    }

    [Fact]
    public void CreateProject_ShouldFail_WhenProjectDescriptionIsEmpty()
    {
        // Arrange
        var portfolio = _faker.ActivePortfolio(_dateTimeProvider);

        // Act
        Action action = () => portfolio.CreateProject("Test Project", string.Empty);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("Required input description was empty. (Parameter 'Description')");
        portfolio.Projects.Should().BeEmpty();
    }

    #endregion Projects
}
