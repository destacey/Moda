using Moda.Common.Domain.Enums.AppIntegrations;
using Moda.Common.Extensions;

namespace Moda.AppIntegration.Domain.Models;
public abstract class Connection : BaseSoftDeletableEntity<Guid>, IActivatable
{
    private string _name = default!;
    private string? _description;
    private string? _systemId;

    /// <summary>
    /// The name of the connection.
    /// </summary>
    public string Name
    {
        get => _name;
        protected set => _name = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    }

    /// <summary>
    /// The description of the connection.
    /// </summary>
    public string? Description
    {
        get => _description;
        protected set => _description = value.NullIfWhiteSpacePlusTrim();
    }

    // TODO: This not be on the generic connection, but rather on the specific connection details.  We will need to refactor this in the future to support that.
    /// <summary>
    /// The unique identifier for the system that this connection connects to.
    /// </summary>
    public string? SystemId 
    { 
        get => _systemId; 
        protected set => _systemId = value.NullIfWhiteSpacePlusTrim(); 
    }

    /// <summary>
    /// The connector type for the connection.
    /// </summary>
    public Connector Connector { get; protected set; }

    /// <summary>
    /// Indicates whether the connection is active or not.  Inactive connection are not included in the synchronization process.
    /// </summary>
    public bool IsActive { get; protected set; } = false;

    /// <summary>
    /// The value indicating whether this instance has a valid configuration.
    /// </summary>
    public bool IsValidConfiguration { get; protected set; } = false;

    // TODO: This not be on the generic connection, but rather on the specific connection details.  We will need to refactor this in the future to support that.
    /// <summary>
    /// The indicator for whether the connection is enabled for synchronization.
    /// </summary>
    public bool IsSyncEnabled { get; private set; } = false;

    /// <summary>
    /// The indicator for whether the connection has any active integration objects.
    /// </summary>
    public abstract bool HasActiveIntegrationObjects { get; }

    /// <summary>
    /// The indicator for whether the connection can be synchronized.
    /// </summary>
    public bool CanSync => IsActive && IsValidConfiguration && IsSyncEnabled;

    /// <summary>
    /// The process for activating a connector.
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns>Result that indicates success or a list of errors</returns>
    public virtual Result Activate(Instant timestamp)
    {
        if (!IsActive)
        {
            // Rules
            // AzDO Organization uniqueness is currently enforced by the command
            IsActive = true;
            AddDomainEvent(EntityActivatedEvent.WithEntity(this, timestamp));
        }

        return Result.Success();
    }

    /// <summary>
    /// The process for deactivating a connection.
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns>Result that indicates success or a list of errors</returns>
    public virtual Result Deactivate(Instant timestamp)
    {
        if (IsActive)
        {
            IsActive = false;
            IsSyncEnabled = false;
            AddDomainEvent(EntityDeactivatedEvent.WithEntity(this, timestamp));
        }

        return Result.Success();
    }

    /// <summary>
    /// The process for enabling or disabling the synchronization of the connection.
    /// </summary>
    /// <param name="isEnabled"></param>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public Result SetSyncState(bool isEnabled, Instant timestamp)
    {
        if (isEnabled)
        {
            if (!IsValidConfiguration)
                return Result.Failure("Unable to turn on sync. The connector configuration is not valid.");

            if (!HasActiveIntegrationObjects)
                return Result.Failure("Unable to turn on sync. The connector does not have any active integration objects.");
        }

        IsSyncEnabled = isEnabled;
        AddDomainEvent(EntityUpdatedEvent.WithEntity(this, timestamp));
        return Result.Success();
    }
}

public abstract class Connection<TC> : Connection
{
    /// <summary>
    /// The connection configuration.
    /// </summary>
    public abstract TC Configuration { get; protected set; }
}
