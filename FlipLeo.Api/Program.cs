using System.Text;
using FlipLeo.Api.Utilities;
using FlipLeo.Core.Interfaces;
using FlipLeo.Repository;
using FlipLeo.Services.Utilities.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
app.UseAuthentication();   // who are you? (reads the JWT)
app.UseAuthorization();    // are you allowed? ([Authorize])
app.MapControllers();

app.Run();
