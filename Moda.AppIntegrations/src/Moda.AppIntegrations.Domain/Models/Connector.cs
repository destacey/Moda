namespace Moda.AppIntegrations.Domain.Models;
public abstract class Connector<T> : BaseAuditableEntity<Guid>, IAggregateRoot, IActivatable
{
    private string _name = null!;
    private string? _description;

    protected Connector() { }

    public Connector(string name, string? description, ConnectorType type, T configuration)
    {
        Name = name;
        Description = description;
        Type = type;
        Configuration = configuration;
    }


    /// <summary>Gets or sets the name of the connector.</summary>
    /// <value>The name of the connector.</value>
    public string Name
    {
        get => _name;
        protected set => _name = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    }

    /// <summary>Gets or sets the description.</summary>
    /// <value>The connector description.</value>
    public string? Description 
    { 
        get => _description;
        protected set => _description = value?.Trim(); 
    }

    /// <summary>Gets the type of connector.  This value cannot change.</summary>
    /// <value>The type of connector.</value>
    public ConnectorType Type { get; }

    // TODO string ConfigurationObject - should there be a class for each type?
    public T? Configuration { get; protected set; }

    /// <summary>
    /// Indicates whether the connector is active or not.  Inactive connectors are not included in the synchronization process.
    /// </summary>
    public bool IsActive { get; protected set; } = true;

    public bool IsValidConfiguration { get; protected set; }

    /// <summary>
    /// The process for activating a connector.
    /// </summary>
    /// <param name="activatedOn"></param>
    /// <returns>Result that indicates success or a list of errors</returns>
    public Result Activate(Instant activatedOn)
    {
        if (!IsActive)
        {
            // TODO is there logic that would prevent activation?
            IsActive = true;
            AddDomainEvent(EntityActivatedEvent.WithEntity(this, activatedOn));
        }

        return Result.Success();
    }

    /// <summary>
    /// The process for deactivating a connector.
    /// </summary>
    /// <param name="deactivatedOn"></param>
    /// <returns>Result that indicates success or a list of errors</returns>
    public Result Deactivate(Instant deactivatedOn)
    {
        if (IsActive)
        {
            // TODO is there logic that would prevent deactivation?
            IsActive = false;
            AddDomainEvent(EntityDeactivatedEvent.WithEntity(this, deactivatedOn));
        }

        return Result.Success();
    }

    public abstract void ValidateConfiguration();
}
