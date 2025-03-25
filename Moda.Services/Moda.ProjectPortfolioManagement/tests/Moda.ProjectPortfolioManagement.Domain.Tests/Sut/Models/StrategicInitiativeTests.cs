using FluentAssertions;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.ProjectPortfolioManagement.Domain.Models;
using Moda.ProjectPortfolioManagement.Domain.Tests.Data;
using Moda.Tests.Shared;
using NodaTime.Extensions;
using NodaTime.Testing;

namespace Moda.ProjectPortfolioManagement.Domain.Tests.Sut.Models;

public sealed class StrategicInitiativeTests
{
    private readonly TestingDateTimeProvider _dateTimeProvider;
    private readonly StrategicInitiativeFaker _strategicInitiativeFaker;

    public StrategicInitiativeTests()
    {
        _dateTimeProvider = new TestingDateTimeProvider(new FakeClock(DateTime.UtcNow.ToInstant()));
        _strategicInitiativeFaker = new StrategicInitiativeFaker(_dateTimeProvider);
    }

    [Fact]
    public void Create_ShouldCreateStrategicInitiativeSuccessfully()
    {
        // Arrange
        var expected = _strategicInitiativeFaker.Generate();

        // Act
        var initiative = StrategicInitiative.Create(expected.Name, expected.Description, expected.DateRange, expected.PortfolioId);

        // Assert
        initiative.Should().NotBeNull();
        initiative.Name.Should().Be(expected.Name);
        initiative.Description.Should().Be(expected.Description);
        initiative.Status.Should().Be(StrategicInitiativeStatus.Proposed);
        initiative.DateRange.Should().Be(expected.DateRange);
        initiative.PortfolioId.Should().Be(expected.PortfolioId);
        initiative.Roles.Should().BeEmpty();
        initiative.Kpis.Should().BeEmpty();
        initiative.StrategicInitiativeProjects.Should().BeEmpty();
    }

    [Theory]
    [InlineData(StrategicInitiativeStatus.Proposed, true)]
    [InlineData(StrategicInitiativeStatus.Approved, true)]
    [InlineData(StrategicInitiativeStatus.Active, false)]
    [InlineData(StrategicInitiativeStatus.Completed, false)]
    [InlineData(StrategicInitiativeStatus.Cancelled, false)]
    public void CanBeDeleted_ShouldReturnExpectedBasedOnStatus(StrategicInitiativeStatus status, bool expected)
    {
        // Arrange
        var initiative = _strategicInitiativeFaker.WithData(status: status).Generate();

        // Act & Assert
        initiative.CanBeDeleted().Should().Be(expected);
    }

    [Theory]
    [InlineData(StrategicInitiativeStatus.Completed, true)]
    [InlineData(StrategicInitiativeStatus.Cancelled, true)]
    [InlineData(StrategicInitiativeStatus.Proposed, false)]
    [InlineData(StrategicInitiativeStatus.Approved, false)]
    [InlineData(StrategicInitiativeStatus.Active, false)]
    public void IsClosed_ShouldReturnExpectedBasedOnStatus(StrategicInitiativeStatus status, bool expected)
    {
        // Arrange
        var initiative = _strategicInitiativeFaker.WithData(status: status).Generate();

        // Act & Assert
        initiative.IsClosed.Should().Be(expected);
    }

    #region Roles

    [Fact]
    public void AssignRole_ShouldAssignEmployeeToRoleSuccessfully()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var initiative = _strategicInitiativeFaker.Generate();

        // Act
        var result = initiative.AssignRole(StrategicInitiativeRole.Owner, employeeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        initiative.Roles.Should().ContainSingle();
        initiative.Roles.First().Role.Should().Be(StrategicInitiativeRole.Owner);
        initiative.Roles.First().EmployeeId.Should().Be(employeeId);
    }

    [Fact]
    public void AssignRole_ShouldFail_WhenEmployeeAlreadyAssignedToRole()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var initiative = _strategicInitiativeFaker.WithData(roles: new Dictionary<StrategicInitiativeRole, HashSet<Guid>>
        {
            { StrategicInitiativeRole.Owner, new HashSet<Guid> { employeeId } }
        }).Generate();

