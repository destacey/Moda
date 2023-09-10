﻿using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Moda.AppIntegration.Domain.Models;
public sealed class AzureDevOpsBoardsConnectionConfiguration
{
    // TODO: .net 8 provides improved json serialization options for unmapped properties

    // TODO: Move _baseUrl to a configuration file.
    private readonly string _baseUrl = "https://dev.azure.com";

    public AzureDevOpsBoardsConnectionConfiguration() { }

    [SetsRequiredMembers]
    public AzureDevOpsBoardsConnectionConfiguration(string organization, string personalAccessToken, IEnumerable<AzureDevOpsBoardsWorkspace>? workspaces = null)
    {
        Organization = organization.Trim();
        PersonalAccessToken = personalAccessToken.Trim();
        Workspaces = workspaces?.ToList() ?? new List<AzureDevOpsBoardsWorkspace>();
    }

    /// <summary>Gets the organization.</summary>
    /// <value>The Azure DevOps Organization name.</value>
    public required string Organization { get; set; }

    /// <summary>Gets the personal access token.</summary>
    /// <value>The personal access token that enables access to Azure DevOps Boards data.</value>
    public required string PersonalAccessToken { get; set; }

    public string OrganizationUrl
        => $"{_baseUrl}/{Organization}";

    [JsonInclude]
    public List<AzureDevOpsBoardsWorkspace> Workspaces { get; private set; } =
    new List<AzureDevOpsBoardsWorkspace>();

    internal Result AddWorkspace(AzureDevOpsBoardsWorkspace workspace)
    {
        try
        {
            Guard.Against.Null(workspace, nameof(workspace));

            if (Workspaces.Any(w => w.ExternalId == workspace.ExternalId))
                return Result.Failure("Unable to add a duplicate workspace.");

            Workspaces.Add(workspace);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    internal Result RemoveWorkspace(AzureDevOpsBoardsWorkspace workspace)
    {
        try
        {
            Guard.Against.Null(workspace, nameof(workspace));

            if (!Workspaces.Any(w => w.ExternalId == workspace.ExternalId))
                return Result.Failure("Unable to remove a workspace that does not exist.");

            Workspaces.Remove(workspace);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }
}
