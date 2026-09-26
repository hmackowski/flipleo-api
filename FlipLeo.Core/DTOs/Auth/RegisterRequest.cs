using System.ComponentModel.DataAnnotations;

namespace FlipLeo.Core.DTOs.Auth;

public class RegisterRequest
{
    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    [Required, MinLength(8, ErrorMessage = "Password must be at least 8 characters."), MaxLength(128)]
    public string Password { get; set; } = string.Empty;
}
