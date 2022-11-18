namespace Moda.AppIntegrations.Domain.Models;
public sealed record AzureDevOpsBoardsConfiguration
{
    private AzureDevOpsBoardsConfiguration() { }
    public AzureDevOpsBoardsConfiguration(string? organizationUrl, string? personalAccessToken)
    {
        OrganizationUrl = organizationUrl?.Trim();
        PersonalAccessToken = personalAccessToken?.Trim();
    }

    public string? OrganizationUrl { get; }
    public string? PersonalAccessToken { get; }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(OrganizationUrl) && !string.IsNullOrWhiteSpace(PersonalAccessToken);
    }
}
