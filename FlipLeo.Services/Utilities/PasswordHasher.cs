using System.Security.Cryptography;
using FlipLeo.Services.Interfaces;

namespace FlipLeo.Services.Utilities;

/// <summary>
/// PBKDF2 (HMAC-SHA256) password hashing using only built-in .NET crypto.
/// Stored format: "PBKDF2-SHA256.{iterations}.{base64 salt}.{base64 hash}"
/// Every password gets its own random salt, so two users with the same password get different hashes.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const string Algorithm = "PBKDF2-SHA256";
    private const int Iterations = 600_000; // OWASP recommendation for PBKDF2-HMAC-SHA256
    private const int SaltSize = 16;
    private const int HashSize = 32;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

        return $"{Algorithm}.{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string passwordHash)
    {
        var parts = passwordHash.Split('.');
        if (parts.Length != 4 || parts[0] != Algorithm || !int.TryParse(parts[1], out var iterations))
            return false;

        var salt = Convert.FromBase64String(parts[2]);
        var expectedHash = Convert.FromBase64String(parts[3]);
        var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);

        // Constant-time comparison so response timing doesn't leak how many bytes matched
        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