        // Act
        var result = initiative.AssignRole(StrategicInitiativeRole.Owner, employeeId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Employee is already assigned to this role.");
    }

    [Fact]
    public void RemoveRole_WithOneRoleAssignment_ShouldRemoveEmployeeFromRoleSuccessfully()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var initiative = _strategicInitiativeFaker.WithData(roles: new Dictionary<StrategicInitiativeRole, HashSet<Guid>>
        {
            { StrategicInitiativeRole.Owner, new HashSet<Guid> { employeeId } }
        }).Generate();

        // Act
        var result = initiative.RemoveRole(StrategicInitiativeRole.Owner, employeeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        initiative.Roles.Should().BeEmpty();
    }

    [Fact]
    public void RemoveRole_WithMultipleRoleAssignments_ShouldRemoveEmployeeFromRoleSuccessfully()
    {
        // Arrange
        var employeeId1 = Guid.NewGuid();
        var employeeId2 = Guid.NewGuid();
        var initiative = _strategicInitiativeFaker.WithData(roles: new Dictionary<StrategicInitiativeRole, HashSet<Guid>>
        {
            { StrategicInitiativeRole.Owner, new HashSet<Guid> { employeeId1, employeeId2 } }
        }).Generate();

        // Act
        var result = initiative.RemoveRole(StrategicInitiativeRole.Owner, employeeId1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        initiative.Roles.Count.Should().Be(1);
        initiative.Roles.First().Role.Should().Be(StrategicInitiativeRole.Owner);
        initiative.Roles.First().EmployeeId.Should().Be(employeeId2);
    }

    [Fact]
    public void RemoveRole_ShouldFail_WhenEmployeeNotAssignedToRole()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var initiative = _strategicInitiativeFaker.Generate();

        // Act
        var result = initiative.RemoveRole(StrategicInitiativeRole.Owner, employeeId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Employee is not assigned to this role.");
    }


    [Fact]
    public void UpdateRoles_ShouldAssignNewRolesSuccessfully()
    {
        // Arrange
        var initiative = _strategicInitiativeFaker.Generate();
        var employee1 = Guid.NewGuid();
        var employee2 = Guid.NewGuid();
        var updatedRoles = new Dictionary<StrategicInitiativeRole, HashSet<Guid>>
        {
            { StrategicInitiativeRole.Sponsor, new HashSet<Guid> { employee1, employee2 } }
        };

        // Act
        var result = initiative.UpdateRoles(updatedRoles);

        // Assert
        result.IsSuccess.Should().BeTrue();
        initiative.Roles.Should().Contain(role => role.Role == StrategicInitiativeRole.Sponsor && role.EmployeeId == employee1);
        initiative.Roles.Should().Contain(role => role.Role == StrategicInitiativeRole.Sponsor && role.EmployeeId == employee2);
    }

    [Fact]
    public void UpdateRoles_ShouldRemoveUnspecifiedRoles()
    {
        // Arrange
        var initiative = _strategicInitiativeFaker.WithData(roles: new Dictionary<StrategicInitiativeRole, HashSet<Guid>>
        {
            { StrategicInitiativeRole.Sponsor, new HashSet<Guid> { Guid.NewGuid(), Guid.NewGuid() } },
            { StrategicInitiativeRole.Owner, new HashSet<Guid> { Guid.NewGuid() } }
        }).Generate();

        var updatedRoles = new Dictionary<StrategicInitiativeRole, HashSet<Guid>>
        {
            { StrategicInitiativeRole.Sponsor, new HashSet<Guid> { Guid.NewGuid() } }  // Remove Owner role
        };

        // Act
        var result = initiative.UpdateRoles(updatedRoles);

        // Assert
        result.IsSuccess.Should().BeTrue();
        initiative.Roles.Should().Contain(role => role.Role == StrategicInitiativeRole.Sponsor);
        initiative.Roles.Should().NotContain(role => role.Role == StrategicInitiativeRole.Owner); // Removed role
    }

    [Fact]
    public void UpdateRoles_ShouldNotChange_WhenRolesAreUnchanged()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var initiative = _strategicInitiativeFaker.WithData(roles: new Dictionary<StrategicInitiativeRole, HashSet<Guid>>
        {
            { StrategicInitiativeRole.Sponsor, new HashSet<Guid> { employeeId } }
        }).Generate();

        var updatedRoles = new Dictionary<StrategicInitiativeRole, HashSet<Guid>>
        {
            { StrategicInitiativeRole.Sponsor, new HashSet<Guid> { employeeId } }
        };

        // Act
        var result = initiative.UpdateRoles(updatedRoles);

        // Assert
        result.IsSuccess.Should().BeTrue();
        initiative.Roles.Count.Should().Be(1);
        initiative.Roles.Should().Contain(role => role.Role == StrategicInitiativeRole.Sponsor && role.EmployeeId == employeeId);
    }

