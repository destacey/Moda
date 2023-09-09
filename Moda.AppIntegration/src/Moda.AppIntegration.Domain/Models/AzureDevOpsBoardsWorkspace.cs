using Moda.Common.Extensions;

namespace Moda.AppIntegration.Domain.Models;
public sealed class AzureDevOpsBoardsWorkspace
{
    private string _name = null!;
    private string? _description;

    private AzureDevOpsBoardsWorkspace() { }
    private AzureDevOpsBoardsWorkspace(Guid id, string name, string? description, bool import)
    {
        Id = id;
        Name = name;
        Description = description;
        Sync = import;
    }

    public Guid Id { get; private init; }

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
    /// Gets a value indicating whether this workspace is configured to sync data.
    /// </summary>
    /// <value><c>true</c> if sync; otherwise, <c>false</c>.</value>
    public bool Sync { get; private set; }

    public Result Update(string name, string? description, bool import, Instant timestamp)
    {
        try
        {
            Name = name;
            Description = description;
            Sync = import;

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    public static AzureDevOpsBoardsWorkspace Create(Guid id, string name, string? description, Instant timestamp)
    {
        return new AzureDevOpsBoardsWorkspace(id, name, description, false);
    }
}
