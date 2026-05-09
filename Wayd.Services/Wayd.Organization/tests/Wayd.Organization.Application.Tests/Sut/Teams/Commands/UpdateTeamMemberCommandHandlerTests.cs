using Microsoft.Extensions.Logging;
using Moq;
using Wayd.Common.Domain.Tests.Data;
using Wayd.Organization.Application.Teams.Commands;
using Wayd.Organization.Application.Tests.Infrastructure;
using Wayd.Organization.Domain.Tests.Data;

namespace Wayd.Organization.Application.Tests.Sut.Teams.Commands;

public class UpdateTeamMemberCommandHandlerTests : IDisposable
{
    private readonly TeamFaker _teamFaker;
    private readonly EmployeeFaker _employeeFaker;
    private readonly FakeOrganizationDbContext _dbContext;
    private readonly UpdateTeamMemberCommandHandler _handler;

    public UpdateTeamMemberCommandHandlerTests()
    {
        _teamFaker = new TeamFaker();
        _employeeFaker = new EmployeeFaker();
        _dbContext = new FakeOrganizationDbContext();

        _handler = new UpdateTeamMemberCommandHandler(
            _dbContext,
            new Mock<ILogger<UpdateTeamMemberCommandHandler>>().Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateRole_WhenMemberExists()
    {
        // Arrange
        var team = _teamFaker.Generate();
        var employee = _employeeFaker.Generate();
        _dbContext.AddTeam(team);

        var addResult = team.AddMember(employee, Guid.NewGuid());
        addResult.IsSuccess.Should().BeTrue();
        var memberId = addResult.Value.Id;

        var newRoleId = Guid.NewGuid();
        var command = new UpdateTeamMemberCommand(team.Id, memberId, newRoleId);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        team.Members.Single().RoleId.Should().Be(newRoleId);
        _dbContext.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenTeamNotFound()
    {
        // Arrange
        var command = new UpdateTeamMemberCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        _dbContext.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenMemberNotFound()
    {
        // Arrange
        var team = _teamFaker.Generate();
        _dbContext.AddTeam(team);

        var command = new UpdateTeamMemberCommand(team.Id, Guid.NewGuid(), Guid.NewGuid());

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
