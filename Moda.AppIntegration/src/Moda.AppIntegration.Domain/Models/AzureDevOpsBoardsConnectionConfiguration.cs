using System.Text.Json.Serialization;

namespace Moda.AppIntegration.Domain.Models;
public sealed record AzureDevOpsBoardsConnectionConfiguration
{
    // TODO: Move _baseUrl to a configuration file.
    private readonly string _baseUrl = "https://dev.azure.com";

    //private string _organization = null!;
    //private string _personalAccessToken = null!;
    //private readonly List<AzureDevOpsBoardsWorkspace> _workspaces = new();
    
    private AzureDevOpsBoardsConnectionConfiguration() { }
    public AzureDevOpsBoardsConnectionConfiguration(string organization, string personalAccessToken)
    {
        Organization = organization;
        PersonalAccessToken = personalAccessToken;
        Workspaces = new List<AzureDevOpsBoardsWorkspace>();
    }
    public AzureDevOpsBoardsConnectionConfiguration(string organization, string personalAccessToken, IEnumerable<AzureDevOpsBoardsWorkspace> workspaces)
    {
        Organization = organization;
        PersonalAccessToken = personalAccessToken;
        Workspaces = workspaces.ToList();
    }

    /// <summary>Gets the organization.</summary>
    /// <value>The Azure DevOps Organization name.</value>
    public string Organization { get; private set; } = "invalid";
    //public string Organization
    //{
    //    get => _organization;
    //    private set => _organization = Guard.Against.NullOrWhiteSpace(value, nameof(Organization)).Trim();
    //}

    /// <summary>Gets the personal access token.</summary>
    /// <value>The personal access token that enables access to Azure DevOps Boards data.</value>
    public string PersonalAccessToken { get; private set; } = "invalid";
    //public string PersonalAccessToken
    //{
    //    get => _personalAccessToken;
    //    private set => _personalAccessToken = Guard.Against.NullOrWhiteSpace(value, nameof(PersonalAccessToken)).Trim();
    //}

    public string OrganizationUrl
        => $"{_baseUrl}/{Organization}";

    public List<AzureDevOpsBoardsWorkspace>? Workspaces { get; private set; } =
    new List<AzureDevOpsBoardsWorkspace>();

    //public IReadOnlyCollection<AzureDevOpsBoardsWorkspace>? Workspaces => _workspaces.AsReadOnly();

    internal Result AddWorkspace(AzureDevOpsBoardsWorkspace workspace)
    {
        try
        {
            //Guard.Against.Null(workspace, nameof(workspace));

            //if (_workspaces.Any(w => w.Id == workspace.Id))
            //    return Result.Failure("Unable to add a duplicate workspace.");

            //_workspaces.Add(workspace);

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
            //Guard.Against.Null(workspace, nameof(workspace));

            //if (!_workspaces.Any(w => w.Id == workspace.Id))
            //    return Result.Failure("Unable to remove a workspace that does not exist.");

            //_workspaces.Remove(workspace);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }
}
