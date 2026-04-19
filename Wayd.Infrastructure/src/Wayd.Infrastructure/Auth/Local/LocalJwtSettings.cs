namespace Wayd.Infrastructure.Auth.Local;

public sealed class LocalJwtSettings
{
    public const string SectionName = "SecuritySettings:LocalJwt";

    public string Secret { get; set; } = null!;
    public string Issuer { get; set; } = "Wayd";
    public string Audience { get; set; } = "WaydApi";
    public int TokenExpirationInMinutes { get; set; } = 60;
    public int RefreshTokenExpirationInDays { get; set; } = 7;
}
