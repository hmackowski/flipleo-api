namespace FlipLeo.Api.Utilities;

/// <summary>
/// Bound from the "Email" section. Leave Host empty to not send at all: in Development the
/// email (with its link) is written to the console log instead, so you can test without SMTP.
/// Password is a secret: keep it in user-secrets locally (never commit it).
/// Examples:
///   smtp4dev / Mailpit (local fake inbox): Host "localhost", Port 25 (smtp4dev) or 1025 (Mailpit), EnableSsl false
///   Gmail (app password):                  Host "smtp.gmail.com", Port 587, EnableSsl true
///   Resend:                                Host "smtp.resend.com", Port 587, EnableSsl true, Username "resend", Password = API key
/// </summary>
public class EmailSettings
{
    public const string SectionName = "Email";

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public bool EnableSsl { get; set; } = true;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FromAddress { get; set; } = "no-reply@flipleo.com";

    public string FromName { get; set; } = "FlipLeo";

    /// <summary>Where the UI runs, for links in emails (e.g. http://localhost:4200 or https://flipleo.com).</summary>
    public string UiBaseUrl { get; set; } = "http://localhost:4200";
}
