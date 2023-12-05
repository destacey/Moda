using Microsoft.Extensions.Configuration;

namespace Moda.Integrations.AzureDevOps.IntegrationTests.Models;
public abstract class BaseConfiguration
{
    public BaseConfiguration(IConfiguration configuration, string sectionName)
    {
        if (configuration is null)
            throw new ArgumentNullException(nameof(configuration));

        var section = configuration.GetSection(sectionName);
        if (section is null)
            throw new ArgumentNullException(nameof(section));

        section.Bind(this);
    }
}