    [Fact]
    public void UpdateRoles_ShouldFail_WhenInvalidRoleProvided()
    {
        // Arrange
        var initiative = _strategicInitiativeFaker.Generate();
        var invalidRole = (StrategicInitiativeRole)999;
        var updatedRoles = new Dictionary<StrategicInitiativeRole, HashSet<Guid>>
        {
            { invalidRole, new HashSet<Guid> { Guid.NewGuid() } }
        };

        // Act
        var result = initiative.UpdateRoles(updatedRoles);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be($"Role is not a valid {nameof(StrategicInitiativeRole)} value.");
    }

    #endregion Roles

    #region Lifecycle Tests

    [Fact]
    public void Approve_ShouldApproveProposedStrategicInitiativeSuccessfully()
    {
        // Arrange
        var initiative = _strategicInitiativeFaker.AsProposed(_dateTimeProvider);

        // Act
        var result = initiative.Approve();

        // Assert
        result.IsSuccess.Should().BeTrue();
        initiative.Status.Should().Be(StrategicInitiativeStatus.Approved);
    }

    [Fact]
    public void Activate_ShouldActivateApprovedStrategicInitiativeSuccessfully()
    {
        // Arrange
        var initiative = _strategicInitiativeFaker.AsApproved(_dateTimeProvider);

        // Act
        var result = initiative.Activate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        initiative.Status.Should().Be(StrategicInitiativeStatus.Active);
    }

    [Fact]
    public void Activate_ShouldFail_WhenStrategicInitiativeIsNotApproved()
    {
        // Arrange
        var initiative = _strategicInitiativeFaker.AsProposed(_dateTimeProvider);

        // Act
        var result = initiative.Activate();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Only approved strategic initiatives can be activated.");
    }

    [Fact]
    public void Complete_ShouldCompleteActiveStrategicInitiativeSuccessfully()
    {
        // Arrange
        var initiative = _strategicInitiativeFaker.AsActive(_dateTimeProvider);

        // Act
        var result = initiative.Complete();

        // Assert
        result.IsSuccess.Should().BeTrue();
        initiative.Status.Should().Be(StrategicInitiativeStatus.Completed);
    }

    [Fact]
    public void Complete_ShouldFail_WhenStrategicInitiativeIsNotActive()
    {
        // Arrange
        var initiative = _strategicInitiativeFaker.Generate();

        // Act
        var result = initiative.Complete();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Only active strategic initiatives can be completed.");
    }

    [Fact]
    public void Cancel_ShouldCancelActiveStrategicInitiativeSuccessfully()
    {
        // Arrange
        var initiative = _strategicInitiativeFaker.AsActive(_dateTimeProvider);

        // Act
        var result = initiative.Cancel();

        // Assert
        result.IsSuccess.Should().BeTrue();
        initiative.Status.Should().Be(StrategicInitiativeStatus.Cancelled);
    }

    [Fact]
    public void Cancel_ShouldFail_WhenStrategicInitiativeIsAlreadyCompletedOrCancelled()
    {
        // Arrange
        var initiative = _strategicInitiativeFaker.AsCancelled(_dateTimeProvider);

        // Act
        var result = initiative.Cancel();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("The strategic initiative is already completed or cancelled.");
    }

    #endregion Lifecycle Tests
}
