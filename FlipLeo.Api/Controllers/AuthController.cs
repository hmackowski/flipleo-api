using FlipLeo.Api.Utilities;
using FlipLeo.Core.DTOs;
using FlipLeo.Core.DTOs.Auth;
using FlipLeo.Core.Interfaces;
using FlipLeo.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FlipLeo.Api.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        ITokenService tokenService,
        ICurrentUserService currentUserService,
        ILogger<AuthController> logger)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Never log the request bodies here: they contain passwords.

    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = await _authService.Register(request);

        _logger.LogInformation("Registered user {UserId} {Email}", user.Id, user.Email);

        return Ok(_tokenService.CreateAuthResponse(user));
    }

    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _authService.Login(request);

        _logger.LogInformation("User logged in {UserId}", user.Id);

        return Ok(_tokenService.CreateAuthResponse(user));
    }

    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.PasswordResetEmail)]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await _authService.ForgotPassword(request);

        // Same answer whether or not the email has an account
        return Ok(new SuccessResult
        {
            Success = true,
            Detail = "If an account exists for that email, we've sent a link to reset your password."
        });
    }

    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        await _authService.ResetPassword(request);

        _logger.LogInformation("Password reset completed");

        return Ok(new SuccessResult { Success = true, Detail = "Your password has been reset. You can sign in now." });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _authService.GetUser(_currentUserService.GetRequiredUserId());

        return Ok(user);
    }
}
