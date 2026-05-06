using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Wayd.Common.Application.Events;
using Wayd.Common.Application.Exceptions;
using Wayd.Common.Application.Identity;
using Wayd.Common.Application.Identity.Users;
using Wayd.Common.Application.Interfaces;
using Wayd.Common.Domain.Authorization;
using Wayd.Common.Domain.Identity;
using Wayd.Infrastructure.Identity;
using Wayd.Tests.Shared;
using NotFoundException = Wayd.Common.Application.Exceptions.NotFoundException;

namespace Wayd.Infrastructure.Tests.Sut.Identity;

public class UserServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
    private readonly Mock<RoleManager<ApplicationRole>> _mockRoleManager;
    private readonly Mock<IEventPublisher> _mockEvents;
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly TestingDateTimeProvider _dateTimeProvider;
    private readonly Mock<ISender> _mockSender;
    private readonly Mock<ICurrentUser> _mockCurrentUser;
    private readonly Mock<IUserIdentityStore> _mockUserIdentityStore;

    // UserService depends on WaydDbContext and GraphServiceClient which are hard to
    // mock. We test methods that don't require them. UserIdentity writes go through
    // IUserIdentityStore so they can be verified via Moq.

    public UserServiceTests()
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
            _mockUserManager.Object,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>().Object,
            null!, null!, null!, null!);

        var roleStore = new Mock<IRoleStore<ApplicationRole>>();
        _mockRoleManager = new Mock<RoleManager<ApplicationRole>>(
            roleStore.Object, null!, null!, null!, null!);

        _mockEvents = new Mock<IEventPublisher>();
        _mockLogger = new Mock<ILogger<UserService>>();
        _dateTimeProvider = new TestingDateTimeProvider(DateTime.UtcNow);
        _mockSender = new Mock<ISender>();
        _mockCurrentUser = new Mock<ICurrentUser>();
        _mockCurrentUser.Setup(x => x.GetUserId()).Returns("current-user-id");
        _mockUserIdentityStore = new Mock<IUserIdentityStore>();

        // Pass-through: tests don't exercise transaction semantics. Invoke the
        // action directly so CreateAsync behaves as if the transaction succeeded.
        _mockUserIdentityStore
            .Setup(s => s.ExecuteInTransaction(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task>, CancellationToken>((action, ct) => action(ct));
    }

    private UserService CreateSut()
    {
        return new UserService(
            _mockLogger.Object,
            _mockSignInManager.Object,
            _mockUserManager.Object,
            _mockRoleManager.Object,
            null!, // WaydDbContext - not used by these methods
            _mockEvents.Object,
            null!, // GraphServiceClient - not used by these methods
            _mockSender.Object,
            _dateTimeProvider,
            _mockCurrentUser.Object,
            _mockUserIdentityStore.Object);
    }

    private static ApplicationUser CreateUser(string id = "user-1", string userName = "testuser", bool isActive = true, string loginProvider = LoginProviders.MicrosoftEntraId)
    {
        return new ApplicationUser
        {
            Id = id,
            UserName = userName,
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            IsActive = isActive,
            LoginProvider = loginProvider,
        };
    }

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_ShouldSucceed_WhenLocalUserWithPassword()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            LoginProvider = LoginProviders.Wayd,
            Password = "Password123!",
        };

        _mockUserManager
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "Password123!"))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager
            .Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), ApplicationRoles.Basic))
            .ReturnsAsync(IdentityResult.Success);

        var sut = CreateSut();

        // Act
        var result = await sut.CreateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNullOrWhiteSpace();
        _mockUserManager.Verify(x => x.CreateAsync(It.Is<ApplicationUser>(u =>
            u.FirstName == "John" &&
            u.LastName == "Doe" &&
            u.Email == "john@example.com" &&
            u.UserName == "john@example.com" &&
            u.LoginProvider == LoginProviders.Wayd &&
            u.IsActive), "Password123!"), Times.Once);
        _mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), ApplicationRoles.Basic), Times.Once);
        _mockEvents.Verify(x => x.PublishAsync(It.IsAny<ApplicationUserCreatedEvent>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldSucceed_WhenEntraIdUserWithoutPassword()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com",
            LoginProvider = LoginProviders.MicrosoftEntraId,
        };

        _mockUserManager
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager
            .Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), ApplicationRoles.Basic))
            .ReturnsAsync(IdentityResult.Success);

        var sut = CreateSut();

        // Act
        var result = await sut.CreateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockUserManager.Verify(x => x.CreateAsync(It.Is<ApplicationUser>(u =>
            u.LoginProvider == LoginProviders.MicrosoftEntraId)), Times.Once);
        // Should NOT call the password overload
        _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnFailure_WhenIdentityResultFails()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            LoginProvider = LoginProviders.Wayd,
            Password = "Password123!",
        };

        _mockUserManager
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "Password123!"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Duplicate username." }));

        var sut = CreateSut();

        // Act
        var result = await sut.CreateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Duplicate username.");
        _mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        _mockEvents.Verify(x => x.PublishAsync(It.IsAny<ApplicationUserCreatedEvent>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldWriteActiveWaydIdentity_WhenLocalUserCreated()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            LoginProvider = LoginProviders.Wayd,
            Password = "Password123!",
        };

        _mockUserManager
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "Password123!"))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager
            .Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), ApplicationRoles.Basic))
            .ReturnsAsync(IdentityResult.Success);

        var sut = CreateSut();

        // Act
        var result = await sut.CreateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockUserIdentityStore.Verify(s => s.Add(
            It.Is<UserIdentity>(ui =>
                ui.UserId == result.Value &&
                ui.Provider == LoginProviders.Wayd &&
                ui.ProviderTenantId == null &&
                ui.ProviderSubject == result.Value &&
                ui.IsActive &&
                ui.UnlinkedAt == null),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldNotWriteIdentity_WhenEntraAdminProvisionedUser()
    {
        // Entra users created by an admin have no oid/tid yet — the identity row
        // is only written on their first SSO login via the principal flow.
        var command = new CreateUserCommand
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com",
            LoginProvider = LoginProviders.MicrosoftEntraId,
        };

        _mockUserManager
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager
            .Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), ApplicationRoles.Basic))
            .ReturnsAsync(IdentityResult.Success);

        var sut = CreateSut();

        await sut.CreateAsync(command, TestContext.Current.CancellationToken);

        _mockUserIdentityStore.Verify(s => s.Add(It.IsAny<UserIdentity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldNotWriteIdentity_WhenUserManagerCreateFails()
    {
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            LoginProvider = LoginProviders.Wayd,
            Password = "Password123!",
        };

        _mockUserManager
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "Password123!"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Duplicate." }));

        var sut = CreateSut();

        var result = await sut.CreateAsync(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        _mockUserIdentityStore.Verify(s => s.Add(It.IsAny<UserIdentity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldSetEmployeeId_WhenProvided()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            LoginProvider = LoginProviders.MicrosoftEntraId,
            EmployeeId = employeeId,
        };

        _mockUserManager
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager
            .Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), ApplicationRoles.Basic))
            .ReturnsAsync(IdentityResult.Success);

        var sut = CreateSut();

        // Act
        var result = await sut.CreateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockUserManager.Verify(x => x.CreateAsync(It.Is<ApplicationUser>(u =>
            u.EmployeeId == employeeId)), Times.Once);
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUser_WhenUserExists()
    {
        // Arrange
        var user = CreateUser();
        var command = new UpdateUserCommand { Id = "user-1", FirstName = "Updated", LastName = "Name", Email = "updated@example.com", PhoneNumber = "555-1234" };

        _mockUserManager.Setup(x => x.FindByIdAsync("user-1")).ReturnsAsync(user);
        _mockUserManager.Setup(x => x.GetPhoneNumberAsync(user)).ReturnsAsync((string?)null);
        _mockUserManager.Setup(x => x.SetPhoneNumberAsync(user, "555-1234")).ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var sut = CreateSut();

        // Act
        await sut.UpdateAsync(command, "user-1");

        // Assert
        user.FirstName.Should().Be("Updated");
        user.LastName.Should().Be("Name");
        user.Email.Should().Be("updated@example.com");
        user.UserName.Should().Be("updated@example.com");
        user.NormalizedEmail.Should().Be("UPDATED@EXAMPLE.COM");
        user.NormalizedUserName.Should().Be("UPDATED@EXAMPLE.COM");
        _mockUserManager.Verify(x => x.UpdateAsync(user), Times.Once);
        _mockSignInManager.Verify(x => x.RefreshSignInAsync(user), Times.Once);
        _mockEvents.Verify(x => x.PublishAsync(It.IsAny<ApplicationUserUpdatedEvent>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        _mockUserManager.Setup(x => x.FindByIdAsync("missing")).ReturnsAsync((ApplicationUser?)null);
        var command = new UpdateUserCommand { Id = "missing", FirstName = "F", LastName = "L", Email = "e@e.com" };

        var sut = CreateSut();

        // Act
        var act = () => sut.UpdateAsync(command, "missing");

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_ShouldNotSetPhone_WhenPhoneNumberUnchanged()
    {
        // Arrange
        var user = CreateUser();
        var command = new UpdateUserCommand { Id = "user-1", FirstName = "Test", LastName = "User", Email = "test@example.com", PhoneNumber = "555-1234" };

        _mockUserManager.Setup(x => x.FindByIdAsync("user-1")).ReturnsAsync(user);
        _mockUserManager.Setup(x => x.GetPhoneNumberAsync(user)).ReturnsAsync("555-1234");
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var sut = CreateSut();

        // Act
        await sut.UpdateAsync(command, "user-1");

        // Assert
        _mockUserManager.Verify(x => x.SetPhoneNumberAsync(user, It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenUpdateFails()
    {
        // Arrange
        var user = CreateUser();
        var command = new UpdateUserCommand { Id = "user-1", FirstName = "Test", LastName = "User", Email = "test@example.com" };

        _mockUserManager.Setup(x => x.FindByIdAsync("user-1")).ReturnsAsync(user);
        _mockUserManager.Setup(x => x.GetPhoneNumberAsync(user)).ReturnsAsync((string?)null);
        _mockUserManager.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Update failed." }));

        var sut = CreateSut();

        // Act
        var act = () => sut.UpdateAsync(command, "user-1");

        // Assert
        await act.Should().ThrowAsync<InternalServerException>();
    }

    #endregion

    #region ActivateUserAsync

    [Fact]
    public async Task ActivateUserAsync_ShouldActivateUser_WhenUserIsInactive()
    {
        // Arrange
        var user = CreateUser();
        user.IsActive = false;
        _mockUserManager.Setup(x => x.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var command = new ActivateUserCommand("user-1");
        var sut = CreateSut();

        // Act
        var result = await sut.ActivateUserAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.IsActive.Should().BeTrue();
        _mockUserManager.Verify(x => x.UpdateAsync(user), Times.Once);
        _mockEvents.Verify(x => x.PublishAsync(It.IsAny<ApplicationUserActivatedEvent>()), Times.Once);
    }

    [Fact]
    public async Task ActivateUserAsync_ShouldFail_WhenUserIsAlreadyActive()
    {
        // Arrange
        var user = CreateUser();
        user.IsActive = true;
        _mockUserManager.Setup(x => x.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);

        var command = new ActivateUserCommand("user-1");
        var sut = CreateSut();

        // Act
        var result = await sut.ActivateUserAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task ActivateUserAsync_ShouldThrowNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        _mockUserManager.Setup(x => x.Users).Returns(Array.Empty<ApplicationUser>().AsQueryable().BuildMockDbSet().Object);

        var command = new ActivateUserCommand("missing");
        var sut = CreateSut();

        // Act
        var act = () => sut.ActivateUserAsync(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region DeactivateUserAsync

    [Fact]
    public async Task DeactivateUserAsync_ShouldDeactivateUser_WhenUserIsActive()
    {
        // Arrange
        var user = CreateUser();
        user.IsActive = true;
        _mockUserManager.Setup(x => x.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var command = new DeactivateUserCommand("user-1");
        var sut = CreateSut();

        // Act
        var result = await sut.DeactivateUserAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.IsActive.Should().BeFalse();
        _mockUserManager.Verify(x => x.UpdateAsync(user), Times.Once);
        _mockEvents.Verify(x => x.PublishAsync(It.IsAny<ApplicationUserDeactivatedEvent>()), Times.Once);
    }

    [Fact]
    public async Task DeactivateUserAsync_ShouldSucceed_WhenDeactivatingAnotherAdmin()
    {
        // Arrange
        var adminUser = CreateUser(id: "other-admin");
        adminUser.IsActive = true;
        _mockUserManager.Setup(x => x.Users).Returns(new[] { adminUser }.AsQueryable().BuildMockDbSet().Object);
        _mockUserManager.Setup(x => x.UpdateAsync(adminUser)).ReturnsAsync(IdentityResult.Success);

        var command = new DeactivateUserCommand("other-admin");
        var sut = CreateSut();

        // Act
        var result = await sut.DeactivateUserAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        adminUser.IsActive.Should().BeFalse();
        _mockUserManager.Verify(x => x.UpdateAsync(adminUser), Times.Once);
        _mockEvents.Verify(x => x.PublishAsync(It.IsAny<ApplicationUserDeactivatedEvent>()), Times.Once);
    }

    [Fact]
    public async Task DeactivateUserAsync_ShouldFail_WhenUserIsAlreadyInactive()
    {
        // Arrange
        var user = CreateUser();
        user.IsActive = false;
        _mockUserManager.Setup(x => x.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);

        var command = new DeactivateUserCommand("user-1");
        var sut = CreateSut();

        // Act
        var result = await sut.DeactivateUserAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task DeactivateUserAsync_ShouldFail_WhenDeactivatingSelf()
    {
        // Arrange
        var user = CreateUser(id: "current-user-id");
        user.IsActive = true;
        _mockUserManager.Setup(x => x.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);

        var command = new DeactivateUserCommand("current-user-id");
        var sut = CreateSut();

        // Act
        var result = await sut.DeactivateUserAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("cannot deactivate your own account");
        user.IsActive.Should().BeTrue();
        _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task DeactivateUserAsync_ShouldThrowNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        _mockUserManager.Setup(x => x.Users).Returns(Array.Empty<ApplicationUser>().AsQueryable().BuildMockDbSet().Object);

        var command = new DeactivateUserCommand("missing");
        var sut = CreateSut();

        // Act
        var act = () => sut.DeactivateUserAsync(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region AssignRolesAsync

    [Fact]
    public async Task AssignRolesAsync_ShouldAddAndRemoveRoles()
    {
        // Arrange
        var user = CreateUser();
        _mockUserManager.Setup(x => x.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
        _mockUserManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(["Basic"]);
        _mockUserManager.Setup(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(x => x.AddToRolesAsync(user, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);

        var command = new AssignUserRolesCommand("user-1", ["Admin"]);
        var sut = CreateSut();

        // Act
        var result = await sut.AssignRolesAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockUserManager.Verify(x => x.RemoveFromRolesAsync(user, It.Is<IEnumerable<string>>(r => r.Contains("Basic"))), Times.Once);
        _mockUserManager.Verify(x => x.AddToRolesAsync(user, It.Is<IEnumerable<string>>(r => r.Contains("Admin"))), Times.Once);
        _mockEvents.Verify(x => x.PublishAsync(It.IsAny<ApplicationUserUpdatedEvent>()), Times.Once);
    }

    [Fact]
    public async Task AssignRolesAsync_ShouldThrowConflict_WhenRemovingLastAdmin()
    {
        // Arrange
        var user = CreateUser();
        _mockUserManager.Setup(x => x.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
        _mockUserManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(["Admin"]);
        _mockUserManager.Setup(x => x.GetUsersInRoleAsync(ApplicationRoles.Admin))
            .ReturnsAsync([user]);

        var command = new AssignUserRolesCommand("user-1", ["Basic"]);
        var sut = CreateSut();

        // Act
        var act = () => sut.AssignRolesAsync(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*at least 1 Admin*");
    }

    [Fact]
    public async Task AssignRolesAsync_ShouldThrowNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        _mockUserManager.Setup(x => x.Users).Returns(Array.Empty<ApplicationUser>().AsQueryable().BuildMockDbSet().Object);

        var command = new AssignUserRolesCommand("missing", ["Basic"]);
        var sut = CreateSut();

        // Act
        var act = () => sut.AssignRolesAsync(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region ChangePasswordAsync

    [Fact]
    public async Task ChangePasswordAsync_ShouldSucceed_WhenLocalUserWithValidPassword()
    {
        // Arrange
        var user = CreateUser(loginProvider: LoginProviders.Wayd);
        _mockUserManager.Setup(x => x.FindByIdAsync("user-1")).ReturnsAsync(user);
        _mockUserManager.Setup(x => x.ChangePasswordAsync(user, "OldPass123!", "NewPass456!"))
            .ReturnsAsync(IdentityResult.Success);

        var command = new ChangePasswordCommand("OldPass123!", "NewPass456!");
        var sut = CreateSut();

        // Act
        var result = await sut.ChangePasswordAsync("user-1", command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockUserManager.Verify(x => x.ChangePasswordAsync(user, "OldPass123!", "NewPass456!"), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_ShouldClearMustChangePassword_WhenFlagIsSet()
    {
        // Arrange
        var user = CreateUser(loginProvider: LoginProviders.Wayd);
        user.MustChangePassword = true;
        _mockUserManager.Setup(x => x.FindByIdAsync("user-1")).ReturnsAsync(user);
        _mockUserManager.Setup(x => x.ChangePasswordAsync(user, "OldPass123!", "NewPass456!"))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var command = new ChangePasswordCommand("OldPass123!", "NewPass456!");
        var sut = CreateSut();

        // Act
        var result = await sut.ChangePasswordAsync("user-1", command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.MustChangePassword.Should().BeFalse();
        _mockUserManager.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_ShouldReturnFailure_WhenUserIsNotLocal()
    {
        // Arrange
        var user = CreateUser(); // Default is MicrosoftEntraId
        _mockUserManager.Setup(x => x.FindByIdAsync("user-1")).ReturnsAsync(user);

        var command = new ChangePasswordCommand("OldPass123!", "NewPass456!");
        var sut = CreateSut();

        // Act
        var result = await sut.ChangePasswordAsync("user-1", command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("only available for local accounts");
    }

    [Fact]
    public async Task ChangePasswordAsync_ShouldReturnFailure_WhenIdentityFails()
    {
        // Arrange
        var user = CreateUser(loginProvider: LoginProviders.Wayd);
        _mockUserManager.Setup(x => x.FindByIdAsync("user-1")).ReturnsAsync(user);
        _mockUserManager.Setup(x => x.ChangePasswordAsync(user, "wrong", "NewPass456!"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Incorrect password." }));

        var command = new ChangePasswordCommand("wrong", "NewPass456!");
        var sut = CreateSut();

        // Act
        var result = await sut.ChangePasswordAsync("user-1", command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Incorrect password.");
    }

    [Fact]
    public async Task ChangePasswordAsync_ShouldThrowNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        _mockUserManager.Setup(x => x.FindByIdAsync("missing")).ReturnsAsync((ApplicationUser?)null);

        var command = new ChangePasswordCommand("OldPass123!", "NewPass456!");
        var sut = CreateSut();

        // Act
        var act = () => sut.ChangePasswordAsync("missing", command);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region ResetPasswordAsync

    [Fact]
    public async Task ResetPasswordAsync_ShouldSucceed_AndSetMustChangePassword()
    {
        // Arrange
        var user = CreateUser(loginProvider: LoginProviders.Wayd);
        _mockUserManager.Setup(x => x.FindByIdAsync("user-1")).ReturnsAsync(user);
        _mockUserManager.Setup(x => x.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("reset-token");
        _mockUserManager.Setup(x => x.ResetPasswordAsync(user, "reset-token", "NewPass456!"))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(x => x.IsLockedOutAsync(user)).ReturnsAsync(false);

        var command = new ResetPasswordCommand("user-1", "NewPass456!");
        var sut = CreateSut();

        // Act
        var result = await sut.ResetPasswordAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.MustChangePassword.Should().BeTrue();
        _mockUserManager.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldClearLockout_WhenUserIsLockedOut()
    {
        // Arrange
        var user = CreateUser(loginProvider: LoginProviders.Wayd);
        _mockUserManager.Setup(x => x.FindByIdAsync("user-1")).ReturnsAsync(user);
        _mockUserManager.Setup(x => x.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("reset-token");
        _mockUserManager.Setup(x => x.ResetPasswordAsync(user, "reset-token", "NewPass456!"))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(x => x.IsLockedOutAsync(user)).ReturnsAsync(true);
        _mockUserManager.Setup(x => x.SetLockoutEndDateAsync(user, null)).ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(x => x.ResetAccessFailedCountAsync(user)).ReturnsAsync(IdentityResult.Success);

        var command = new ResetPasswordCommand("user-1", "NewPass456!");
        var sut = CreateSut();

        // Act
        var result = await sut.ResetPasswordAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockUserManager.Verify(x => x.SetLockoutEndDateAsync(user, null), Times.Once);
        _mockUserManager.Verify(x => x.ResetAccessFailedCountAsync(user), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldNotClearLockout_WhenUserIsNotLockedOut()
    {
        // Arrange
        var user = CreateUser(loginProvider: LoginProviders.Wayd);
        _mockUserManager.Setup(x => x.FindByIdAsync("user-1")).ReturnsAsync(user);
        _mockUserManager.Setup(x => x.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("reset-token");
        _mockUserManager.Setup(x => x.ResetPasswordAsync(user, "reset-token", "NewPass456!"))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(x => x.IsLockedOutAsync(user)).ReturnsAsync(false);

        var command = new ResetPasswordCommand("user-1", "NewPass456!");
        var sut = CreateSut();

        // Act
        var result = await sut.ResetPasswordAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockUserManager.Verify(x => x.SetLockoutEndDateAsync(It.IsAny<ApplicationUser>(), It.IsAny<DateTimeOffset?>()), Times.Never);
        _mockUserManager.Verify(x => x.ResetAccessFailedCountAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldReturnFailure_WhenUserIsNotLocal()
    {
        // Arrange
        var user = CreateUser(); // Default is MicrosoftEntraId
        _mockUserManager.Setup(x => x.FindByIdAsync("user-1")).ReturnsAsync(user);

        var command = new ResetPasswordCommand("user-1", "NewPass456!");
        var sut = CreateSut();

        // Act
        var result = await sut.ResetPasswordAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("only available for local accounts");
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldReturnFailure_WhenIdentityFails()
    {
        // Arrange
        var user = CreateUser(loginProvider: LoginProviders.Wayd);
        _mockUserManager.Setup(x => x.FindByIdAsync("user-1")).ReturnsAsync(user);
        _mockUserManager.Setup(x => x.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("reset-token");
        _mockUserManager.Setup(x => x.ResetPasswordAsync(user, "reset-token", "weak"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too short." }));

        var command = new ResetPasswordCommand("user-1", "weak");
        var sut = CreateSut();

        // Act
        var result = await sut.ResetPasswordAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Password too short.");
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldThrowNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        _mockUserManager.Setup(x => x.FindByIdAsync("missing")).ReturnsAsync((ApplicationUser?)null);

        var command = new ResetPasswordCommand("missing", "NewPass456!");
        var sut = CreateSut();

        // Act
        var act = () => sut.ResetPasswordAsync(command);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region StageTenantMigration

    [Fact]
    public async Task StageTenantMigration_ShouldSetPendingTenant_WhenEntraUserHasActiveIdentity()
    {
        var user = CreateUser(loginProvider: LoginProviders.MicrosoftEntraId);
        var targetTenant = Guid.NewGuid().ToString();

        _mockUserManager.Setup(x => x.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
        _mockUserIdentityStore.Setup(s => s.ExistsActive(user.Id, LoginProviders.MicrosoftEntraId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var sut = CreateSut();

        var result = await sut.StageTenantMigration(
            new StageTenantMigrationCommand(user.Id, targetTenant),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        user.PendingMigrationTenantId.Should().Be(targetTenant);
        _mockUserManager.Verify(x => x.UpdateAsync(user), Times.Once);
        _mockEvents.Verify(x => x.PublishAsync(It.IsAny<ApplicationUserUpdatedEvent>()), Times.Once);
    }

    [Fact]
    public async Task StageTenantMigration_ShouldOverwritePreviousTarget_WhenAlreadyStaged()
    {
        var user = CreateUser(loginProvider: LoginProviders.MicrosoftEntraId);
        user.PendingMigrationTenantId = Guid.NewGuid().ToString();
        var newTarget = Guid.NewGuid().ToString();

        _mockUserManager.Setup(x => x.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
        _mockUserIdentityStore.Setup(s => s.ExistsActive(user.Id, LoginProviders.MicrosoftEntraId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var sut = CreateSut();

        var result = await sut.StageTenantMigration(
            new StageTenantMigrationCommand(user.Id, newTarget),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        user.PendingMigrationTenantId.Should().Be(newTarget);
    }

    [Fact]
    public async Task StageTenantMigration_ShouldFail_WhenUserIsNotEntraProvider()
    {
        var user = CreateUser(loginProvider: LoginProviders.Wayd);
        _mockUserManager.Setup(x => x.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);

        var sut = CreateSut();

        var result = await sut.StageTenantMigration(
            new StageTenantMigrationCommand(user.Id, Guid.NewGuid().ToString()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Microsoft Entra ID");
        _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task StageTenantMigration_ShouldFail_WhenUserHasNoActiveEntraIdentity()
    {
        var user = CreateUser(loginProvider: LoginProviders.MicrosoftEntraId);
        _mockUserManager.Setup(x => x.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
        _mockUserIdentityStore.Setup(s => s.ExistsActive(user.Id, LoginProviders.MicrosoftEntraId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var sut = CreateSut();

        var result = await sut.StageTenantMigration(
            new StageTenantMigrationCommand(user.Id, Guid.NewGuid().ToString()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("no active");
        _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task StageTenantMigration_ShouldThrowNotFound_WhenUserDoesNotExist()
    {
        _mockUserManager.Setup(x => x.Users).Returns(Array.Empty<ApplicationUser>().AsQueryable().BuildMockDbSet().Object);

        var sut = CreateSut();

        var act = () => sut.StageTenantMigration(
            new StageTenantMigrationCommand("missing", Guid.NewGuid().ToString()),
            TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region CancelTenantMigration

    [Fact]
    public async Task CancelTenantMigration_ShouldClearPendingTenant_WhenStaged()
    {
        var user = CreateUser();
        user.PendingMigrationTenantId = Guid.NewGuid().ToString();

        _mockUserManager.Setup(x => x.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var sut = CreateSut();

        var result = await sut.CancelTenantMigration(user.Id, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        user.PendingMigrationTenantId.Should().BeNull();
        _mockUserManager.Verify(x => x.UpdateAsync(user), Times.Once);
        _mockEvents.Verify(x => x.PublishAsync(It.IsAny<ApplicationUserUpdatedEvent>()), Times.Once);
    }

    [Fact]
    public async Task CancelTenantMigration_ShouldBeIdempotent_WhenNothingStaged()
    {
        var user = CreateUser();
        user.PendingMigrationTenantId = null;

        _mockUserManager.Setup(x => x.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);

        var sut = CreateSut();

        var result = await sut.CancelTenantMigration(user.Id, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        _mockEvents.Verify(x => x.PublishAsync(It.IsAny<ApplicationUserUpdatedEvent>()), Times.Never);
    }

    [Fact]
    public async Task CancelTenantMigration_ShouldThrowNotFound_WhenUserDoesNotExist()
    {
        _mockUserManager.Setup(x => x.Users).Returns(Array.Empty<ApplicationUser>().AsQueryable().BuildMockDbSet().Object);

        var sut = CreateSut();

        var act = () => sut.CancelTenantMigration("missing", TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetOrCreateFromPrincipalAsync — pending tenant migration rebind

    private static ClaimsPrincipal CreateEntraPrincipal(string objectId, string tenantId, string? upn = null)
    {
        var claims = new List<Claim>
        {
            new(Microsoft.Identity.Web.ClaimConstants.ObjectId, objectId),
            new(Microsoft.Identity.Web.ClaimConstants.TenantId, tenantId),
        };
        if (upn is not null)
        {
            claims.Add(new Claim(ClaimTypes.Upn, upn));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    [Fact]
    public async Task GetOrCreateFromPrincipalAsync_ShouldRebindIdentity_WhenMigrationStagedAndUpnMatches()
    {
        var newTenantId = Guid.NewGuid().ToString();
        var newObjectId = Guid.NewGuid().ToString();
        var upn = "alice@newtenant.com";

        var user = CreateUser(id: "user-rebind", userName: upn, loginProvider: LoginProviders.MicrosoftEntraId);
        user.NormalizedUserName = upn.ToUpperInvariant();
        user.NormalizedEmail = "ALICE@NEWTENANT.COM";
        user.PendingMigrationTenantId = newTenantId;

        // No active identity for the new (tid, oid). No NULL-tenant backfill row either.
        _mockUserIdentityStore.Setup(s => s.FindActive(LoginProviders.MicrosoftEntraId, newTenantId, newObjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserIdentity?)null);
        _mockUserIdentityStore.Setup(s => s.FindActiveByNullTenant(LoginProviders.MicrosoftEntraId, newObjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<UserIdentity>());

        // Two queryable hits on _userManager.Users: AnyAsync (isFirstUser) then FirstOrDefault for the migration lookup.
        _mockUserManager.Setup(x => x.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
        _mockUserManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(["Basic"]);
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var sut = CreateSut();

        var (resolvedId, _) = await sut.GetOrCreateFromPrincipalAsync(
            CreateEntraPrincipal(newObjectId, newTenantId, upn));

        resolvedId.Should().Be(user.Id);
        user.PendingMigrationTenantId.Should().BeNull();

        _mockUserIdentityStore.Verify(s => s.DeactivateAllActive(
            user.Id,
            It.IsAny<NodaTime.Instant>(),
            UserIdentityUnlinkReasons.TenantMigration,
            It.IsAny<CancellationToken>()),
            Times.Once);

        _mockUserIdentityStore.Verify(s => s.Add(
            It.Is<UserIdentity>(ui =>
                ui.UserId == user.Id &&
                ui.Provider == LoginProviders.MicrosoftEntraId &&
                ui.ProviderTenantId == newTenantId &&
                ui.ProviderSubject == newObjectId &&
                ui.IsActive),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task TryApplyPendingTenantMigration_ShouldNotRebind_WhenPendingTenantDoesNotMatchToken()
    {
        var stagedTenant = Guid.NewGuid().ToString();
        var unrelatedTenant = Guid.NewGuid().ToString();
        var newObjectId = Guid.NewGuid().ToString();
        var upn = "alice@example.com";

        var user = CreateUser(id: "user-rebind", userName: upn, loginProvider: LoginProviders.MicrosoftEntraId);
        user.NormalizedUserName = upn.ToUpperInvariant();
        user.NormalizedEmail = "ALICE@EXAMPLE.COM";
        // Staged for a different tenant than the token's tid.
        user.PendingMigrationTenantId = stagedTenant;

        _mockUserManager.Setup(x => x.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);

        var sut = CreateSut();

        // Exercise the rebind decision directly so the test doesn't depend on the
        // surrounding GetOrCreateFromPrincipalAsync (which calls into Graph on the
        // create path). Returning null here is what causes the caller to fall through
        // to CreateOrUpdateFromPrincipalAsync.
        var result = await sut.TryApplyPendingTenantMigration(unrelatedTenant, newObjectId, upn);

        result.Should().BeNull();
        user.PendingMigrationTenantId.Should().Be(stagedTenant);
        _mockUserIdentityStore.Verify(s => s.DeactivateAllActive(
            It.IsAny<string>(), It.IsAny<NodaTime.Instant>(), UserIdentityUnlinkReasons.TenantMigration, It.IsAny<CancellationToken>()),
            Times.Never);
        _mockUserIdentityStore.Verify(s => s.Add(It.IsAny<UserIdentity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task TryApplyPendingTenantMigration_ShouldNotRebind_WhenDifferentUserHasMatchingEmailButNoFlag()
    {
        var newTenantId = Guid.NewGuid().ToString();
        var newObjectId = Guid.NewGuid().ToString();
        var upn = "alice@example.com";

        // A user with the same email but no pending migration must not be rebound.
        var unrelatedUser = CreateUser(id: "user-unrelated", userName: upn, loginProvider: LoginProviders.MicrosoftEntraId);
        unrelatedUser.NormalizedUserName = upn.ToUpperInvariant();
        unrelatedUser.NormalizedEmail = "ALICE@EXAMPLE.COM";
        unrelatedUser.PendingMigrationTenantId = null;

        _mockUserManager.Setup(x => x.Users).Returns(new[] { unrelatedUser }.AsQueryable().BuildMockDbSet().Object);

        var sut = CreateSut();

        var result = await sut.TryApplyPendingTenantMigration(newTenantId, newObjectId, upn);

        result.Should().BeNull();
        _mockUserIdentityStore.Verify(s => s.DeactivateAllActive(
            It.IsAny<string>(), It.IsAny<NodaTime.Instant>(), UserIdentityUnlinkReasons.TenantMigration, It.IsAny<CancellationToken>()),
            Times.Never);
        _mockUserIdentityStore.Verify(s => s.Add(It.IsAny<UserIdentity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task TryApplyPendingTenantMigration_ShouldReturnNull_WhenUpnIsMissing()
    {
        // Defensive: a token without a UPN claim should not match any user, even if
        // one happens to have a pending migration for the token's tenant.
        var newTenantId = Guid.NewGuid().ToString();
        var newObjectId = Guid.NewGuid().ToString();

        var user = CreateUser(id: "user-with-pending", loginProvider: LoginProviders.MicrosoftEntraId);
        user.PendingMigrationTenantId = newTenantId;

        _mockUserManager.Setup(x => x.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);

        var sut = CreateSut();

        var result = await sut.TryApplyPendingTenantMigration(newTenantId, newObjectId, upn: null);

        result.Should().BeNull();
        user.PendingMigrationTenantId.Should().Be(newTenantId);
        _mockUserIdentityStore.Verify(s => s.DeactivateAllActive(
            It.IsAny<string>(), It.IsAny<NodaTime.Instant>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task TryApplyPendingTenantMigration_ShouldThrowAndRollBackTransaction_WhenClearingFlagFails()
    {
        // If UserManager.UpdateAsync fails inside the rebind transaction (e.g.,
        // concurrency token mismatch), the deactivate+insert must NOT commit — otherwise
        // the user has a fresh active identity row but PendingMigrationTenantId is still
        // set, which would re-trigger the rebind path on next login and explode against
        // the unique-active-row index.
        var newTenantId = Guid.NewGuid().ToString();
        var newObjectId = Guid.NewGuid().ToString();
        var upn = "alice@newtenant.com";

        var user = CreateUser(id: "user-rebind-fail", userName: upn, loginProvider: LoginProviders.MicrosoftEntraId);
        user.NormalizedUserName = upn.ToUpperInvariant();
        user.NormalizedEmail = "ALICE@NEWTENANT.COM";
        user.PendingMigrationTenantId = newTenantId;

        _mockUserManager.Setup(x => x.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
        _mockUserManager.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Concurrency failure." }));

        // Track what happened inside the transaction lambda. We can't inspect EF's
        // Database.BeginTransactionAsync rollback directly with the mocked store, but
        // we can verify (a) the action threw — which is what triggers rollback in the
        // real ExecuteInTransaction — and (b) Add was called before the failing
        // UpdateAsync, proving the rebind logic ran to the failing step rather than
        // bailing out earlier.
        Exception? captured = null;
        _mockUserIdentityStore
            .Setup(s => s.ExecuteInTransaction(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task>, CancellationToken>(async (action, ct) =>
            {
                try
                {
                    await action(ct);
                }
                catch (Exception ex)
                {
                    captured = ex;
                    throw;
                }
            });

        var sut = CreateSut();

        var act = () => sut.TryApplyPendingTenantMigration(newTenantId, newObjectId, upn);

        await act.Should().ThrowAsync<InternalServerException>()
            .WithMessage("*Failed to clear pending migration flag*Concurrency failure*");

        captured.Should().NotBeNull("the transaction lambda must throw so ExecuteInTransaction rolls back");
        _mockUserIdentityStore.Verify(s => s.Add(It.IsAny<UserIdentity>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUserIdentityStore.Verify(s => s.DeactivateAllActive(
            user.Id, It.IsAny<NodaTime.Instant>(), UserIdentityUnlinkReasons.TenantMigration, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region UnlockUserAsync

    [Fact]
    public async Task UnlockUserAsync_ShouldSucceed_WhenUserIsLockedOut()
    {
        // Arrange
        var user = CreateUser(loginProvider: LoginProviders.Wayd);
        _mockUserManager.Setup(x => x.FindByIdAsync("user-1")).ReturnsAsync(user);
        _mockUserManager.Setup(x => x.IsLockedOutAsync(user)).ReturnsAsync(true);
        _mockUserManager.Setup(x => x.SetLockoutEndDateAsync(user, null)).ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(x => x.ResetAccessFailedCountAsync(user)).ReturnsAsync(IdentityResult.Success);

        var sut = CreateSut();

        // Act
        var result = await sut.UnlockUserAsync("user-1");

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockUserManager.Verify(x => x.SetLockoutEndDateAsync(user, null), Times.Once);
        _mockUserManager.Verify(x => x.ResetAccessFailedCountAsync(user), Times.Once);
    }

    [Fact]
    public async Task UnlockUserAsync_ShouldReturnFailure_WhenUserIsNotLockedOut()
    {
        // Arrange
        var user = CreateUser(loginProvider: LoginProviders.Wayd);
        _mockUserManager.Setup(x => x.FindByIdAsync("user-1")).ReturnsAsync(user);
        _mockUserManager.Setup(x => x.IsLockedOutAsync(user)).ReturnsAsync(false);

        var sut = CreateSut();

        // Act
        var result = await sut.UnlockUserAsync("user-1");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not currently locked out");
        _mockUserManager.Verify(x => x.SetLockoutEndDateAsync(It.IsAny<ApplicationUser>(), It.IsAny<DateTimeOffset?>()), Times.Never);
    }

    [Fact]
    public async Task UnlockUserAsync_ShouldThrowNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        _mockUserManager.Setup(x => x.FindByIdAsync("missing")).ReturnsAsync((ApplicationUser?)null);

        var sut = CreateSut();

        // Act
        var act = () => sut.UnlockUserAsync("missing");

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}

/// <summary>
/// Extension to build a mock DbSet from an IQueryable for UserManager.Users property.
/// </summary>
internal static class MockDbSetExtensions
{
    public static Mock<Microsoft.EntityFrameworkCore.DbSet<T>> BuildMockDbSet<T>(this IQueryable<T> source) where T : class
    {
        var mockSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<T>>();

        mockSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(source.GetEnumerator()));

        mockSet.As<IQueryable<T>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<T>(source.Provider));

        mockSet.As<IQueryable<T>>()
            .Setup(m => m.Expression)
            .Returns(source.Expression);

        mockSet.As<IQueryable<T>>()
            .Setup(m => m.ElementType)
            .Returns(source.ElementType);

        mockSet.As<IQueryable<T>>()
            .Setup(m => m.GetEnumerator())
            .Returns(source.GetEnumerator());

        return mockSet;
    }
}

internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    public TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

    public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
        => new TestAsyncEnumerable<TEntity>(expression);

    public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
        => new TestAsyncEnumerable<TElement>(expression);

    public object? Execute(System.Linq.Expressions.Expression expression)
        => _inner.Execute(expression);

    public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
        => _inner.Execute<TResult>(expression);

    public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken = default)
    {
        var expectedResultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider)
            .GetMethod(nameof(IQueryProvider.Execute), 1, [typeof(System.Linq.Expressions.Expression)])!
            .MakeGenericMethod(expectedResultType)
            .Invoke(_inner, [expression]);

        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(expectedResultType)
            .Invoke(null, [executionResult])!;
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(System.Linq.Expressions.Expression expression) : base(expression) { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;

    public T Current => _inner.Current;

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(_inner.MoveNext());
}
