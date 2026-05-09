using Microsoft.Extensions.Logging;
using Moq;
using Wayd.Common.Domain.Tests.Data;
using Wayd.Organization.Application.Teams.Commands;
using Wayd.Organization.Application.Tests.Infrastructure;
using Wayd.Organization.Domain.Tests.Data;

namespace Wayd.Organization.Application.Tests.Sut.Teams.Commands;

public class RemoveTeamMemberCommandHandlerTests : IDisposable
{
    private readonly TeamFaker _teamFaker;
    private readonly EmployeeFaker _employeeFaker;
    private readonly FakeOrganizationDbContext _dbContext;
    private readonly RemoveTeamMemberCommandHandler _handler;

    public RemoveTeamMemberCommandHandlerTests()
    {
        _teamFaker = new TeamFaker();
        _employeeFaker = new EmployeeFaker();
        _dbContext = new FakeOrganizationDbContext();

        _handler = new RemoveTeamMemberCommandHandler(
            _dbContext,
            new Mock<ILogger<RemoveTeamMemberCommandHandler>>().Object);
    }

    [Fact]
    public async Task Handle_ShouldRemoveMember_WhenMemberExists()
    {
        // Arrange
        var team = _teamFaker.Generate();
        var employee = _employeeFaker.Generate();
        _dbContext.AddTeam(team);

        var addResult = team.AddMember(employee, Guid.NewGuid());
        addResult.IsSuccess.Should().BeTrue();
        var memberId = addResult.Value.Id;

        team.Members.Should().HaveCount(1);

        var command = new RemoveTeamMemberCommand(team.Id, memberId);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        team.Members.Should().BeEmpty();
        _dbContext.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldOnlyRemoveTargetMember_WhenEmployeeHasMultipleRoles()
    {
        // Arrange
        var team = _teamFaker.Generate();
        var employee = _employeeFaker.Generate();
        var roleId1 = Guid.NewGuid();
        var roleId2 = Guid.NewGuid();
        _dbContext.AddTeam(team);

        var member1 = team.AddMember(employee, roleId1).Value;
        var member2 = team.AddMember(employee, roleId2).Value;

        team.Members.Should().HaveCount(2);

        var command = new RemoveTeamMemberCommand(team.Id, member1.Id);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        team.Members.Should().HaveCount(1);
        team.Members.Single().Id.Should().Be(member2.Id);
        _dbContext.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenTeamNotFound()
    {
        // Arrange
        var command = new RemoveTeamMemberCommand(Guid.NewGuid(), Guid.NewGuid());

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

        var command = new RemoveTeamMemberCommand(team.Id, Guid.NewGuid());

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
