namespace Moda.AppIntegrations.Domain.Models;
public class Connector : BaseAuditableEntity<Guid>, IAggregateRoot, IActivatable
{
    private string _name = null!;
    private string? _description;    
    
    private Connector() { }
    
    public Connector(string name, string? description, ConnectorType type)
    {
        Name = name;
        Description = description;
        Type = type;
    }


    /// <summary>Gets or sets the name of the connector.</summary>
    /// <value>The name of the connector.</value>
    public string Name
    {
        get => _name;
        set => _name = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    }

    /// <summary>Gets or sets the description.</summary>
    /// <value>The connector description.</value>
    public string? Description 
    { 
        get => _description; 
        set => _description = value?.Trim(); 
    }

    /// <summary>Gets the type of connector.  This value cannot change.</summary>
    /// <value>The type of connector.</value>
    public ConnectorType Type { get; }
    
    // TODO string ConfigurationObject - should there be a class for each type?

    /// <summary>
    /// Indicates whether the connector is active or not.  Inactive connectors are not included in the synchronization process.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    public Result Update(string name, string? description)
    {
        Name = name;
        Description = description;

        return Result.Success();
    }

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
}
