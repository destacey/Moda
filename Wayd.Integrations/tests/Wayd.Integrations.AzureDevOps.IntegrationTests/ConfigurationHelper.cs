using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Wayd.Integrations.AzureDevOps.IntegrationTests;

public static class ConfigurationHelper
{
    public static IConfigurationRoot GetConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .AddEnvironmentVariables();

        return builder.Build();
    }
}
