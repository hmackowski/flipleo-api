using System.Text;
using System.Threading.RateLimiting;
using FlipLeo.Api.Utilities;
using FlipLeo.Core.Interfaces;
using FlipLeo.Repository;
using FlipLeo.Services.Utilities.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ---------- JWT settings ----------
var jwtSection = builder.Configuration.GetSection(JwtSettings.SectionName);
var jwtSettings = jwtSection.Get<JwtSettings>() ?? new JwtSettings();

// Fail fast with a helpful message instead of issuing tokens nobody can verify
if (Encoding.UTF8.GetByteCount(jwtSettings.SigningKey) < 32)
{
    throw new InvalidOperationException(
        "Jwt:SigningKey is missing or shorter than 32 characters. For local development run (in the FlipLeo.Api folder): " +
        "dotnet user-secrets set \"Jwt:SigningKey\" \"<a long random string>\"");
}

// ---------- Services (DI registrations) ----------
builder.Services.AddControllers();

// OpenAPI document + a "Bearer" scheme so Swagger UI shows an Authorize button
builder.Services.AddOpenApi(options => options.AddDocumentTransformer((document, _, _) =>
{
    document.Components ??= new OpenApiComponents();
    document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the token from POST /api/auth/login"
    };
    document.SecurityRequirements.Add(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
        }] = Array.Empty<string>()
    });
    return Task.CompletedTask;
}));

builder.Services.AddDbContext<FlipLeoContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("FlipLeo"),
        // Azure SQL can briefly drop connections (failover, or waking from auto-pause): retry instead of failing
        sql => sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null)));

builder.Services.AddScopedServices();

// Who is calling: read from the JWT on each request
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Issue tokens (login/register) ...
builder.Services.Configure<JwtSettings>(jwtSection);
builder.Services.AddSingleton<ITokenService, JwtTokenService>();

// ... and validate them on every [Authorize] request
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Keep claim names as they are in the token ("sub", "email", "name")
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = JwtClaimNames.Name
        };
    });
builder.Services.AddAuthorization();

// Account emails (password reset). SMTP settings come from the "Email" section.
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(EmailSettings.SectionName));
builder.Services.AddScoped<IAccountEmailService, AccountEmailService>();

// ---------- Rate limiting (slows down password guessing / spam on the auth endpoints) ----------
// Limits are per IP address. Behind a proxy (e.g. Cloudflare Tunnel) you'd also need forwarded headers
// so RemoteIpAddress is the real visitor, not the proxy.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Login / register / reset: 10 tries a minute
    options.AddPolicy(RateLimitPolicies.Auth, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));

    // Forgot password sends emails, so it's stricter: 5 every 15 minutes
    options.AddPolicy(RateLimitPolicies.PasswordResetEmail, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(15), QueueLimit = 0 }));

    // Same ProblemDetails shape as our other errors, so the UI shows the message
    options.OnRejected = async (context, cancellationToken) =>
    {
        await context.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Title = "Too Many Requests",
            Detail = "Too many attempts. Please wait a few minutes and try again."
        }, cancellationToken);
    };
});

// Service exceptions (NotFound/BadRequest/Unauthorized/Conflict) -> ProblemDetails responses
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ---------- CORS: which websites may call the API ----------
// Development: the Angular dev server. Production: https://flipleo.com (appsettings.Production.json "Cors:AllowedOrigins").
const string CorsPolicyName = "FlipLeoUi";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
if (allowedOrigins.Length == 0)
    allowedOrigins = ["http://localhost:4200"];

builder.Services.AddCors(options => options.AddPolicy(CorsPolicyName, policy =>
    policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

// ---------- Hosting behind a proxy (Azure App Service) ----------
// App Service's front end terminates HTTPS and forwards the request. These headers carry the visitor's real
// IP (needed for per-IP rate limiting) and the original https scheme.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // App Service's proxy address isn't fixed, so trust the forwarding hop it adds (ForwardLimit = 1)
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// ---------- Health check: GET /health (used by App Service health check + uptime monitors) ----------
builder.Services.AddHealthChecks();

var app = builder.Build();

// ---------- HTTP pipeline ----------
app.UseForwardedHeaders();  // first, so everything after sees the real client IP / https
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();   // serves /openapi/v1.json
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/openapi/v1.json", "FlipLeo API"));
}

app.UseHttpsRedirection();
app.UseCors(CorsPolicyName);
app.UseRateLimiter();      // after CORS so 429 responses still reach the browser
app.UseAuthentication();   // who are you? (reads the JWT)
app.UseAuthorization();    // are you allowed? ([Authorize])
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
