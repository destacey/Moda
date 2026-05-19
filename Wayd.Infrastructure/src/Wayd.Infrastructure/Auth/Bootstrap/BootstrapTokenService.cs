using System.Security.Cryptography;
using Wayd.Common.Application.Identity.Bootstrap;

namespace Wayd.Infrastructure.Auth.Bootstrap;

/// <summary>
/// Singleton. Holds a one-time bootstrap token in memory, generated only when
/// the database has no users. The token is consumed on first successful use and
/// is never persisted or logged in full — only printed to stdout on startup.
/// </summary>
internal sealed class BootstrapTokenService : IBootstrapTokenService
{
    private string? _token;

    public bool IsActive => _token is not null;

    /// <summary>
    /// Generates and stores the bootstrap token. Must be called once from the
    /// startup path when user count is confirmed to be zero.
    /// </summary>
    internal string Generate()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        _token = Convert.ToBase64String(bytes);
        return _token;
    }

    public bool TryConsume(string token)
    {
        if (_token is null)
            return false;

        // Constant-time comparison to prevent timing attacks.
        var match = CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.UTF8.GetBytes(token),
            System.Text.Encoding.UTF8.GetBytes(_token));

        if (match)
            _token = null;

        return match;
    }
}
