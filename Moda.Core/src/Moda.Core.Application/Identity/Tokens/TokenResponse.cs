using NodaTime;

namespace Moda.Core.Application.Identity.Tokens;

public record TokenResponse(string Token, string RefreshToken, Instant RefreshTokenExpiryTime);