using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wayd.Common.Application.Exceptions;
using Wayd.Common.Application.Identity;
using Wayd.Common.Application.Identity.Tokens;
using Wayd.Common.Application.Identity.Users;
using Wayd.Infrastructure.Auth.Entra;
using Wayd.Infrastructure.Auth.Local;
using Wayd.Infrastructure.Identity;
using Wayd.Tests.Shared;

namespace Wayd.Infrastructure.Tests.Sut.Auth.Local;

public class TokenServiceTests
{
    private const string TestSecret = "ThisIsATestSecretKeyThatIsLongEnoughForHmacSha256!";
    private const string TestIssuer = "https://test.wayd.dev";
    private const string TestAudience = "https://test.api.wayd.dev";

    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
    private readonly IConfiguration _configuration;
    private readonly TestingDateTimeProvider _dateTimeProvider;
    private readonly Mock<ILogger<TokenService>> _mockLogger;
    private readonly Mock<IUserIdentityStore> _mockUserIdentityStore;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IEntraIdTokenValidator> _mockEntraIdTokenValidator;
    private readonly EntraSettings _entraSettings;
    private readonly TokenService _sut;

    public TokenServiceTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
            _mockUserManager.Object,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>().Object,
            null!, null!, null!, null!);

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "SecuritySettings:LocalJwt:Secret", TestSecret },
                { "SecuritySettings:LocalJwt:Issuer", TestIssuer },
                { "SecuritySettings:LocalJwt:Audience", TestAudience },
                { "SecuritySettings:LocalJwt:TokenExpirationInMinutes", "60" },
                { "SecuritySettings:LocalJwt:RefreshTokenExpirationInDays", "7" },
            })
            .Build();

        _dateTimeProvider = new TestingDateTimeProvider(DateTime.UtcNow);
        _mockLogger = new Mock<ILogger<TokenService>>();
        _mockUserIdentityStore = new Mock<IUserIdentityStore>();
        _mockUserService = new Mock<IUserService>();
        _mockEntraIdTokenValidator = new Mock<IEntraIdTokenValidator>();

        // Default: user has no permissions. Individual tests can override.
        _mockUserService
            .Setup(s => s.GetPermissionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>());

        // Default: Entra is enabled so the majority of exchange tests exercise the
        // full flow. Tests that care about the disabled path mutate this directly.
        _entraSettings = new EntraSettings { Enabled = true };

        _sut = new TokenService(
            _mockUserManager.Object,
            _mockSignInManager.Object,
            _configuration,
            _dateTimeProvider,
            _mockUserIdentityStore.Object,
            _mockUserService.Object,
            _mockEntraIdTokenValidator.Object,
            Options.Create(_entraSettings),
            _mockLogger.Object);
    }

    private static ApplicationUser CreateLocalUser(string id = "user-1", string userName = "testuser", bool isActive = true)
    {
        return new ApplicationUser
        {
            Id = id,
            UserName = userName,
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            IsActive = isActive,
            LoginProvider = LoginProviders.Wayd,
        };
    }

    private void SeedActiveWaydIdentity(string userId)
    {
        _mockUserIdentityStore
            .Setup(s => s.ExistsActive(userId, LoginProviders.Wayd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    #region GetTokenAsync

    [Fact]
    public async Task GetTokenAsync_ShouldReturnTokenResponse_WhenCredentialsAreValid()
    {
        // Arrange
        var user = CreateLocalUser();
        SeedActiveWaydIdentity(user.Id);
        var command = new LoginCommand("testuser", "Password123!");

        _mockUserManager.Setup(x => x.FindByNameAsync("testuser")).ReturnsAsync(user);
        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, "Password123!", true))
            .ReturnsAsync(SignInResult.Success);
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _sut.GetTokenAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.TokenExpiresAt.Should().BeAfter(DateTime.UtcNow);

        // Verify the JWT contains expected claims
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(result.Token);
        jwt.Issuer.Should().Be(TestIssuer);
        jwt.Audiences.Should().Contain(TestAudience);
        jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value.Should().Be("user-1");
        jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetTokenAsync_ShouldIncludeEmployeeIdClaim_WhenUserHasEmployee()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var user = CreateLocalUser();
        user.EmployeeId = employeeId;
        SeedActiveWaydIdentity(user.Id);
        var command = new LoginCommand("testuser", "Password123!");

        _mockUserManager.Setup(x => x.FindByNameAsync("testuser")).ReturnsAsync(user);
        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, "Password123!", true))
            .ReturnsAsync(SignInResult.Success);
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _sut.GetTokenAsync(command, TestContext.Current.CancellationToken);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(result.Token);
        jwt.Claims.First(c => c.Type == "EmployeeId").Value.Should().Be(employeeId.ToString());
    }

    [Fact]
    public async Task GetTokenAsync_ShouldThrowUnauthorized_WhenUserNotFound()
    {
        // Arrange
        _mockUserManager.Setup(x => x.FindByNameAsync("unknown")).ReturnsAsync((ApplicationUser?)null);
        var command = new LoginCommand("unknown", "Password123!");

        // Act
        var act = () => _sut.GetTokenAsync(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid credentials.");
    }

    [Fact]
    public async Task GetTokenAsync_ShouldThrowUnauthorized_WhenUserIsInactive()
    {
        // Arrange
        var user = CreateLocalUser(isActive: false);
        _mockUserManager.Setup(x => x.FindByNameAsync("testuser")).ReturnsAsync(user);
        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, "Password123!", true))
            .ReturnsAsync(SignInResult.Success);
        var command = new LoginCommand("testuser", "Password123!");

        // Act
        var act = () => _sut.GetTokenAsync(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("*deactivated*");
    }

    [Fact]
    public async Task GetTokenAsync_ShouldThrowUnauthorized_WhenUserIsNotLocalProvider()
    {
        // Arrange
        var user = CreateLocalUser();
        user.LoginProvider = LoginProviders.MicrosoftEntraId;
        _mockUserManager.Setup(x => x.FindByNameAsync("testuser")).ReturnsAsync(user);
        var command = new LoginCommand("testuser", "Password123!");

        // Act
        var act = () => _sut.GetTokenAsync(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid credentials.");
    }

    [Fact]
    public async Task GetTokenAsync_ShouldThrowUnauthorized_WhenPasswordIsInvalid()
    {
        // Arrange
        var user = CreateLocalUser();
        _mockUserManager.Setup(x => x.FindByNameAsync("testuser")).ReturnsAsync(user);
        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, "wrong", true))
            .ReturnsAsync(SignInResult.Failed);
        var command = new LoginCommand("testuser", "wrong");

        // Act
        var act = () => _sut.GetTokenAsync(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid credentials.");
    }

    [Fact]
    public async Task GetTokenAsync_ShouldThrowUnauthorized_WhenUserIsLockedOut()
    {
        // Arrange
        var user = CreateLocalUser();
        _mockUserManager.Setup(x => x.FindByNameAsync("testuser")).ReturnsAsync(user);
        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, "Password123!", true))
            .ReturnsAsync(SignInResult.LockedOut);
        var command = new LoginCommand("testuser", "Password123!");

        // Act
        var act = () => _sut.GetTokenAsync(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("*locked*");
    }

    [Fact]
    public async Task GetTokenAsync_ShouldThrowUnauthorized_WhenNoActiveWaydIdentity()
    {
        // Credentials valid, user active, but the UserIdentity row has been deactivated
        // (admin-revoked local login). Must fail without issuing a token.
        var user = CreateLocalUser();
        _mockUserManager.Setup(x => x.FindByNameAsync("testuser")).ReturnsAsync(user);
        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, "Password123!", true))
            .ReturnsAsync(SignInResult.Success);
        _mockUserIdentityStore
            .Setup(s => s.ExistsActive(user.Id, LoginProviders.Wayd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new LoginCommand("testuser", "Password123!");

        var act = () => _sut.GetTokenAsync(command, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid credentials.");
        _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task GetTokenAsync_ShouldUpdateRefreshToken_WhenLoginSucceeds()
    {
        // Arrange
        var user = CreateLocalUser();
        SeedActiveWaydIdentity(user.Id);
        var command = new LoginCommand("testuser", "Password123!");

        _mockUserManager.Setup(x => x.FindByNameAsync("testuser")).ReturnsAsync(user);
        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, "Password123!", true))
            .ReturnsAsync(SignInResult.Success);
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        // Act
        await _sut.GetTokenAsync(command, TestContext.Current.CancellationToken);

        // Assert
        user.RefreshToken.Should().NotBeNullOrWhiteSpace();
        user.RefreshTokenExpiryTime.Should().NotBeNull();
        _mockUserManager.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    #endregion

    #region RefreshTokenAsync

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnNewTokens_WhenRefreshTokenIsValid()
    {
        // Arrange - first get a valid token
        var user = CreateLocalUser();
        SeedActiveWaydIdentity(user.Id);
        user.RefreshToken = "valid-refresh-token";
        user.RefreshTokenExpiryTime = _dateTimeProvider.Now.ToDateTimeUtc().AddDays(7);

        var command = new LoginCommand("testuser", "Password123!");
        _mockUserManager.Setup(x => x.FindByNameAsync("testuser")).ReturnsAsync(user);
        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, "Password123!", true))
            .ReturnsAsync(SignInResult.Success);
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var initialTokenResponse = await _sut.GetTokenAsync(command, TestContext.Current.CancellationToken);

        // Update the user's refresh token to match what was generated
        var currentRefreshToken = user.RefreshToken;
        _mockUserManager.Setup(x => x.FindByIdAsync("user-1")).ReturnsAsync(user);

        // Advance time so the new token has a different expiry
        _dateTimeProvider.Advance(Duration.FromMinutes(5));

        var refreshCommand = new RefreshTokenCommand(initialTokenResponse.Token, currentRefreshToken!);

        // Act
        var result = await _sut.RefreshTokenAsync(refreshCommand, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBe(currentRefreshToken, "a new refresh token should be generated");
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldThrowUnauthorized_WhenRefreshTokenDoesNotMatch()
    {
        // Arrange - get a valid JWT first
        var user = CreateLocalUser();
        SeedActiveWaydIdentity(user.Id);
        user.RefreshToken = "stored-refresh-token";
        user.RefreshTokenExpiryTime = _dateTimeProvider.Now.ToDateTimeUtc().AddDays(7);

        _mockUserManager.Setup(x => x.FindByNameAsync("testuser")).ReturnsAsync(user);
        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, "Password123!", true))
            .ReturnsAsync(SignInResult.Success);
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var tokenResponse = await _sut.GetTokenAsync(new LoginCommand("testuser", "Password123!"), TestContext.Current.CancellationToken);
        _mockUserManager.Setup(x => x.FindByIdAsync("user-1")).ReturnsAsync(user);

        // Force the stored refresh token to differ
        user.RefreshToken = "different-stored-token";
        var refreshCommand = new RefreshTokenCommand(tokenResponse.Token, "wrong-refresh-token");

        // Act
        var act = () => _sut.RefreshTokenAsync(refreshCommand, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid or expired refresh token.");
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldThrowUnauthorized_WhenRefreshTokenIsExpired()
    {
        // Arrange
        var user = CreateLocalUser();
        SeedActiveWaydIdentity(user.Id);

        _mockUserManager.Setup(x => x.FindByNameAsync("testuser")).ReturnsAsync(user);
        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, "Password123!", true))
            .ReturnsAsync(SignInResult.Success);
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var tokenResponse = await _sut.GetTokenAsync(new LoginCommand("testuser", "Password123!"), TestContext.Current.CancellationToken);
        var currentRefreshToken = user.RefreshToken;

        _mockUserManager.Setup(x => x.FindByIdAsync("user-1")).ReturnsAsync(user);

        // Set expiry to the past
        user.RefreshTokenExpiryTime = _dateTimeProvider.Now.ToDateTimeUtc().AddDays(-1);

        var refreshCommand = new RefreshTokenCommand(tokenResponse.Token, currentRefreshToken!);

        // Act
        var act = () => _sut.RefreshTokenAsync(refreshCommand, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid or expired refresh token.");
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldThrowUnauthorized_WhenUserIsInactive()
    {
        // Arrange
        var user = CreateLocalUser();
        SeedActiveWaydIdentity(user.Id);

        _mockUserManager.Setup(x => x.FindByNameAsync("testuser")).ReturnsAsync(user);
        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, "Password123!", true))
            .ReturnsAsync(SignInResult.Success);
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var tokenResponse = await _sut.GetTokenAsync(new LoginCommand("testuser", "Password123!"), TestContext.Current.CancellationToken);
        var currentRefreshToken = user.RefreshToken;

        // Deactivate user after login
        user.IsActive = false;
        _mockUserManager.Setup(x => x.FindByIdAsync("user-1")).ReturnsAsync(user);

        var refreshCommand = new RefreshTokenCommand(tokenResponse.Token, currentRefreshToken!);

        // Act
        var act = () => _sut.RefreshTokenAsync(refreshCommand, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("User account is inactive.");
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldThrowUnauthorized_WhenWaydIdentityDeactivatedAfterLogin()
    {
        // Simulates admin revocation of the user's local login between issuing the
        // initial token and the refresh. The refresh must fail — otherwise a revoked
        // user could keep minting access tokens until the refresh-token TTL elapses.
        var user = CreateLocalUser();
        SeedActiveWaydIdentity(user.Id);

        _mockUserManager.Setup(x => x.FindByNameAsync("testuser")).ReturnsAsync(user);
        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, "Password123!", true))
            .ReturnsAsync(SignInResult.Success);
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var tokenResponse = await _sut.GetTokenAsync(new LoginCommand("testuser", "Password123!"), TestContext.Current.CancellationToken);
        var currentRefreshToken = user.RefreshToken;

        _mockUserManager.Setup(x => x.FindByIdAsync("user-1")).ReturnsAsync(user);

        // Deactivate the Wayd identity after the initial token was issued.
        _mockUserIdentityStore
            .Setup(s => s.ExistsActive(user.Id, LoginProviders.Wayd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var refreshCommand = new RefreshTokenCommand(tokenResponse.Token, currentRefreshToken!);

        var act = () => _sut.RefreshTokenAsync(refreshCommand, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid credentials.");
    }

    #endregion

    #region Permission claims

    [Fact]
    public async Task GetTokenAsync_ShouldEmbedPermissionClaims_WhenUserHasPermissions()
    {
        var user = CreateLocalUser();
        SeedActiveWaydIdentity(user.Id);
        _mockUserService
            .Setup(s => s.GetPermissionsAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>
            {
                "Permissions.Projects.View",
                "Permissions.Projects.Manage",
                "Permissions.Teams.View",
            });

        _mockUserManager.Setup(x => x.FindByNameAsync("testuser")).ReturnsAsync(user);
        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, "Password123!", true))
            .ReturnsAsync(SignInResult.Success);
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var result = await _sut.GetTokenAsync(new LoginCommand("testuser", "Password123!"), TestContext.Current.CancellationToken);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(result.Token);
        var permissionClaims = jwt.Claims.Where(c => c.Type == "permission").Select(c => c.Value).ToList();
        permissionClaims.Should().BeEquivalentTo(
            "Permissions.Projects.View",
            "Permissions.Projects.Manage",
            "Permissions.Teams.View");
    }

    [Fact]
    public async Task GetTokenAsync_ShouldEmbedNoPermissionClaims_WhenUserHasNoPermissions()
    {
        // Default mock returns empty list — verify no permission claims are added.
        var user = CreateLocalUser();
        SeedActiveWaydIdentity(user.Id);

        _mockUserManager.Setup(x => x.FindByNameAsync("testuser")).ReturnsAsync(user);
        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, "Password123!", true))
            .ReturnsAsync(SignInResult.Success);
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var result = await _sut.GetTokenAsync(new LoginCommand("testuser", "Password123!"), TestContext.Current.CancellationToken);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(result.Token);
        jwt.Claims.Where(c => c.Type == "permission").Should().BeEmpty();
    }

    #endregion

    #region ExchangeTokenAsync

    private void SeedActiveEntraIdentity(string userId) =>
        _mockUserIdentityStore
            .Setup(s => s.ExistsActive(userId, LoginProviders.MicrosoftEntraId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

    private static ClaimsPrincipal CreateEntraPrincipal(string objectId = "entra-oid-1", string tenantId = "tenant-1", string email = "entra@example.com", string firstName = "Entra", string lastName = "User")
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", objectId),
            new Claim("tid", tenantId),
            new Claim(ClaimTypes.Upn, email),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, firstName + " " + lastName),
            new Claim(ClaimTypes.GivenName, firstName),
            new Claim(ClaimTypes.Surname, lastName),
        }, authenticationType: "Test");
        return new ClaimsPrincipal(identity);
    }

    private static ApplicationUser CreateEntraUser(string id = "entra-user-1") =>
        new()
        {
            Id = id,
            UserName = "entra-user",
            Email = "entra@example.com",
            FirstName = "Entra",
            LastName = "User",
            IsActive = true,
            LoginProvider = LoginProviders.MicrosoftEntraId,
        };

    [Fact]
    public async Task ExchangeTokenAsync_ShouldReturnTokenResponse_WhenTokenIsValid()
    {
        var user = CreateEntraUser();
        var principal = CreateEntraPrincipal();

        _mockEntraIdTokenValidator
            .Setup(v => v.Validate("entra-id-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(principal);
        _mockUserService
            .Setup(s => s.GetOrCreateFromPrincipalAsync(principal))
            .ReturnsAsync((user.Id, (string?)null));
        _mockUserManager.Setup(x => x.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        SeedActiveEntraIdentity(user.Id);

        var result = await _sut.ExchangeTokenAsync(
            new ExchangeTokenCommand(LoginProviders.MicrosoftEntraId, "entra-id-token"),
            TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(result.Token);
        jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value.Should().Be(user.Id);
    }

    [Fact]
    public async Task ExchangeTokenAsync_ShouldEmbedPermissionClaims()
    {
        var user = CreateEntraUser();
        var principal = CreateEntraPrincipal();

        _mockEntraIdTokenValidator
            .Setup(v => v.Validate(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(principal);
        _mockUserService
            .Setup(s => s.GetOrCreateFromPrincipalAsync(principal))
            .ReturnsAsync((user.Id, (string?)null));
        _mockUserService
            .Setup(s => s.GetPermissionsAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "Permissions.Projects.View" });
        _mockUserManager.Setup(x => x.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        SeedActiveEntraIdentity(user.Id);

        var result = await _sut.ExchangeTokenAsync(
            new ExchangeTokenCommand(LoginProviders.MicrosoftEntraId, "token"),
            TestContext.Current.CancellationToken);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(result.Token);
        jwt.Claims.Where(c => c.Type == "permission").Select(c => c.Value).Should().Contain("Permissions.Projects.View");
    }

    [Fact]
    public async Task ExchangeTokenAsync_ShouldThrowUnauthorized_WhenProviderIsUnsupported()
    {
        var act = () => _sut.ExchangeTokenAsync(
            new ExchangeTokenCommand("Auth0", "some-token"),
            TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("Invalid token.");
        _mockEntraIdTokenValidator.Verify(v => v.Validate(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExchangeTokenAsync_ShouldThrowUnauthorized_WhenValidatorRejectsToken()
    {
        _mockEntraIdTokenValidator
            .Setup(v => v.Validate("bad-token", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedException("Invalid token."));

        var act = () => _sut.ExchangeTokenAsync(
            new ExchangeTokenCommand(LoginProviders.MicrosoftEntraId, "bad-token"),
            TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("Invalid token.");
        _mockUserService.Verify(s => s.GetOrCreateFromPrincipalAsync(It.IsAny<ClaimsPrincipal>()), Times.Never);
    }

    [Fact]
    public async Task ExchangeTokenAsync_ShouldThrowUnauthorized_WhenUserIsInactive()
    {
        var user = CreateEntraUser();
        user.IsActive = false;
        var principal = CreateEntraPrincipal();

        _mockEntraIdTokenValidator
            .Setup(v => v.Validate(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(principal);
        _mockUserService
            .Setup(s => s.GetOrCreateFromPrincipalAsync(principal))
            .ReturnsAsync((user.Id, (string?)null));
        _mockUserManager.Setup(x => x.FindByIdAsync(user.Id)).ReturnsAsync(user);

        var act = () => _sut.ExchangeTokenAsync(
            new ExchangeTokenCommand(LoginProviders.MicrosoftEntraId, "token"),
            TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("*deactivated*");
    }

    [Fact]
    public async Task ExchangeTokenAsync_ShouldThrowUnauthorized_WhenNoActiveEntraIdentity()
    {
        var user = CreateEntraUser();
        var principal = CreateEntraPrincipal();

        _mockEntraIdTokenValidator
            .Setup(v => v.Validate(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(principal);
        _mockUserService
            .Setup(s => s.GetOrCreateFromPrincipalAsync(principal))
            .ReturnsAsync((user.Id, (string?)null));
        _mockUserManager.Setup(x => x.FindByIdAsync(user.Id)).ReturnsAsync(user);
        // Note: SeedActiveEntraIdentity NOT called → ExistsActive returns default (false).

        var act = () => _sut.ExchangeTokenAsync(
            new ExchangeTokenCommand(LoginProviders.MicrosoftEntraId, "token"),
            TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("Invalid credentials.");
    }

    #endregion

    #region AuthProviders + disabled Entra

    [Fact]
    public async Task ExchangeTokenAsync_ShouldThrow503_WhenEntraIsDisabled()
    {
        // Local-only deployment — Enabled=false means the endpoint returns 503
        // rather than 401. Distinguishes "feature off" from "bad token".
        _entraSettings.Enabled = false;

        var act = () => _sut.ExchangeTokenAsync(
            new ExchangeTokenCommand(LoginProviders.MicrosoftEntraId, "any-token"),
            TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ServiceUnavailableException>()
            .WithMessage("*not enabled*");
        _mockEntraIdTokenValidator.Verify(v => v.Validate(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void GetAuthProviders_ShouldReportLocalTrueAndEntraFalse_WhenEntraDisabled()
    {
        _entraSettings.Enabled = false;

        var result = _sut.GetAuthProviders();

        result.Local.Should().BeTrue();
        result.Entra.Should().BeFalse();
    }

    [Fact]
    public void GetAuthProviders_ShouldReportBothTrue_WhenEntraEnabled()
    {
        _entraSettings.Enabled = true;

        var result = _sut.GetAuthProviders();

        result.Local.Should().BeTrue();
        result.Entra.Should().BeTrue();
    }

    #endregion

    #region GetSettings

    [Fact]
    public async Task GetTokenAsync_ShouldThrowInvalidOperation_WhenJwtSettingsNotConfigured()
    {
        // Arrange
        var emptyConfig = new ConfigurationBuilder().Build();
        var sut = new TokenService(
            _mockUserManager.Object,
            _mockSignInManager.Object,
            emptyConfig,
            _dateTimeProvider,
            _mockUserIdentityStore.Object,
            _mockUserService.Object,
            _mockEntraIdTokenValidator.Object,
            Options.Create(_entraSettings),
            _mockLogger.Object);

        var user = CreateLocalUser();
        SeedActiveWaydIdentity(user.Id);
        _mockUserManager.Setup(x => x.FindByNameAsync("testuser")).ReturnsAsync(user);
        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, "Password123!", true))
            .ReturnsAsync(SignInResult.Success);

        var command = new LoginCommand("testuser", "Password123!");

        // Act
        var act = () => sut.GetTokenAsync(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Local JWT settings are not configured.");
    }

    #endregion
}
