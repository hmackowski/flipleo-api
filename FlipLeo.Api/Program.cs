using FlipLeo.Api.Utilities;
using FlipLeo.Repository;
using FlipLeo.Services.Utilities.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------- Services (DI registrations) ----------
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<FlipLeoContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FlipLeo")));

builder.Services.AddScopedServices();

// Service exceptions (NotFound/BadRequest) -> ProblemDetails responses
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
    app.MapOpenApi();   // already there: serves /openapi/v1.json
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/openapi/v1.json", "FlipLeo API"));
}

app.UseHttpsRedirection();
app.UseCors("AngularDev");
app.UseAuthorization();
app.MapControllers();

app.Run();