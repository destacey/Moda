using Moda.Common.Extensions;

namespace Moda.AppIntegration.Domain.Models;
public sealed class AzureDevOpsBoardsWorkspace : BaseAuditableEntity<Guid>
{
    private string _name = null!;
    private string? _description;

    private AzureDevOpsBoardsWorkspace() { }
    private AzureDevOpsBoardsWorkspace(Guid id, string name, string? description, bool import)
    {
        Id = id;
        Name = name;
        Description = description;
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

    public static AzureDevOpsBoardsWorkspace Create(Guid id, string name, string? description, Instant timestamp)
    {
        var workspace = new AzureDevOpsBoardsWorkspace(id, name, description, false);
        workspace.AddDomainEvent(EntityCreatedEvent.WithEntity(workspace, timestamp));
        return workspace;
    }
}
