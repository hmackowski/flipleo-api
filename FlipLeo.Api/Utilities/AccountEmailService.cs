using System.Net;
using System.Net.Mail;
using FlipLeo.Core.Interfaces;
using Microsoft.Extensions.Options;

namespace FlipLeo.Api.Utilities;

/// <summary>
/// Builds and sends account emails over SMTP (works with smtp4dev, Gmail, Resend, etc.).
/// If Email:Host isn't set, Development logs the email instead; other environments log an error.
/// </summary>
public class AccountEmailService(
    IOptions<EmailSettings> options,
    IHostEnvironment environment,
    ILogger<AccountEmailService> logger) : IAccountEmailService
{
    private readonly EmailSettings _settings = options.Value;

    public async Task SendPasswordResetAsync(string toEmail, string displayName, string token)
    {
        var link = $"{_settings.UiBaseUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(token)}";
        var name = WebUtility.HtmlEncode(displayName);

        var body = $"""
            <p>Hi {name},</p>
            <p>Someone asked to reset the password for your FlipLeo account.</p>
            <p><a href="{link}">Reset your password</a></p>
            <p>This link works once and expires in 1 hour. If you didn't ask for this, you can ignore this email;
               your password won't change.</p>
            <p>— FlipLeo</p>
            """;

        await SendAsync(toEmail, "Reset your FlipLeo password", body, link);
    }

    private async Task SendAsync(string toEmail, string subject, string htmlBody, string linkForDevLog)
    {
        if (string.IsNullOrWhiteSpace(_settings.Host))
        {
            if (environment.IsDevelopment())
            {
                // Dev only: never log links like this in production (they let anyone reset the password)
                logger.LogWarning("Email:Host not set, so no email was sent. To {To}: {Subject} -> {Link}",
                    toEmail, subject, linkForDevLog);
            }
            else
            {
                logger.LogError("Email:Host is not configured; could not send '{Subject}'", subject);
            }
            return;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_settings.FromAddress, _settings.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(toEmail);

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            Credentials = string.IsNullOrEmpty(_settings.Username)
                ? null
                : new NetworkCredential(_settings.Username, _settings.Password)
        };

        try
        {
            await client.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            // Don't fail the request (or reveal anything) because email is down; just log it
            logger.LogError(ex, "Failed to send '{Subject}' email", subject);
        }
    }
}
