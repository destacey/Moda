﻿using Moda.Common.Extensions;

namespace Moda.AppIntegration.Domain.Models;
public abstract class Connection : BaseSoftDeletableEntity<Guid>, IActivatable
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
        protected set => _description = value.NullIfWhiteSpacePlusTrim();
    }

    /// <summary>Gets the type of connector.  This value cannot change.</summary>
    /// <value>The type of connector.</value>
    public Connector Connector { get; protected set; }

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
    /// The indicator for whether the connection is enabled for synchronization.
    /// </summary>
    public bool IsSyncEnabled { get; private set; } = false;

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

public abstract class Connection<TC,TT> : Connection
{
    /// <summary>Gets the configuration.</summary>
    /// <value>The configuration.</value>
    public abstract TC Configuration { get; protected set; }
    
    /// <summary>
    /// Gets the team configuration.
    /// </summary>
    public abstract TT TeamConfiguration { get; protected set; }
}
