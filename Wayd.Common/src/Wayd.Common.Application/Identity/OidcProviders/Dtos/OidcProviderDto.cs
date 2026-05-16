namespace Wayd.Common.Application.Identity.OidcProviders.Dtos;

/// <summary>
/// Admin-facing detail DTO. Includes every editable field plus audit metadata.
/// </summary>
public sealed record OidcProviderDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string DisplayName { get; init; } = null!;
    public string ProviderType { get; init; } = null!;
    public string Authority { get; init; } = null!;
    public string ClientId { get; init; } = null!;
    public string Audience { get; init; } = null!;
    public IReadOnlyList<string> Scopes { get; init; } = [];
    public IReadOnlyList<string>? AllowedTenantIds { get; init; }
    public int ClockSkewSeconds { get; init; }
    public bool IsEnabled { get; init; }
}

/// <summary>
/// Compact list row. Same fields as the detail DTO; kept as a separate type so
/// the API contract is explicit and the listing endpoint stays stable if the
/// detail shape grows fields later.
/// </summary>
public sealed record OidcProviderListItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string DisplayName { get; init; } = null!;
    public string ProviderType { get; init; } = null!;
    public string Authority { get; init; } = null!;
    public bool IsEnabled { get; init; }
}
