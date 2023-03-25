using Microsoft.Extensions.Configuration;

namespace Moda.Infrastructure.Auth.AzureAd;
public sealed class AzureAdSettings
{
    public const string SectionName = "SecuritySettings:AzureAd";

    public string? Instance { get; set; }
    public string? Domain { get; set; }
    public string? TenantId { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? RootIssuer { get; set; }
    public string? Scopes { get; set; }

    public static AzureAdSettings GetConfig(IConfiguration configuration)
    {
        var settings = new AzureAdSettings();
        configuration.GetSection(SectionName).Bind(settings);
        return settings;
    }
}
