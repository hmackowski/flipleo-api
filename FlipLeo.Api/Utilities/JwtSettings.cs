namespace FlipLeo.Api.Utilities;

/// <summary>
/// Bound from the "Jwt" section of configuration.
/// Issuer/Audience/ExpiresInMinutes live in appsettings.json.
/// SigningKey is a secret: it comes from user-secrets locally (never commit it).
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string SigningKey { get; set; } = string.Empty;

    public int ExpiresInMinutes { get; set; } = 480;
}

/// <summary>Claim names used in FlipLeo tokens (standard JWT names).</summary>
public static class JwtClaimNames
{
    public const string Subject = "sub";
    public const string Email = "email";
    public const string Name = "name";
}
