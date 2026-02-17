namespace Moda.Web.Api.Models.AppIntegrations.Connections;

public class TestAzureDevOpsConnectionRequest
{
    /// <summary>Gets the organization.</summary>
    /// <value>The Azure DevOps Organization name.</value>
    public string Organization { get; set; } = default!;

    /// <summary>Gets the personal access token.</summary>
    /// <value>The personal access token that enables access to Azure DevOps data.</value>
    public string PersonalAccessToken { get; set; } = default!;
}
