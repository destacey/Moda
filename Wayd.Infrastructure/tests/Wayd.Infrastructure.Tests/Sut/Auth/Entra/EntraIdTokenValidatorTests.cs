using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Wayd.Common.Application.Exceptions;
using Wayd.Infrastructure.Auth.Entra;

namespace Wayd.Infrastructure.Tests.Sut.Auth.Entra;

public class EntraIdTokenValidatorTests
{
    private const string TestAudience = "api://test-wayd-api";
    private const string AllowedTenantId = "11111111-1111-1111-1111-111111111111";
    private const string OtherTenantId = "22222222-2222-2222-2222-222222222222";

    private readonly RsaSecurityKey _signingKey;
    private readonly SigningCredentials _signingCredentials;
    private readonly Mock<IConfigurationManager<OpenIdConnectConfiguration>> _mockOidcConfigManager;
    private readonly CapturingLogger<EntraIdTokenValidator> _logger;

    public EntraIdTokenValidatorTests()
    {
        var rsa = RSA.Create(2048);
        _signingKey = new RsaSecurityKey(rsa) { KeyId = "test-key-1" };
        _signingCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.RsaSha256);

        var oidcConfig = new OpenIdConnectConfiguration();
        oidcConfig.SigningKeys.Add(_signingKey);

