using System.Text.Json;

namespace Moda.AppIntegration.Domain.Models;
public abstract class Connection : BaseAuditableEntity<Guid>, IActivatable
{
    private string _name = null!;
    private string? _description;

    /// <summary>Gets or sets the name of the connection.</summary>
    /// <value>The name of the connection.</value>
    public string Name
    {
        get => _name;
        protected set => _name = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    }

    /// <summary>Gets or sets the description.</summary>
    /// <value>The connection description.</value>
    public string? Description 
    { 
        get => _description;
        protected set => _description = value?.Trim(); 
    }

    /// <summary>Gets the type of connector.  This value cannot change.</summary>
    /// <value>The type of connector.</value>
    public Connector Connector { get; protected set; }

    public string? ConfigurationString { get; protected set; }

    /// <summary>
    /// Indicates whether the connection is active or not.  Inactive connection are not included in the synchronization process.
    /// </summary>
    /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
    public bool IsActive { get; protected set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether this instance has a valid configuration.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is valid configuration; otherwise, <c>false</c>.
    /// </value>
    public bool IsValidConfiguration { get; protected set; } = false;

    /// <summary>
    /// The process for activating a connector.
    /// </summary>
    /// <param name="activatedOn"></param>
    /// <returns>Result that indicates success or a list of errors</returns>
    public virtual Result Activate(Instant activatedOn)
    {
        if (!IsActive)
        {
            // Rules
            // AzDO Organization uniqueness is currently enforced by the command
            IsActive = true;
            AddDomainEvent(EntityActivatedEvent.WithEntity(this, activatedOn));
        }

        return Result.Success();
    }

    /// <summary>
    /// The process for deactivating a connection.
    /// </summary>
    /// <param name="deactivatedOn"></param>
    /// <returns>Result that indicates success or a list of errors</returns>
    public virtual Result Deactivate(Instant deactivatedOn)
    {
        if (IsActive)
        {
            IsActive = false;
            AddDomainEvent(EntityDeactivatedEvent.WithEntity(this, deactivatedOn));
        }

        return Result.Success();
    }

    public abstract void ValidateConfiguration();
}

public abstract class Connection<T> : Connection
{
    public T? Configuration 
    { 
        get => ConfigurationString is null ? default : JsonSerializer.Deserialize<T>(ConfigurationString);
        protected set => ConfigurationString = JsonSerializer.Serialize(value); 
    }
}
