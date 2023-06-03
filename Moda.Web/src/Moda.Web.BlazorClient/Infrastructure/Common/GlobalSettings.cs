namespace Moda.Web.BlazorClient.Infrastructure.Common;

public class GlobalSettings
{
    public const string SectionName = "Global";

    public string? DocsUrl { get; set; }

    public bool IsDocUrlEnabled
        => !string.IsNullOrWhiteSpace(DocsUrl);

    public static GlobalSettings GetConfig(IConfiguration configuration)
    {
        var settings = new GlobalSettings();
        configuration.GetSection(SectionName).Bind(settings);
        return settings;
    }
}
