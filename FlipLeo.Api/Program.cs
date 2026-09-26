using System.Text;
using System.Threading.RateLimiting;
using FlipLeo.Api.Utilities;
using FlipLeo.Core.Interfaces;
using FlipLeo.Repository;
using FlipLeo.Services.Utilities.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
    options.UseSqlServer(builder.Configuration.GetConnectionString("FlipLeo")));

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

// Allow the Angular dev server to call the API
builder.Services.AddCors(options => options.AddPolicy("AngularDev", policy =>
    policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// ---------- HTTP pipeline ----------
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();   // serves /openapi/v1.json
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/openapi/v1.json", "FlipLeo API"));
}

app.UseHttpsRedirection();
app.UseCors("AngularDev");
app.UseRateLimiter();      // after CORS so 429 responses still reach the browser
app.UseAuthentication();   // who are you? (reads the JWT)
app.UseAuthorization();    // are you allowed? ([Authorize])
app.MapControllers();

app.Run();
