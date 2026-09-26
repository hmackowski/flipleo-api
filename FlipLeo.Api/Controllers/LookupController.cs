using System.Text.Json;
using FlipLeo.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FlipLeo.Api.Controllers;

[Route("api/lookups")]
[ApiController]
public class LookupController : ControllerBase
{
    private readonly ILookupService _lookupService;
    private readonly ILogger<LookupController> _logger;

    public LookupController(
        ILookupService lookupService,
        ILogger<LookupController> logger)
    {
        _lookupService = lookupService ?? throw new ArgumentNullException(nameof(lookupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("auction-sites")]
    public async Task<IActionResult> GetAuctionSites()
    {
        var result = await _lookupService.GetAuctionSites();

        _logger.LogDebug("Get Auction Sites {Results}", JsonSerializer.Serialize(result));

        return Ok(result);
    }
}
