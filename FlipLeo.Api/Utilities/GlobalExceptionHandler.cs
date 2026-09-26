using FlipLeo.Core.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FlipLeo.Api.Utilities;

/// <summary>
/// Turns exceptions thrown by services into ProblemDetails responses, so controllers
/// don't need try/catch blocks:
///   NotFoundException   -> 404
///   BadRequestException -> 400
///   UnauthorizedException -> 401
///   ConflictException   -> 409
///   anything else       -> 500 (logged)
/// </summary>
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            BadRequestException => (StatusCodes.Status400BadRequest, "Bad Request"),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (StatusCodes.Status500InternalServerError, "Server Error")
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception for {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            // Don't leak internal error details to the client for 500s
            Detail = status == StatusCodes.Status500InternalServerError ? "An unexpected error occurred." : exception.Message
        };

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }
}
