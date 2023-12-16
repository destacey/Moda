using Microsoft.Extensions.Configuration;

namespace Moda.Integrations.AzureDevOps.IntegrationTests.Models;
public sealed class AzdoOrganizationOptions : BaseConfiguration
{
    public readonly static string SectionName = "AzdoOrganization";

    public AzdoOrganizationOptions(IConfiguration configuration) : base(configuration, SectionName)
    {
    }

    public string OrganizationUrl { get; init; } = string.Empty;
    public string ApiVersion { get; init; } = string.Empty;
    public string PersonalAccessToken { get; init; } = string.Empty;
}
