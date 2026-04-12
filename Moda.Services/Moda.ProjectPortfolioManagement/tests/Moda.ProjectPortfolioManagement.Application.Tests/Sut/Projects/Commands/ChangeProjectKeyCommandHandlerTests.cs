using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moda.Common.Application.Interfaces;
using Moda.Common.Domain.Models.ProjectPortfolioManagement;
using Moda.ProjectPortfolioManagement.Application.Projects.Commands;
using Moda.ProjectPortfolioManagement.Application.Tests.Infrastructure;
using Moda.ProjectPortfolioManagement.Domain.Tests.Data;
using Moda.Tests.Shared;
using Moq;

namespace Moda.ProjectPortfolioManagement.Application.Tests.Sut.Projects.Commands;

public class ChangeProjectKeyCommandHandlerTests : IDisposable
{
    private readonly FakeProjectPortfolioManagementDbContext _dbContext;
    private readonly ChangeProjectKeyCommandHandler _handler;
    private readonly Mock<ILogger<ChangeProjectKeyCommandHandler>> _mockLogger;
    private readonly IDateTimeProvider _dateTimeProvider;

    private readonly ProjectFaker _projectFaker;

    public ChangeProjectKeyCommandHandlerTests()
    {
        _dbContext = new FakeProjectPortfolioManagementDbContext();
        _mockLogger = new Mock<ILogger<ChangeProjectKeyCommandHandler>>();
        _dateTimeProvider = new TestingDateTimeProvider(new DateTime(2025, 3, 3));

        _handler = new ChangeProjectKeyCommandHandler(_dbContext, _mockLogger.Object, _dateTimeProvider);

        _projectFaker = new ProjectFaker();
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenProjectDoesNotExist()
    {
        // Arrange
        var command = new ChangeProjectKeyCommand(Guid.NewGuid(), new ProjectKey("NEWKEY"));

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Project not found.");
        _dbContext.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldChangeProjectKey_AndCascadeToTasks_WhenProjectExists()
    {
        // Arrange
        var project = _projectFaker.Generate();

        // Set up tasks using faker (need real IDs for key cascade verification)
        var tasks = project.WithTasks(2);

        _dbContext.AddProject(project);

        var command = new ChangeProjectKeyCommand(project.Id, new ProjectKey("CHANGED"));

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.Key.Should().Be(new ProjectKey("CHANGED"));
        project.Tasks.Should().HaveCount(2);
        project.Tasks.Select(t => t.Key.Value).Should().BeEquivalentTo(["CHANGED-1", "CHANGED-2"]);
        _dbContext.SaveChangesCallCount.Should().Be(1);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
