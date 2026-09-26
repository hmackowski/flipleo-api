using System.Text.Json;
using FlipLeo.Core.DTOs;
using FlipLeo.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FlipLeo.Api.Controllers;

[Route("api/flip-records")]
[ApiController]
public class FlipRecordController : ControllerBase
{
    private readonly IFlipRecordService _flipRecordService;
    private readonly ILogger<FlipRecordController> _logger;

    public FlipRecordController(
        IFlipRecordService flipRecordService,
        ILogger<FlipRecordController> logger)
    {
        _flipRecordService = flipRecordService ?? throw new ArgumentNullException(nameof(flipRecordService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Flip Records

    [HttpGet]
    public async Task<IActionResult> GetFlipRecords()
    {
        var result = await _flipRecordService.GetFlipRecords();

        _logger.LogDebug("Get Flip Records {Results}", JsonSerializer.Serialize(result));

        return Ok(result);
    }

    [HttpGet("{flipRecordId:int}")]
    public async Task<IActionResult> GetFlipRecord(int flipRecordId)
    {
        var result = await _flipRecordService.GetFlipRecord(flipRecordId);

        _logger.LogDebug("Get Flip Record {FlipRecordId} {Results}", flipRecordId, JsonSerializer.Serialize(result));

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> AddFlipRecord([FromBody] FlipRecord flipRecord)
    {
        var result = await _flipRecordService.AddFlipRecord(flipRecord);

        _logger.LogDebug("Add Flip Record {Results}", JsonSerializer.Serialize(result));

        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateFlipRecord([FromBody] FlipRecord flipRecord)
    {
        var result = await _flipRecordService.UpdateFlipRecord(flipRecord);

        _logger.LogDebug("Update Flip Record {FlipRecordId} {Results}", flipRecord.Id, JsonSerializer.Serialize(result));

        return Ok(result);
    }

    [HttpDelete("{flipRecordId:int}")]
    public async Task<IActionResult> DeleteFlipRecord(int flipRecordId)
    {
        var result = await _flipRecordService.DeleteFlipRecord(flipRecordId);

        _logger.LogDebug("Delete Flip Record {FlipRecordId} {Results}", flipRecordId, JsonSerializer.Serialize(result));

        return Ok(result);
    }

    #endregion

    #region Add-Ons

    [HttpPost("{flipRecordId:int}/add-ons")]
    public async Task<IActionResult> AddAddOn(int flipRecordId, [FromBody] FlipRecordAddOn addOn)
    {
        var result = await _flipRecordService.AddAddOn(flipRecordId, addOn);

        _logger.LogDebug("Add Add-On {FlipRecordId} {Results}", flipRecordId, JsonSerializer.Serialize(result));

        return Ok(result);
    }

    [HttpPut("add-ons")]
    public async Task<IActionResult> UpdateAddOn([FromBody] FlipRecordAddOn addOn)
    {
        var result = await _flipRecordService.UpdateAddOn(addOn);

        _logger.LogDebug("Update Add-On {AddOnId} {Results}", addOn.Id, JsonSerializer.Serialize(result));

        return Ok(result);
    }

    [HttpDelete("add-ons/{addOnId:int}")]
    public async Task<IActionResult> DeleteAddOn(int addOnId)
    {
        var result = await _flipRecordService.DeleteAddOn(addOnId);

        _logger.LogDebug("Delete Add-On {AddOnId} {Results}", addOnId, JsonSerializer.Serialize(result));

        return Ok(result);
    }

    #endregion
}
