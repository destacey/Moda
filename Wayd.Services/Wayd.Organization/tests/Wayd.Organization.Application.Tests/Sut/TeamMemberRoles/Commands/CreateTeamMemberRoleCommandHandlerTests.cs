using Microsoft.Extensions.Logging;
using Moq;
using Wayd.Organization.Application.TeamMemberRoles.Commands;
using Wayd.Organization.Application.Tests.Infrastructure;

namespace Wayd.Organization.Application.Tests.Sut.TeamMemberRoles.Commands;

public class CreateTeamMemberRoleCommandHandlerTests : IDisposable
{
    private readonly FakeOrganizationDbContext _dbContext;
    private readonly CreateTeamMemberRoleCommandHandler _handler;

    public CreateTeamMemberRoleCommandHandlerTests()
    {
        _dbContext = new FakeOrganizationDbContext();

        _handler = new CreateTeamMemberRoleCommandHandler(
            _dbContext,
            new Mock<ILogger<CreateTeamMemberRoleCommandHandler>>().Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateRole_WithValidName()
    {
        // Arrange
        var command = new CreateTeamMemberRoleCommand("Tech Lead");

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _dbContext.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnNewId_OnSuccess()
    {
        // Arrange
        var command = new CreateTeamMemberRoleCommand("Engineer");

        // Act
        var result1 = await _handler.Handle(command, TestContext.Current.CancellationToken);
        var result2 = await _handler.Handle(new CreateTeamMemberRoleCommand("Manager"), TestContext.Current.CancellationToken);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Value.Should().NotBe(result2.Value);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
