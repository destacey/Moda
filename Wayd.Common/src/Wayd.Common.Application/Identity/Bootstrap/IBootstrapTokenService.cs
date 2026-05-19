namespace Wayd.Common.Application.Identity.Bootstrap;

/// <summary>
/// Manages the one-time bootstrap token used to create the first admin user.
/// The token is generated in-memory on startup when no users exist, logged to
/// the console, and consumed exactly once via POST /api/auth/setup.
/// </summary>
public interface IBootstrapTokenService
{
    /// <summary>Returns true if a bootstrap token is currently active.</summary>
    bool IsActive { get; }

    /// <summary>
    /// Validates the supplied token and, if valid, consumes it (preventing reuse).
    /// Returns false if no token is active or the token does not match.
    /// </summary>
    bool TryConsume(string token);
}
