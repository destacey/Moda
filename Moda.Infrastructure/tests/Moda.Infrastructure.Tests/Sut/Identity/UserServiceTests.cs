using System.Linq.Expressions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moda.Common.Application.Events;
using Moda.Common.Application.Exceptions;
using Moda.Common.Application.Identity;
using Moda.Common.Application.Identity.Users;
using Moda.Common.Application.Interfaces;
using Moda.Common.Domain.Authorization;
using Moda.Common.Domain.Identity;
using Moda.Infrastructure.Identity;
using Moda.Tests.Shared;
using NotFoundException = Moda.Common.Application.Exceptions.NotFoundException;

namespace Moda.Infrastructure.Tests.Sut.Identity;

public class UserServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
    private readonly Mock<RoleManager<ApplicationRole>> _mockRoleManager;
    private readonly Mock<IEventPublisher> _mockEvents;
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly TestingDateTimeProvider _dateTimeProvider;
    private readonly Mock<ISender> _mockSender;

    // UserService depends on ModaDbContext and GraphServiceClient which are hard to mock.
    // We test the methods that primarily use UserManager.

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
    }

    private UserService CreateSut()
    {
        // ModaDbContext and GraphServiceClient are required but not used by the methods we test
        // (CreateAsync, UpdateAsync, ToggleStatusAsync, AssignRolesAsync).
        // We pass null! for them since those methods don't touch them.
        return new UserService(
            _mockLogger.Object,
            _mockSignInManager.Object,
            _mockUserManager.Object,
            _mockRoleManager.Object,
            null!, // ModaDbContext - not used by these methods
            _mockEvents.Object,
            null!, // GraphServiceClient - not used by these methods
            _mockSender.Object,
            _dateTimeProvider);
    }

    private static ApplicationUser CreateUser(string id = "user-1", string userName = "testuser", bool isActive = true)
    {
        return new ApplicationUser
        {
            Id = id,
            UserName = userName,
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            IsActive = isActive,
            LoginProvider = LoginProviders.MicrosoftEntraId,
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
            LoginProvider = LoginProviders.Moda,
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
        var result = await sut.CreateAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNullOrWhiteSpace();
        _mockUserManager.Verify(x => x.CreateAsync(It.Is<ApplicationUser>(u =>
            u.FirstName == "John" &&
            u.LastName == "Doe" &&
            u.Email == "john@example.com" &&
            u.UserName == "john@example.com" &&
            u.LoginProvider == LoginProviders.Moda &&
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
        var result = await sut.CreateAsync(command, CancellationToken.None);

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
            LoginProvider = LoginProviders.Moda,
            Password = "Password123!",
        };

        _mockUserManager
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "Password123!"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Duplicate username." }));

        var sut = CreateSut();

        // Act
        var result = await sut.CreateAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Duplicate username.");
        _mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        _mockEvents.Verify(x => x.PublishAsync(It.IsAny<ApplicationUserCreatedEvent>()), Times.Never);
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
        var result = await sut.CreateAsync(command, CancellationToken.None);

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

    #region ToggleStatusAsync

    [Fact]
    public async Task ToggleStatusAsync_ShouldDeactivateUser_WhenUserIsNotAdmin()
    {
        // Arrange
        var user = CreateUser();
        _mockUserManager.Setup(x => x.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
        _mockUserManager.Setup(x => x.IsInRoleAsync(user, ApplicationRoles.Admin)).ReturnsAsync(false);
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var command = new ToggleUserStatusCommand("user-1", false);
        var sut = CreateSut();

        // Act
        await sut.ToggleStatusAsync(command, CancellationToken.None);

        // Assert
        user.IsActive.Should().BeFalse();
        _mockUserManager.Verify(x => x.UpdateAsync(user), Times.Once);
        _mockEvents.Verify(x => x.PublishAsync(It.IsAny<ApplicationUserUpdatedEvent>()), Times.Once);
    }

    [Fact]
    public async Task ToggleStatusAsync_ShouldThrowConflict_WhenUserIsAdmin()
    {
        // Arrange
        var user = CreateUser();
        _mockUserManager.Setup(x => x.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
        _mockUserManager.Setup(x => x.IsInRoleAsync(user, ApplicationRoles.Admin)).ReturnsAsync(true);

        var command = new ToggleUserStatusCommand("user-1", false);
        var sut = CreateSut();

        // Act
        var act = () => sut.ToggleStatusAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task ToggleStatusAsync_ShouldThrowNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        _mockUserManager.Setup(x => x.Users).Returns(Array.Empty<ApplicationUser>().AsQueryable().BuildMockDbSet().Object);

        var command = new ToggleUserStatusCommand("missing", false);
        var sut = CreateSut();

        // Act
        var act = () => sut.ToggleStatusAsync(command, CancellationToken.None);

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
        _mockUserManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Basic" });
        _mockUserManager.Setup(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(x => x.AddToRolesAsync(user, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);

        var command = new AssignUserRolesCommand("user-1", ["Admin"]);
        var sut = CreateSut();

        // Act
        var result = await sut.AssignRolesAsync(command, CancellationToken.None);

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
        _mockUserManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });
        _mockUserManager.Setup(x => x.GetUsersInRoleAsync(ApplicationRoles.Admin))
            .ReturnsAsync(new List<ApplicationUser> { user });

        var command = new AssignUserRolesCommand("user-1", ["Basic"]);
        var sut = CreateSut();

        // Act
        var act = () => sut.AssignRolesAsync(command, CancellationToken.None);

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
        var act = () => sut.AssignRolesAsync(command, CancellationToken.None);

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
