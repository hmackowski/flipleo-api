using System.ComponentModel.DataAnnotations;

namespace FlipLeo.Core.DTOs.Auth;

public class ResetPasswordRequest
{
    /// <summary>The token from the emailed link (?token=...).</summary>
    [Required, MaxLength(200)]
    public string Token { get; set; } = string.Empty;

    [Required, MinLength(8, ErrorMessage = "Password must be at least 8 characters."), MaxLength(128)]
    public string NewPassword { get; set; } = string.Empty;
}
