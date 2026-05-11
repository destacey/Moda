using Microsoft.Extensions.Logging;
using Moq;
using Wayd.Organization.Application.TeamMemberRoles.Commands;
using Wayd.Organization.Application.Tests.Infrastructure;
using Wayd.Organization.Domain.Models;

namespace Wayd.Organization.Application.Tests.Sut.TeamMemberRoles.Commands;

public class ActivateDeactivateTeamMemberRoleCommandHandlerTests : IDisposable
{
    private readonly FakeOrganizationDbContext _dbContext;
    private readonly ActivateTeamMemberRoleCommandHandler _activateHandler;
    private readonly DeactivateTeamMemberRoleCommandHandler _deactivateHandler;

    public ActivateDeactivateTeamMemberRoleCommandHandlerTests()
    {
        _dbContext = new FakeOrganizationDbContext();

        _activateHandler = new ActivateTeamMemberRoleCommandHandler(
            _dbContext,
            new Mock<ILogger<ActivateTeamMemberRoleCommandHandler>>().Object);

        _deactivateHandler = new DeactivateTeamMemberRoleCommandHandler(
            _dbContext,
            new Mock<ILogger<DeactivateTeamMemberRoleCommandHandler>>().Object);
    }

    [Fact]
    public async Task Deactivate_ShouldDeactivateRole_WhenActive()
    {
        // Arrange
        var role = TeamMemberRole.Create("Tech Lead", "Tech Lead role").Value;
        role.IsActive.Should().BeTrue();
        _dbContext.AddTeamMemberRole(role);

        var command = new DeactivateTeamMemberRoleCommand(role.Id);

        // Act
        var result = await _deactivateHandler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        role.IsActive.Should().BeFalse();
        _dbContext.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task Activate_ShouldActivateRole_WhenInactive()
    {
        // Arrange
        var role = TeamMemberRole.Create("Tech Lead", "Tech Lead role").Value;
        role.Deactivate();
        role.IsActive.Should().BeFalse();
        _dbContext.AddTeamMemberRole(role);

        var command = new ActivateTeamMemberRoleCommand(role.Id);

        // Act
        var result = await _activateHandler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        role.IsActive.Should().BeTrue();
        _dbContext.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task Deactivate_ShouldFail_WhenRoleNotFound()
    {
        // Arrange
        var command = new DeactivateTeamMemberRoleCommand(Guid.NewGuid());

        // Act
        var result = await _deactivateHandler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        _dbContext.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Activate_ShouldFail_WhenRoleNotFound()
    {
        // Arrange
        var command = new ActivateTeamMemberRoleCommand(Guid.NewGuid());

        // Act
        var result = await _activateHandler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        _dbContext.SaveChangesCallCount.Should().Be(0);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }
}
