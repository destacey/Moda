using Moda.Common.Extensions;

namespace Moda.AppIntegration.Domain.Models;
public sealed class AzureDevOpsBoardsProcess
{
    private string _name = null!;
    private string? _description;

    private AzureDevOpsBoardsProcess() { }
    private AzureDevOpsBoardsProcess(Guid externalId, string name, string? description, List<Guid> workspaceIds, bool isEnabled)
    {
        ExternalId = externalId;
        Name = name;
        Description = description;
        WorkspaceIds = workspaceIds;
        IsEnabled = isEnabled;
    }

    public Guid ExternalId { get; }

    public string Name
    {
        get => _name;
        private set => _name = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    }

    public string? Description
    {
        get => _description;
        private set => _description = value.NullIfWhiteSpacePlusTrim();
    }

    public List<Guid> WorkspaceIds { get; set; } = new();

    public bool IsEnabled { get; private set; }

    public Result Update(string name, string? description, List<Guid> workspaceIds, bool isEnabled)
    {
        try
        {
            Name = name;
            Description = description;
            IsEnabled = isEnabled;
            WorkspaceIds = workspaceIds;

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    public static AzureDevOpsBoardsProcess Create(Guid id, string name, string? description, List<Guid> workspaceIds, bool isEnabled)
    {
        return new AzureDevOpsBoardsProcess(id, name, description, workspaceIds, isEnabled);
    }
}
