using Moda.Common.Extensions;

namespace Moda.AppIntegration.Domain.Models;
public sealed class AzureDevOpsBoardsWorkspaceConfiguration : BaseAuditableEntity<Guid>
{
    private string _name = null!;
    private string? _description;

    private AzureDevOpsBoardsWorkspaceConfiguration() { }
    private AzureDevOpsBoardsWorkspaceConfiguration(Guid id, string name, string? description, Guid connectionId, bool import)
    {
        Id = id;
        Name = name;
        Description = description;
        ConnectionId = connectionId;
        Import = import;
    }

    /// <summary>Gets or sets the name of the connection.</summary>
    /// <value>The name of the connection.</value>
    public string Name
    {
        get => _name;
        private set => _name = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    }

    /// <summary>Gets or sets the description.</summary>
    /// <value>The connection description.</value>
    public string? Description
    {
        get => _description;
        private set => _description = value.NullIfWhiteSpacePlusTrim();
    }

    /// <summary>Gets the connection identifier.</summary>
    /// <value>The connection identifier.</value>
    public Guid ConnectionId { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this workspace is configured to import data.
    /// </summary>
    /// <value><c>true</c> if import; otherwise, <c>false</c>.</value>
    public bool Import { get; private set; }

    public Result Update(string name, string? description, bool import, Instant timestamp)
    {
        try
        {
            Name = name;
            Description = description;
            Import = import;

            AddDomainEvent(EntityUpdatedEvent.WithEntity(this, timestamp));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    public static AzureDevOpsBoardsWorkspaceConfiguration Create(Guid id, string name, string? description, Guid connectionId, Instant timestamp)
    {
        var workspace = new AzureDevOpsBoardsWorkspaceConfiguration(id, name, description, connectionId, false);
        workspace.AddDomainEvent(EntityCreatedEvent.WithEntity(workspace, timestamp));
        return workspace;
    }
}
