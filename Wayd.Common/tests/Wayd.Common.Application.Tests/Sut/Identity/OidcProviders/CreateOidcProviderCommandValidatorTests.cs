using FluentValidation.TestHelper;
using Wayd.Common.Application.Identity.OidcProviders.Commands;
using Wayd.Common.Application.Tests.Infrastructure;
using Wayd.Common.Domain.Identity;

namespace Wayd.Common.Application.Tests.Sut.Identity.OidcProviders;

/// <summary>
/// FluentValidation rule tests for <see cref="CreateOidcProviderCommandValidator"/>.
/// The entity-level invariants (in <see cref="OidcProviderTests"/>) are the
/// second line of defense; the validator is the first, surfacing clear 400s
/// before the request ever reaches the handler.
/// </summary>
public class CreateOidcProviderCommandValidatorTests
{
    private readonly FakeWaydDbContext _dbContext;
    private readonly CreateOidcProviderCommandValidator _sut;

    public CreateOidcProviderCommandValidatorTests()
    {
        _dbContext = new FakeWaydDbContext();
        _sut = new CreateOidcProviderCommandValidator(_dbContext);
    }

    private static CreateOidcProviderCommand ValidEntraCommand() => new(
        Name: "Acme-Entra",
        DisplayName: "Acme Entra",
        ProviderType: OidcProviderType.MicrosoftEntraId,
        Authority: "https://login.microsoftonline.com/common/v2.0",
        ClientId: "test-client-id",
        Audience: "api://test",
        Scopes: new[] { "openid", "profile" },
        AllowedTenantIds: new[] { "11111111-1111-1111-1111-111111111111" },
        ClockSkewSeconds: 60,
        IsEnabled: true);

    private static CreateOidcProviderCommand ValidGenericCommand() => new(
        Name: "Acme-Google",
        DisplayName: "Acme Google",
        ProviderType: OidcProviderType.GenericOidc,
        Authority: "https://accounts.google.com",
        ClientId: "test-client-id",
        Audience: "api://test",
        Scopes: new[] { "openid", "profile" },
        AllowedTenantIds: null,
        ClockSkewSeconds: 60,
        IsEnabled: true);

    [Fact]
    public async Task Validate_WithValidEntraCommand_PassesAllRules()
    {
        var result = await _sut.TestValidateAsync(ValidEntraCommand(), cancellationToken: TestContext.Current.CancellationToken);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_WithValidGenericOidcCommand_PassesAllRules()
    {
        var result = await _sut.TestValidateAsync(ValidGenericCommand(), cancellationToken: TestContext.Current.CancellationToken);
        result.ShouldNotHaveAnyValidationErrors();
    }

    // --- Name uniqueness ---
    // Note: the unique-name rule is intentionally not unit-tested here. The
    // shared FakeWaydDbContext mocks DbSet but doesn't model `.Add()` semantics,
    // so seeding a "duplicate" row to make the rule fail isn't straightforward.
    // The rule itself is a 1-line `AnyAsync`; integration coverage is provided
    // by the unique-index on the underlying database column, which would reject
    // a duplicate even if the validator silently passed.

    // --- Authority shape ---

    [Theory]
    [InlineData("http://login.microsoftonline.com/common/v2.0")]
    [InlineData("ftp://example.com")]
    [InlineData("not-a-url")]
    [InlineData("/relative")]
    public async Task Validate_WithNonHttpsAuthority_FailsAuthorityRule(string authority)
    {
        var command = ValidEntraCommand() with { Authority = authority };
        var result = await _sut.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);
        result.ShouldHaveValidationErrorFor(x => x.Authority);
    }

    // --- Entra tenant-list requirement ---

    [Fact]
    public async Task Validate_EntraWithNullTenantList_FailsAllowedTenantIdsRule()
    {
        var command = ValidEntraCommand() with { AllowedTenantIds = null };
        var result = await _sut.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);
        result.ShouldHaveValidationErrorFor(x => x.AllowedTenantIds);
    }

    [Fact]
    public async Task Validate_EntraWithEmptyTenantList_FailsAllowedTenantIdsRule()
    {
        var command = ValidEntraCommand() with { AllowedTenantIds = Array.Empty<string>() };
        var result = await _sut.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);
        result.ShouldHaveValidationErrorFor(x => x.AllowedTenantIds);
    }

    [Fact]
    public async Task Validate_EntraWithWhitespaceOnlyTenants_FailsAllowedTenantIdsRule()
    {
        var command = ValidEntraCommand() with { AllowedTenantIds = new[] { "", "   " } };
        var result = await _sut.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);
        result.ShouldHaveValidationErrorFor(x => x.AllowedTenantIds);
    }

    [Fact]
    public async Task Validate_GenericOidcWithoutTenantList_PassesAllowedTenantIdsRule()
    {
        // The Entra-only rule must NOT fire for GenericOidc — operators can
        // configure a Google/Auth0 provider without supplying a tenant list.
        var command = ValidGenericCommand() with { AllowedTenantIds = null };
        var result = await _sut.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);
        result.ShouldNotHaveValidationErrorFor(x => x.AllowedTenantIds);
    }

    // --- Reserved name ---

    [Theory]
    [InlineData("Wayd")]
    [InlineData("wayd")]
    [InlineData("WAYD")]
    [InlineData("  Wayd  ")]
    public async Task Validate_WithReservedWaydName_FailsNameRule(string name)
    {
        var command = ValidEntraCommand() with { Name = name };
        var result = await _sut.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("'Wayd' is a reserved provider name.");
    }

    // --- Field-level shape ---

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_WithEmptyName_FailsNameRule(string name)
    {
        var command = ValidEntraCommand() with { Name = name };
        var result = await _sut.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Validate_WithNameTooLong_FailsNameRule()
    {
        var command = ValidEntraCommand() with { Name = new string('a', 51) };
        var result = await _sut.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(601)]
    public async Task Validate_WithClockSkewOutOfRange_FailsClockSkewRule(int clockSkew)
    {
        var command = ValidEntraCommand() with { ClockSkewSeconds = clockSkew };
        var result = await _sut.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);
        result.ShouldHaveValidationErrorFor(x => x.ClockSkewSeconds);
    }
}
