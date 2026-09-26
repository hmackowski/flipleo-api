using System.Text.Json;
using FlipLeo.Core.DTOs;
using FlipLeo.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlipLeo.Api.Controllers;

[Route("api/add-on-presets")]
[ApiController]
[Authorize]
public class AddOnPresetController : ControllerBase
{
    private readonly IAddOnPresetService _addOnPresetService;
    private readonly ILogger<AddOnPresetController> _logger;

    public AddOnPresetController(
        IAddOnPresetService addOnPresetService,
        ILogger<AddOnPresetController> logger)
    {
        _addOnPresetService = addOnPresetService ?? throw new ArgumentNullException(nameof(addOnPresetService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public async Task<IActionResult> GetAddOnPresets()
    {
        var result = await _addOnPresetService.GetAddOnPresets();

        _logger.LogDebug("Get Add-On Presets {Results}", JsonSerializer.Serialize(result));

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> AddAddOnPreset([FromBody] AddOnPreset addOnPreset)
    {
        var result = await _addOnPresetService.AddAddOnPreset(addOnPreset);

        _logger.LogDebug("Add Add-On Preset {Results}", JsonSerializer.Serialize(result));

        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateAddOnPreset([FromBody] AddOnPreset addOnPreset)
    {
        var result = await _addOnPresetService.UpdateAddOnPreset(addOnPreset);

        _logger.LogDebug("Update Add-On Preset {AddOnPresetId} {Results}", addOnPreset.Id, JsonSerializer.Serialize(result));

        return Ok(result);
    }

    [HttpDelete("{addOnPresetId:int}")]
    public async Task<IActionResult> DeleteAddOnPreset(int addOnPresetId)
    {
        var result = await _addOnPresetService.DeleteAddOnPreset(addOnPresetId);

        _logger.LogDebug("Delete Add-On Preset {AddOnPresetId} {Results}", addOnPresetId, JsonSerializer.Serialize(result));

        return Ok(result);
    }
}
