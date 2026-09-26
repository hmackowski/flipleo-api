using System.Text.Json;
using FlipLeo.Core.DTOs;
using FlipLeo.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FlipLeo.Api.Controllers;

[Route("api/auctions")]
[ApiController]
public class AuctionController : ControllerBase
{
    private readonly IAuctionService _auctionService;
    private readonly ILogger<AuctionController> _logger;

    public AuctionController(
        IAuctionService auctionService,
        ILogger<AuctionController> logger)
    {
        _auctionService = auctionService ?? throw new ArgumentNullException(nameof(auctionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public async Task<IActionResult> GetAuctions()
    {
        var result = await _auctionService.GetAuctions();

        _logger.LogDebug("Get Auctions {Results}", JsonSerializer.Serialize(result));

        return Ok(result);
    }

    [HttpGet("{auctionId:int}")]
    public async Task<IActionResult> GetAuction(int auctionId)
    {
        var result = await _auctionService.GetAuction(auctionId);

        _logger.LogDebug("Get Auction {AuctionId} {Results}", auctionId, JsonSerializer.Serialize(result));

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> AddAuction([FromBody] Auction auction)
    {
        var result = await _auctionService.AddAuction(auction);

        _logger.LogDebug("Add Auction {Results}", JsonSerializer.Serialize(result));

        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateAuction([FromBody] Auction auction)
    {
        var result = await _auctionService.UpdateAuction(auction);

        _logger.LogDebug("Update Auction {AuctionId} {Results}", auction.Id, JsonSerializer.Serialize(result));

        return Ok(result);
    }

    [HttpDelete("{auctionId:int}")]
    public async Task<IActionResult> DeleteAuction(int auctionId)
    {
        var result = await _auctionService.DeleteAuction(auctionId);

        _logger.LogDebug("Delete Auction {AuctionId} {Results}", auctionId, JsonSerializer.Serialize(result));

        return Ok(result);
    }
}
