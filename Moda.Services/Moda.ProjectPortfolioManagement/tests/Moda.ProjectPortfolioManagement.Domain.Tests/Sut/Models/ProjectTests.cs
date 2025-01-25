using FluentAssertions;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.ProjectPortfolioManagement.Domain.Models;
using Moda.ProjectPortfolioManagement.Domain.Tests.Data;
using Moda.Tests.Shared;
using NodaTime.Extensions;
using NodaTime.Testing;

namespace Moda.ProjectPortfolioManagement.Domain.Tests.Sut.Models;
public sealed class ProjectTests
{
    private readonly TestingDateTimeProvider _dateTimeProvider;
    private readonly ProjectFaker _faker;

    public ProjectTests()
    {
        _dateTimeProvider = new(new FakeClock(DateTime.UtcNow.ToInstant()));
        _faker = new ProjectFaker();
    }

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
        project.DateRange.Should().BeNull();
    }

    [Fact]
    public void Activate_ShouldActivateProposedProjectSuccessfully()
    {
        // Arrange
        var project = _faker.Generate();
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
        var project = _faker.ActiveProject(_dateTimeProvider);
        var startDate = _dateTimeProvider.Today;

        // Act
        var result = project.Activate(startDate);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Only proposed projects can be activated.");
    }

    [Fact]
    public void Complete_ShouldCompleteActiveProjectSuccessfully()
    {
        // Arrange
        var project = _faker.ActiveProject(_dateTimeProvider);
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
        var project = _faker.WithData(status: ProjectStatus.Proposed).Generate();
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
        var project = _faker.ActiveProject(_dateTimeProvider);
        var endDate = project.DateRange!.Start.PlusDays(-1);

        // Act
        var result = project.Complete(endDate);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("The end date cannot be earlier than the start date.");
    }

    [Fact]
    public void Cancel_ShouldCancelProjectSuccessfully()
    {
        // Arrange
        var project = _faker.ActiveProject(_dateTimeProvider);
        var endDate = _dateTimeProvider.Today.PlusDays(5);

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
        var project = _faker.CompletedProject(_dateTimeProvider);
        var endDate = _dateTimeProvider.Today.PlusDays(5);

        // Act
        var result = project.Cancel(endDate);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("The project is already completed or cancelled.");
    }
}
