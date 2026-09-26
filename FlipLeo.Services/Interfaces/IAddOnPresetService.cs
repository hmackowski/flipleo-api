using FlipLeo.Core.DTOs;

namespace FlipLeo.Services.Interfaces;

public interface IAddOnPresetService
{
    Task<AddOnPreset[]> GetAddOnPresets();

    Task<AddOnPreset> AddAddOnPreset(AddOnPreset addOnPreset);

    Task<AddOnPreset> UpdateAddOnPreset(AddOnPreset addOnPreset);

    Task<SuccessResult> DeleteAddOnPreset(int addOnPresetId);
}
