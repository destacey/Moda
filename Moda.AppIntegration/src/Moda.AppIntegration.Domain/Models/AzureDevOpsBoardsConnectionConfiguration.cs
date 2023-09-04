﻿namespace Moda.AppIntegration.Domain.Models;
public sealed record AzureDevOpsBoardsConnectionConfiguration
{
    // TODO: Move _baseUrl to a configuration file.
    private readonly string _baseUrl = "https://dev.azure.com";
    private AzureDevOpsBoardsConnectionConfiguration() { }
    public AzureDevOpsBoardsConnectionConfiguration(string organization, string personalAccessToken)
    {
        Organization = organization.Trim();
        PersonalAccessToken = personalAccessToken.Trim();
    }

    /// <summary>Gets the organization.</summary>
    /// <value>The Azure DevOps Organization name.</value>
    public string Organization { get; } = "invalid";

    /// <summary>Gets the personal access token.</summary>
    /// <value>The personal access token that enables access to Azure DevOps Boards data.</value>
    public string PersonalAccessToken { get; } = "invalid";

    public string OrganizationUrl
        => $"{_baseUrl}/{Organization}";
}
