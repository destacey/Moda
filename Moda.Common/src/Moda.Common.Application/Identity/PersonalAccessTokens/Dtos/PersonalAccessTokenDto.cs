namespace Moda.Common.Application.Identity.PersonalAccessTokens.Dtos;

/// <summary>
/// DTO for personal access token information.
/// Note: The token value is NEVER included - it's only shown once at creation.
/// </summary>
public sealed record PersonalAccessTokenDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public Instant ExpiresAt { get; init; }
    public Instant? LastUsedAt { get; init; }
    public Instant? RevokedAt { get; init; }
    public bool IsActive { get; init; }
    public bool IsExpired { get; init; }
    public bool IsRevoked { get; init; }
    public string? Scopes { get; init; }
}
