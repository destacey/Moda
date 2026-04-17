namespace Wayd.Infrastructure.Auth.Local;

public sealed class LocalJwtSettings
{
    public const string SectionName = "SecuritySettings:LocalJwt";

    public string Secret { get; set; } = null!;
    public string Issuer { get; set; } = "Moda";
    public string Audience { get; set; } = "ModaApi";
    public int TokenExpirationInMinutes { get; set; } = 60;
    public int RefreshTokenExpirationInDays { get; set; } = 7;
}
