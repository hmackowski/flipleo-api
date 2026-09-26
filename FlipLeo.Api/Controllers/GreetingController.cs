using System.Text.Json;
using FlipLeo.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FlipLeo.Api.Controllers;

[Route("api/greetings")]
[ApiController]
public class GreetingController : ControllerBase
{
    private readonly IGreetingService _greetingService;
    private readonly ILogger<GreetingController> _logger;

    public GreetingController(
        IGreetingService greetingService,
        ILogger<GreetingController> logger)
    {
        _greetingService = greetingService ?? throw new ArgumentNullException(nameof(greetingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public IActionResult GetGreeting()
    {
        var result = _greetingService.GetGreeting();

        _logger.LogDebug("Get Greeting {Result}", JsonSerializer.Serialize(result));

        return Ok(new { message = result });
    }
}