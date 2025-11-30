using Moda.Common.Application.Identity.PersonalAccessTokens;
using NodaTime;

namespace Moda.Web.Api.Models.UserManagement.PersonalAccessTokens;

public sealed record CreatePersonalAccessTokenRequest
{
    /// <summary>
    /// Gets or sets the user-friendly name for this token.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the optional expiration date for the token.
    /// If not provided, defaults to 1 year from creation.
    /// Maximum allowed is 2 years from creation.
    /// </summary>
    public Instant? ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the optional scopes/permissions to limit this token to.
    /// If null or empty, token has full access (all user permissions).
    /// </summary>
    public string? Scopes { get; set; }

    public CreatePersonalAccessTokenCommand ToCommand()
        => new()
        {
            Name = Name,
            ExpiresAt = ExpiresAt,
            Scopes = Scopes
        };
}

public sealed class CreatePersonalAccessTokenRequestValidator : CustomValidator<CreatePersonalAccessTokenRequest>
{
    public CreatePersonalAccessTokenRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(p => p.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(p => p.Scopes)
            .MaximumLength(4000)
            .When(p => !string.IsNullOrWhiteSpace(p.Scopes));
    }
}
