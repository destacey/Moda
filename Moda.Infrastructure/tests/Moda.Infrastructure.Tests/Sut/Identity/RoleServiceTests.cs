using Microsoft.AspNetCore.Identity;
using Moda.Common.Application.Events;
using Moda.Common.Application.Exceptions;
using Moda.Common.Application.Identity.Roles;
using Moda.Common.Application.Interfaces;
using Moda.Common.Domain.Authorization;
using Moda.Common.Domain.Identity;
using Moda.Infrastructure.Identity;
using Moda.Tests.Shared;
using NotFoundException = Moda.Common.Application.Exceptions.NotFoundException;

namespace Moda.Infrastructure.Tests.Sut.Identity;

public class RoleServiceTests
{
    private readonly Mock<RoleManager<ApplicationRole>> _mockRoleManager;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<IEventPublisher> _mockEvents;
    private readonly Mock<ICurrentUser> _mockCurrentUser;
    private readonly TestingDateTimeProvider _dateTimeProvider;

    public RoleServiceTests()
    {
        var roleStore = new Mock<IRoleStore<ApplicationRole>>();
        _mockRoleManager = new Mock<RoleManager<ApplicationRole>>(
            roleStore.Object, null!, null!, null!, null!);

        var userStore = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _mockEvents = new Mock<IEventPublisher>();
        _mockCurrentUser = new Mock<ICurrentUser>();
        _dateTimeProvider = new TestingDateTimeProvider(DateTime.UtcNow);
    }

    private RoleService CreateSut()
    {
        return new RoleService(
            _mockRoleManager.Object,
            _mockUserManager.Object,
            null!, // ModaDbContext - not used by Create/Delete methods
            _mockCurrentUser.Object,
            _mockEvents.Object,
            _dateTimeProvider);
    }

    #region CreateOrUpdate - Create

    [Fact]
    public async Task CreateOrUpdate_ShouldCreateRole_WhenIdIsEmpty()
    {
        // Arrange
        var command = new CreateOrUpdateRoleCommand(null, "NewRole", "A new role");
        _mockRoleManager
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Success);

        var sut = CreateSut();

        // Act
        var result = await sut.CreateOrUpdate(command);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        _mockRoleManager.Verify(x => x.CreateAsync(It.Is<ApplicationRole>(r =>
            r.Name == "NewRole" && r.Description == "A new role")), Times.Once);
        _mockEvents.Verify(x => x.PublishAsync(It.IsAny<ApplicationRoleCreatedEvent>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrUpdate_ShouldThrow_WhenCreateFails()
    {
        // Arrange
        var command = new CreateOrUpdateRoleCommand(null, "NewRole", null);
        _mockRoleManager
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Failed" }));

        var sut = CreateSut();

        // Act
        var act = () => sut.CreateOrUpdate(command);

        // Assert
        await act.Should().ThrowAsync<InternalServerException>();
    }

    [Fact]
    public async Task CreateOrUpdate_ShouldThrowValidation_WhenDuplicateRoleName()
    {
        // Arrange
        var command = new CreateOrUpdateRoleCommand(null, "Admin", null);
        _mockRoleManager
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError
            {
                Code = "DuplicateRoleName",
                Description = "Role name 'Admin' is already taken."
            }));

        var sut = CreateSut();

        // Act
        var act = () => sut.CreateOrUpdate(command);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region CreateOrUpdate - Update

    [Fact]
    public async Task CreateOrUpdate_ShouldUpdateRole_WhenIdIsProvided()
    {
        // Arrange
        var existingRole = new ApplicationRole("OldName", "Old description");
        var command = new CreateOrUpdateRoleCommand("role-1", "UpdatedName", "Updated description");

        _mockRoleManager.Setup(x => x.FindByIdAsync("role-1")).ReturnsAsync(existingRole);
        _mockRoleManager.Setup(x => x.UpdateAsync(existingRole)).ReturnsAsync(IdentityResult.Success);

        var sut = CreateSut();

        // Act
        var result = await sut.CreateOrUpdate(command);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        existingRole.Name.Should().Be("UpdatedName");
        existingRole.Description.Should().Be("Updated description");
        _mockRoleManager.Verify(x => x.UpdateAsync(existingRole), Times.Once);
        _mockEvents.Verify(x => x.PublishAsync(It.IsAny<ApplicationRoleUpdatedEvent>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrUpdate_ShouldThrowNotFound_WhenRoleDoesNotExist()
    {
        // Arrange
        _mockRoleManager.Setup(x => x.FindByIdAsync("missing")).ReturnsAsync((ApplicationRole?)null);
        var command = new CreateOrUpdateRoleCommand("missing", "Name", null);

        var sut = CreateSut();

        // Act
        var act = () => sut.CreateOrUpdate(command);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateOrUpdate_ShouldThrowConflict_WhenUpdatingDefaultRole()
    {
        // Arrange
        var adminRole = new ApplicationRole(ApplicationRoles.Admin);
        _mockRoleManager.Setup(x => x.FindByIdAsync("admin-id")).ReturnsAsync(adminRole);

        var command = new CreateOrUpdateRoleCommand("admin-id", "RenamedAdmin", null);
        var sut = CreateSut();

        // Act
        var act = () => sut.CreateOrUpdate(command);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*Not allowed to modify*Admin*");
    }

    #endregion

    #region Delete

    [Fact]
    public async Task Delete_ShouldDeleteRole_WhenRoleExistsAndIsNotDefault()
    {
        // Arrange
        var role = new ApplicationRole("CustomRole", "A custom role");
        _mockRoleManager.Setup(x => x.FindByIdAsync("role-1")).ReturnsAsync(role);
        _mockUserManager.Setup(x => x.GetUsersInRoleAsync("CustomRole")).ReturnsAsync([]);
        _mockRoleManager.Setup(x => x.DeleteAsync(role)).ReturnsAsync(IdentityResult.Success);

        var sut = CreateSut();

        // Act
        var result = await sut.Delete("role-1");

        // Assert
        result.Should().Contain("CustomRole").And.Contain("Deleted");
        _mockRoleManager.Verify(x => x.DeleteAsync(role), Times.Once);
        _mockEvents.Verify(x => x.PublishAsync(It.IsAny<ApplicationRoleDeletedEvent>()), Times.Once);
    }

    [Fact]
    public async Task Delete_ShouldThrowNotFound_WhenRoleDoesNotExist()
    {
        // Arrange
        _mockRoleManager.Setup(x => x.FindByIdAsync("missing")).ReturnsAsync((ApplicationRole?)null);

        var sut = CreateSut();

        // Act
        var act = () => sut.Delete("missing");

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Delete_ShouldThrowConflict_WhenDeletingDefaultRole()
    {
        // Arrange
        var basicRole = new ApplicationRole(ApplicationRoles.Basic);
        _mockRoleManager.Setup(x => x.FindByIdAsync("basic-id")).ReturnsAsync(basicRole);

        var sut = CreateSut();

        // Act
        var act = () => sut.Delete("basic-id");

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*Not allowed to delete*Basic*");
    }

    [Fact]
    public async Task Delete_ShouldThrowConflict_WhenRoleHasUsers()
    {
        // Arrange
        var role = new ApplicationRole("CustomRole");
        _mockRoleManager.Setup(x => x.FindByIdAsync("role-1")).ReturnsAsync(role);
        _mockUserManager.Setup(x => x.GetUsersInRoleAsync("CustomRole"))
            .ReturnsAsync([new() { Id = "user-1" }]);

        var sut = CreateSut();

        // Act
        var act = () => sut.Delete("role-1");

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*currently assigned to users*");
    }

    #endregion
}
