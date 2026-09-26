namespace FlipLeo.Core.DTOs.Auth;

/// <summary>Returned by register and login. The UI sends Token as "Authorization: Bearer {token}".</summary>
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public UserProfile User { get; set; } = new();
}
