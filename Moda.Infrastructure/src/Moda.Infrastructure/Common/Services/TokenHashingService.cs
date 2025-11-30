using System.Security.Cryptography;
using System.Text;

namespace Moda.Infrastructure.Common.Services;

/// <summary>
/// Implementation of token hashing service using SHA256 with salt.
/// </summary>
public class TokenHashingService : ITokenHashingService
{
    private const string TokenPrefix = "moda_pat_";
    private const int TokenBytesLength = 32; // 32 bytes = 256 bits of randomness
    private const int SaltBytesLength = 16; // 16 bytes = 128 bits salt

    /// <summary>
    /// Generates a cryptographically secure random token.
    /// </summary>
    /// <returns>A token string in the format: moda_pat_{base64-encoded-random-bytes}</returns>
    public string GenerateToken()
    {
        byte[] tokenBytes = RandomNumberGenerator.GetBytes(TokenBytesLength);
        string tokenValue = Convert.ToBase64String(tokenBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('='); // URL-safe base64

        return $"{TokenPrefix}{tokenValue}";
    }

    /// <summary>
    /// Hashes a token for secure storage using SHA256 with a random salt.
    /// Format: {salt}:{hash}
    /// </summary>
    /// <param name="token">The plain text token to hash.</param>
    /// <returns>The hashed token with salt.</returns>
    public string HashToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Token cannot be null or empty.", nameof(token));
        }

        // Generate random salt
        byte[] salt = RandomNumberGenerator.GetBytes(SaltBytesLength);

        // Hash the token with the salt
        byte[] hash = HashWithSalt(token, salt);

        // Return salt and hash as base64 strings separated by ':'
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Verifies that a plain text token matches a stored hash.
    /// </summary>
    /// <param name="token">The plain text token to verify.</param>
    /// <param name="hash">The stored hash to verify against (format: {salt}:{hash}).</param>
    /// <returns>True if the token matches the hash, false otherwise.</returns>
    public bool VerifyToken(string token, string hash)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(hash))
        {
            return false;
        }

        try
        {
            // Split the stored hash into salt and hash parts
            string[] parts = hash.Split(':');
            if (parts.Length != 2)
            {
                return false;
            }

            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] storedHash = Convert.FromBase64String(parts[1]);

            // Hash the provided token with the stored salt
            byte[] computedHash = HashWithSalt(token, salt);

            // Compare the hashes using a constant-time comparison
            return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Hashes a token with a given salt using SHA256.
    /// </summary>
    private static byte[] HashWithSalt(string token, byte[] salt)
    {
        byte[] tokenBytes = Encoding.UTF8.GetBytes(token);

        // Combine salt and token bytes
        byte[] saltedToken = new byte[salt.Length + tokenBytes.Length];
        Buffer.BlockCopy(salt, 0, saltedToken, 0, salt.Length);
        Buffer.BlockCopy(tokenBytes, 0, saltedToken, salt.Length, tokenBytes.Length);

        // Hash with SHA256
        return SHA256.HashData(saltedToken);
    }
}
