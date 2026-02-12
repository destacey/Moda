namespace Moda.AppIntegration.Domain.Interfaces;

/// <summary>
/// Marker interface for connections that support bidirectional or unidirectional synchronization
/// with external systems. Typically implemented by Work Management connectors.
/// </summary>
public interface ISyncableConnection
{
    /// <summary>
    /// The unique identifier for the external system this connection syncs with.
    /// </summary>
    string? SystemId { get; }

    /// <summary>
    /// Indicates whether synchronization is currently enabled for this connection.
    /// </summary>
    bool IsSyncEnabled { get; }

    /// <summary>
    /// Computed property indicating whether the connection can currently sync.
    /// Requires: IsActive && IsValidConfiguration && IsSyncEnabled && HasActiveIntegrationObjects
    /// </summary>
    bool CanSync { get; }

    /// <summary>
    /// Enable or disable synchronization for this connection.
    /// </summary>
    Result SetSyncState(bool isEnabled, Instant timestamp);
}
