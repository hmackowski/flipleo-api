namespace FlipLeo.Api.Utilities;

/// <summary>Rate limiter policy names (configured in Program.cs, used with [EnableRateLimiting]).</summary>
public static class RateLimitPolicies
{
    public const string Auth = "auth";
    public const string PasswordResetEmail = "password-reset-email";
}
