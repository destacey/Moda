using Microsoft.Extensions.Logging;
using Moq;
using Wayd.Organization.Application.TeamMemberRoles.Commands;
using Wayd.Organization.Application.Tests.Infrastructure;
using Wayd.Organization.Domain.Models;

namespace Wayd.Organization.Application.Tests.Sut.TeamMemberRoles.Commands;

public class UpdateTeamMemberRoleCommandHandlerTests : IDisposable
{
    private readonly FakeOrganizationDbContext _dbContext;
    private readonly UpdateTeamMemberRoleCommandHandler _handler;

    public UpdateTeamMemberRoleCommandHandlerTests()
    {
        _dbContext = new FakeOrganizationDbContext();

        _handler = new UpdateTeamMemberRoleCommandHandler(
            _dbContext,
            new Mock<ILogger<UpdateTeamMemberRoleCommandHandler>>().Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateName_WhenRoleExists()
    {
        // Arrange
        var role = TeamMemberRole.Create("Tech Lead").Value;
        _dbContext.AddTeamMemberRole(role);

        var command = new UpdateTeamMemberRoleCommand(role.Id, "Senior Tech Lead");

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        role.Name.Should().Be("Senior Tech Lead");
        _dbContext.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenRoleNotFound()
    {
        // Arrange
        var command = new UpdateTeamMemberRoleCommand(Guid.NewGuid(), "Engineer");

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        _dbContext.SaveChangesCallCount.Should().Be(0);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
