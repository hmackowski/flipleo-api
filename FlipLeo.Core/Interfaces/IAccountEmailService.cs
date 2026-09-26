namespace FlipLeo.Core.Interfaces;

/// <summary>
/// Sends account emails (password reset now; email confirmation later).
/// Implemented in the Api project, because it needs the email settings and the UI's address.
/// </summary>
public interface IAccountEmailService
{
    Task SendPasswordResetAsync(string toEmail, string displayName, string token);
}
