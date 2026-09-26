namespace FlipLeo.Core.DTOs.Auth;

/// <summary>The logged-in user's public info. Never includes the password hash.</summary>
public class UserProfile
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;
}
