using Microsoft.Extensions.Configuration;

namespace Moda.Infrastructure.GraphApi;

public class GraphApiSettings
{
    public static string SectionName { get; } = "SecuritySettings:GraphApi";

    public bool Enabled { get; set; } = false;
    public string? BaseUrl { get; set; }
    public string? Scopes { get; set; }
    public string? AllEmployeesGroupObjectId { get; set; }

    public static GraphApiSettings GetConfig(IConfiguration configuration)
    {
        var settings = new GraphApiSettings();
        configuration.GetSection(SectionName).Bind(settings);
        return settings;
    }
}
