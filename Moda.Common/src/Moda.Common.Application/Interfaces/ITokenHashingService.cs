namespace Moda.Common.Application.Interfaces;

/// <summary>
/// Service for securely hashing and validating personal access tokens.
/// </summary>
public interface ITokenHashingService
{
    /// <summary>
    /// Generates a cryptographically secure random token.
    /// </summary>
    /// <returns>A token string in the format: moda_pat_{base64-encoded-random-bytes}</returns>
    string GenerateToken();

    /// <summary>
    /// Hashes a token for secure storage.
    /// </summary>
    /// <param name="token">The plain text token to hash.</param>
    /// <returns>The hashed token.</returns>
    string HashToken(string token);

    /// <summary>
    /// Verifies that a plain text token matches a stored hash.
    /// </summary>
    /// <param name="token">The plain text token to verify.</param>
    /// <param name="hash">The stored hash to verify against.</param>
    /// <returns>True if the token matches the hash, false otherwise.</returns>
    bool VerifyToken(string token, string hash);
}
