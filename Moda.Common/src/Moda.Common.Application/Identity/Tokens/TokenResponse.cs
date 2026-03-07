namespace Moda.Common.Application.Identity.Tokens;

public sealed record TokenResponse(string Token, string RefreshToken, DateTime TokenExpiresAt);