        _mockOidcConfigManager = new Mock<IConfigurationManager<OpenIdConnectConfiguration>>();
        _mockOidcConfigManager
            .Setup(m => m.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(oidcConfig);

        _logger = new CapturingLogger<EntraIdTokenValidator>();
    }

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public Exception? LastException { get; private set; }
        public string? LastMessage { get; private set; }
        IDisposable? ILogger.BeginScope<TState>(TState state) => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
        {
            LastException = exception;
            LastMessage = formatter(state, exception);
        }
    }

    // --- ExtractTenantFromIssuer (pure function) ---

    [Fact]
    public void ExtractTenantFromIssuer_WithV1Issuer_ReturnsTenantGuid()
    {
        var result = EntraIdTokenValidator.ExtractTenantFromIssuer(
            $"https://sts.windows.net/{AllowedTenantId}/");

        result.Should().Be(AllowedTenantId);
    }

    [Fact]
    public void ExtractTenantFromIssuer_WithV2Issuer_ReturnsTenantGuid()
    {
        var result = EntraIdTokenValidator.ExtractTenantFromIssuer(
            $"https://login.microsoftonline.com/{AllowedTenantId}/v2.0");

        result.Should().Be(AllowedTenantId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ExtractTenantFromIssuer_WithNullOrWhitespace_ReturnsNull(string? issuer)
    {
        var result = EntraIdTokenValidator.ExtractTenantFromIssuer(issuer);

        result.Should().BeNull();
    }

    [Fact]
    public void ExtractTenantFromIssuer_WithCommonPseudoTenant_ReturnsNull()
    {
        // 'common' is not a real tenant — it's the multi-tenant endpoint marker.
        // Accepting it would be a major allowlist hole.
        var result = EntraIdTokenValidator.ExtractTenantFromIssuer(
            "https://login.microsoftonline.com/common/v2.0");

        result.Should().BeNull();
    }

    [Fact]
    public void ExtractTenantFromIssuer_WithOrganizationsPseudoTenant_ReturnsNull()
    {
        var result = EntraIdTokenValidator.ExtractTenantFromIssuer(
            "https://login.microsoftonline.com/organizations/v2.0");

        result.Should().BeNull();
    }

    [Fact]
    public void ExtractTenantFromIssuer_WithPersonalAccountsPseudoTenant_ReturnsTheGuid()
    {
        // The MSA (personal account) issuer tenant ID 9188040d-... is a real-looking
        // GUID — the function returns it. The allowlist is what keeps it out; we do
        // NOT special-case the MSA tenant here.
        var result = EntraIdTokenValidator.ExtractTenantFromIssuer(
            "https://login.microsoftonline.com/9188040d-6c67-4c5b-b112-36a304b66dad/v2.0");

        result.Should().Be("9188040d-6c67-4c5b-b112-36a304b66dad");
    }

    [Fact]
    public void ExtractTenantFromIssuer_WithMalformedUrl_ReturnsNull()
    {
        var result = EntraIdTokenValidator.ExtractTenantFromIssuer("not a url");

        result.Should().BeNull();
    }

    [Fact]
    public void ExtractTenantFromIssuer_WithEmptyPath_ReturnsNull()
    {
        var result = EntraIdTokenValidator.ExtractTenantFromIssuer("https://login.microsoftonline.com");

        result.Should().BeNull();
    }

    // --- Validate (full pipeline) ---

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_WithEmptyToken_Throws(string? token)
    {
        var sut = CreateSut(new[] { AllowedTenantId });

        var act = () => sut.Validate(token!, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Validate_WithEmptyAllowlist_Throws()
    {
        // Runtime defense-in-depth: even if startup validation was bypassed, an
        // empty allowlist at call time must reject.
        var sut = CreateSut(Array.Empty<string>());
        var token = BuildToken(tid: AllowedTenantId);

        var act = () => sut.Validate(token, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Validate_WithOidcDiscoveryFailure_Throws()
    {
        _mockOidcConfigManager
            .Setup(m => m.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("network down"));

        var sut = CreateSut(new[] { AllowedTenantId });
        var token = BuildToken(tid: AllowedTenantId);

        var act = () => sut.Validate(token, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Validate_WithValidTokenAndAllowedTenant_ReturnsPrincipal()
    {
        var sut = CreateSut(new[] { AllowedTenantId });
        var token = BuildToken(tid: AllowedTenantId);

        var principal = await sut.Validate(token, CancellationToken.None);

        principal.Should().NotBeNull();
        principal.Identity!.IsAuthenticated.Should().BeTrue();
        principal.FindFirstValue("tid").Should().Be(AllowedTenantId);
    }

    [Fact]
    public async Task Validate_WithTenantNotInAllowlist_Throws()
    {
        // The core multi-tenant boundary: an otherwise-valid token from a tenant
        // we haven't onboarded must be rejected.
        var sut = CreateSut(new[] { AllowedTenantId });
        var token = BuildToken(tid: OtherTenantId);

        var act = () => sut.Validate(token, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Validate_WithNoTidClaim_FallsBackToIssuer()
    {
        // Personal accounts and some v2.0 edge cases omit tid. The validator
        // parses the tenant from the signed iss claim as a fallback.
        var sut = CreateSut(new[] { AllowedTenantId });
        var token = BuildToken(
            tid: null,
            issuer: $"https://login.microsoftonline.com/{AllowedTenantId}/v2.0");

        var principal = await sut.Validate(token, CancellationToken.None);

        principal.Should().NotBeNull();
    }

    [Fact]
    public async Task Validate_WithNoTidClaimAndCommonIssuer_Throws()
    {
        // With no tid AND an unrecognizable issuer, the validator has nothing
        // to allowlist against and must reject.
        var sut = CreateSut(new[] { AllowedTenantId });
        var token = BuildToken(
            tid: null,
            issuer: "https://login.microsoftonline.com/common/v2.0");

        var act = () => sut.Validate(token, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Validate_AllowlistComparisonIsCaseInsensitive()
    {
        // Operators occasionally enter tenant IDs with mixed case; Entra itself
        // always emits them lowercase. Both should match.
        var sut = CreateSut(new[] { AllowedTenantId.ToUpperInvariant() });
        var token = BuildToken(tid: AllowedTenantId); // lowercase

        var principal = await sut.Validate(token, CancellationToken.None);

        principal.Should().NotBeNull();
    }

    [Fact]
    public async Task Validate_WithWrongAudience_Throws()
    {
        var sut = CreateSut(new[] { AllowedTenantId });
        var token = BuildToken(tid: AllowedTenantId, audience: "api://some-other-app");

        var act = () => sut.Validate(token, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Validate_WithExpiredToken_Throws()
    {
        var sut = CreateSut(new[] { AllowedTenantId });
        var token = BuildToken(
            tid: AllowedTenantId,
            expires: DateTime.UtcNow.AddHours(-1));

        var act = () => sut.Validate(token, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Validate_WithTokenSignedByDifferentKey_Throws()
    {
        using var otherRsa = RSA.Create(2048);
        var otherKey = new RsaSecurityKey(otherRsa) { KeyId = "other-key" };
        var otherCreds = new SigningCredentials(otherKey, SecurityAlgorithms.RsaSha256);

        var sut = CreateSut(new[] { AllowedTenantId });
        var token = BuildToken(tid: AllowedTenantId, signingCredentials: otherCreds);

        var act = () => sut.Validate(token, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Validate_WithMalformedToken_Throws()
    {
        var sut = CreateSut(new[] { AllowedTenantId });

        var act = () => sut.Validate("not.a.jwt", CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    // --- Harness ---

    private EntraIdTokenValidator CreateSut(IReadOnlyList<string> allowedTenantIds)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(BuildConfig(allowedTenantIds))
            .Build();

        return new EntraIdTokenValidator(
            config,
            _mockOidcConfigManager.Object,
            _logger);
    }

    private static IEnumerable<KeyValuePair<string, string?>> BuildConfig(
        IReadOnlyList<string> allowedTenantIds)
    {
        yield return new("SecuritySettings:Providers:Entra:Enabled", "true");
        yield return new("SecuritySettings:Providers:Entra:Authority",
            "https://login.microsoftonline.com/common/v2.0");
        yield return new("SecuritySettings:Providers:Entra:Audience", TestAudience);
        yield return new("SecuritySettings:Providers:Entra:ClockSkewSeconds", "60");
        for (var i = 0; i < allowedTenantIds.Count; i++)
        {
            yield return new(
                $"SecuritySettings:Providers:Entra:AllowedTenantIds:{i}",
                allowedTenantIds[i]);
        }
    }

    private string BuildToken(
        string? tid,
        string? issuer = null,
        string? audience = null,
        DateTime? expires = null,
        SigningCredentials? signingCredentials = null)
    {
        var claims = new List<Claim> { new("sub", "test-subject") };
        if (!string.IsNullOrEmpty(tid))
        {
            claims.Add(new Claim("tid", tid));
        }

        var expiresAt = expires ?? DateTime.UtcNow.AddHours(1);
        // notBefore must be <= expires, but keep it in the past relative to "now"
        // so the happy-path tests aren't rejected with "not yet valid". For the
        // expired-token case, both nbf and exp end up in the past — which is what
        // we want (ValidateLifetime rejects on expiry).
        var notBefore = expiresAt < DateTime.UtcNow
            ? expiresAt.AddMinutes(-5)
            : DateTime.UtcNow.AddMinutes(-5);

        // OutboundClaimTypeMap would rewrite our short claim names (tid, sub)
        // to schema URLs when serializing — producing a token unlike what Entra
        // actually emits. Clearing it preserves the real wire shape.
        var handler = new JwtSecurityTokenHandler();
        handler.OutboundClaimTypeMap.Clear();
        var jwt = handler.CreateJwtSecurityToken(
            issuer: issuer ?? $"https://login.microsoftonline.com/{AllowedTenantId}/v2.0",
            audience: audience ?? TestAudience,
            subject: new ClaimsIdentity(claims),
            notBefore: notBefore,
            expires: expiresAt,
            issuedAt: notBefore,
            signingCredentials: signingCredentials ?? _signingCredentials);

        return handler.WriteToken(jwt);
    }
}
