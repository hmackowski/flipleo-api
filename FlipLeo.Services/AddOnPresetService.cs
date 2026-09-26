using FlipLeo.Core.DTOs;
using FlipLeo.Core.Exceptions;
using FlipLeo.Core.Interfaces;
using FlipLeo.Repository.Interfaces;
using FlipLeo.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using AddOnPresetDto = FlipLeo.Core.DTOs.AddOnPreset;
using AddOnPresetEntity = FlipLeo.Repository.Entities.AddOnPreset;

namespace FlipLeo.Services;

public class AddOnPresetService : IAddOnPresetService
{
    private readonly IFlipLeoUnitOfWork _flipLeoUnitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AddOnPresetService(IFlipLeoUnitOfWork flipLeoUnitOfWork, ICurrentUserService currentUserService)
    {
        _flipLeoUnitOfWork = flipLeoUnitOfWork ?? throw new ArgumentNullException(nameof(flipLeoUnitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    // Presets are per user: every query is scoped to the logged-in user
    private Guid UserId => _currentUserService.GetRequiredUserId();

    public async Task<AddOnPresetDto[]> GetAddOnPresets()
    {
        return await _flipLeoUnitOfWork.AddOnPresetRepository
            .Find(p => p.UserId == UserId)
            .OrderBy(p => p.Name)
            .Select(p => new AddOnPresetDto
            {
                Id = p.Id,
                Name = p.Name,
                DefaultPrice = p.DefaultPrice,
                Link = p.Link,
                ImageUrl = p.ImageUrl
            })
            .ToArrayAsync();
    }

    public async Task<AddOnPresetDto> AddAddOnPreset(AddOnPresetDto addOnPreset)
    {
        var addOnPresetEntity = new AddOnPresetEntity
        {
            UserId = UserId,
            Name = addOnPreset.Name.Trim(),
            DefaultPrice = addOnPreset.DefaultPrice,
            Link = addOnPreset.Link,
            ImageUrl = addOnPreset.ImageUrl
        };

        _flipLeoUnitOfWork.AddOnPresetRepository.Add(addOnPresetEntity);
        await _flipLeoUnitOfWork.CommitAsync();

        return new AddOnPresetDto
        {
            Id = addOnPresetEntity.Id,
            Name = addOnPresetEntity.Name,
            DefaultPrice = addOnPresetEntity.DefaultPrice,
            Link = addOnPresetEntity.Link,
            ImageUrl = addOnPresetEntity.ImageUrl
        };
    }

    public async Task<AddOnPresetDto> UpdateAddOnPreset(AddOnPresetDto addOnPreset)
    {
        var addOnPresetToUpdate = await _flipLeoUnitOfWork.AddOnPresetRepository
            .SingleOrDefaultAsync(p => p.Id == addOnPreset.Id && p.UserId == UserId)
            ?? throw new NotFoundException($"Add-on preset with ID {addOnPreset.Id} not found");

        // Only the preset changes; add-ons already on flips keep the price they were logged with
        addOnPresetToUpdate.Name = addOnPreset.Name.Trim();
        addOnPresetToUpdate.DefaultPrice = addOnPreset.DefaultPrice;
        addOnPresetToUpdate.Link = addOnPreset.Link;
        addOnPresetToUpdate.ImageUrl = addOnPreset.ImageUrl;

        await _flipLeoUnitOfWork.CommitAsync();

        return new AddOnPresetDto
        {
            Id = addOnPresetToUpdate.Id,
            Name = addOnPresetToUpdate.Name,
            DefaultPrice = addOnPresetToUpdate.DefaultPrice,
            Link = addOnPresetToUpdate.Link,
            ImageUrl = addOnPresetToUpdate.ImageUrl
        };
    }

    public async Task<SuccessResult> DeleteAddOnPreset(int addOnPresetId)
    {
        var addOnPresetToDelete = await _flipLeoUnitOfWork.AddOnPresetRepository
            .SingleOrDefaultAsync(p => p.Id == addOnPresetId && p.UserId == UserId)
            ?? throw new NotFoundException($"Add-on preset with ID {addOnPresetId} not found");

        // Soft delete: the preset disappears from the quick buttons, past flips are unaffected
        _flipLeoUnitOfWork.AddOnPresetRepository.Delete(addOnPresetToDelete);
        await _flipLeoUnitOfWork.CommitAsync();

        return new SuccessResult { Success = true, Detail = "Add-on preset deleted!" };
    }
}
