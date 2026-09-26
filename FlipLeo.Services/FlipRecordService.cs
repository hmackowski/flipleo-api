using FlipLeo.Core.DTOs;
using FlipLeo.Core.Exceptions;
using FlipLeo.Core.Interfaces;
using FlipLeo.Repository.Interfaces;
using FlipLeo.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using AddOnDto = FlipLeo.Core.DTOs.FlipRecordAddOn;
using AddOnEntity = FlipLeo.Repository.Entities.FlipRecordAddOn;
using FlipRecordDto = FlipLeo.Core.DTOs.FlipRecord;
using FlipRecordEntity = FlipLeo.Repository.Entities.FlipRecord;

namespace FlipLeo.Services;

public class FlipRecordService : IFlipRecordService
{
    private readonly IFlipLeoUnitOfWork _flipLeoUnitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public FlipRecordService(IFlipLeoUnitOfWork flipLeoUnitOfWork, ICurrentUserService currentUserService)
    {
        _flipLeoUnitOfWork = flipLeoUnitOfWork ?? throw new ArgumentNullException(nameof(flipLeoUnitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    // Every query is scoped to the logged-in user. Add-ons are owned through their FlipRecord.
    private Guid UserId => _currentUserService.GetRequiredUserId();

    public async Task<FlipRecordDto[]> GetFlipRecords()
    {
        return await _flipLeoUnitOfWork.FlipRecordRepository
            .Find(f => f.UserId == UserId)
            .OrderByDescending(f => f.FlipDate)
            .ThenByDescending(f => f.Id)
            .Select(f => new FlipRecordDto
            {
                Id = f.Id,
                ItemName = f.ItemName,
                BuyPrice = f.BuyPrice,
                SellPrice = f.SellPrice,
                FlipDate = f.FlipDate,
                AuctionId = f.AuctionId,
                // PartsPrice and Profit are calculated (in SQL) instead of being stored in the table
                PartsPrice = f.AddOns.Sum(a => a.Price),
                Profit = f.SellPrice - f.BuyPrice - f.AddOns.Sum(a => a.Price),
                AddOns = f.AddOns
                    .OrderBy(a => a.Id)
                    .Select(a => new AddOnDto
                    {
                        Id = a.Id,
                        FlipRecordId = a.FlipRecordId,
                        AddOnPresetId = a.AddOnPresetId,
                        Name = a.Name,
                        Price = a.Price,
                        Link = a.Link,
                        ImageUrl = a.ImageUrl
                    })
                    .ToList()
            })
            .ToArrayAsync();
    }

    public async Task<FlipRecordDto> GetFlipRecord(int flipRecordId)
    {
        var flipRecord = await _flipLeoUnitOfWork.FlipRecordRepository
            .Find(f => f.Id == flipRecordId && f.UserId == UserId)
            .Select(f => new FlipRecordDto
            {
                Id = f.Id,
                ItemName = f.ItemName,
                BuyPrice = f.BuyPrice,
                SellPrice = f.SellPrice,
                FlipDate = f.FlipDate,
                AuctionId = f.AuctionId,
                // PartsPrice and Profit are calculated (in SQL) instead of being stored in the table
                PartsPrice = f.AddOns.Sum(a => a.Price),
                Profit = f.SellPrice - f.BuyPrice - f.AddOns.Sum(a => a.Price),
                AddOns = f.AddOns
                    .OrderBy(a => a.Id)
                    .Select(a => new AddOnDto
                    {
                        Id = a.Id,
                        FlipRecordId = a.FlipRecordId,
                        AddOnPresetId = a.AddOnPresetId,
                        Name = a.Name,
                        Price = a.Price,
                        Link = a.Link,
                        ImageUrl = a.ImageUrl
                    })
                    .ToList()
            })
            .SingleOrDefaultAsync();

        return flipRecord ?? throw new NotFoundException($"Flip record with ID {flipRecordId} not found");
    }

    public async Task<FlipRecordDto> AddFlipRecord(FlipRecordDto flipRecord)
    {
        await ValidateAuction(flipRecord.AuctionId);
        await ValidateAddOnPresets(flipRecord.AddOns.Select(a => a.AddOnPresetId).ToArray());

        var flipRecordEntity = new FlipRecordEntity
        {
            UserId = UserId,
            ItemName = flipRecord.ItemName,
            BuyPrice = flipRecord.BuyPrice,
            SellPrice = flipRecord.SellPrice,
            FlipDate = flipRecord.FlipDate.Date,
            AuctionId = flipRecord.AuctionId,
            AddOns = flipRecord.AddOns
                .Select(a => new AddOnEntity
                {
                    AddOnPresetId = a.AddOnPresetId,
                    Name = a.Name,
                    Price = a.Price,
                    Link = a.Link,
                    ImageUrl = a.ImageUrl
                })
                .ToList()
        };

        // Adding the parent also adds its AddOns; everything saves in one transaction
        _flipLeoUnitOfWork.FlipRecordRepository.Add(flipRecordEntity);
        await _flipLeoUnitOfWork.CommitAsync();

        return await GetFlipRecord(flipRecordEntity.Id);
    }

    public async Task<FlipRecordDto> UpdateFlipRecord(FlipRecordDto flipRecord)
    {
        var flipRecordToUpdate = await _flipLeoUnitOfWork.FlipRecordRepository
            .SingleOrDefaultAsync(f => f.Id == flipRecord.Id && f.UserId == UserId)
            ?? throw new NotFoundException($"Flip record with ID {flipRecord.Id} not found");

        await ValidateAuction(flipRecord.AuctionId);

        flipRecordToUpdate.ItemName = flipRecord.ItemName;
        flipRecordToUpdate.BuyPrice = flipRecord.BuyPrice;
        flipRecordToUpdate.SellPrice = flipRecord.SellPrice;
        flipRecordToUpdate.FlipDate = flipRecord.FlipDate.Date;
        flipRecordToUpdate.AuctionId = flipRecord.AuctionId;

        await _flipLeoUnitOfWork.CommitAsync();

        return await GetFlipRecord(flipRecordToUpdate.Id);
    }

    public async Task<SuccessResult> DeleteFlipRecord(int flipRecordId)
    {
        var flipRecordToDelete = await _flipLeoUnitOfWork.FlipRecordRepository
            .Find(f => f.Id == flipRecordId && f.UserId == UserId)
            .Include(f => f.AddOns)
            .SingleOrDefaultAsync()
            ?? throw new NotFoundException($"Flip record with ID {flipRecordId} not found");

        // Soft delete the add-ons first, then the flip itself
        foreach (var addOn in flipRecordToDelete.AddOns)
            _flipLeoUnitOfWork.FlipRecordAddOnRepository.Delete(addOn);

        _flipLeoUnitOfWork.FlipRecordRepository.Delete(flipRecordToDelete);
        await _flipLeoUnitOfWork.CommitAsync();

        return new SuccessResult { Success = true, Detail = "Flip record deleted!" };
    }

    public async Task<FlipRecordDto> AddAddOn(int flipRecordId, AddOnDto addOn)
    {
        var flipRecordExists = await _flipLeoUnitOfWork.FlipRecordRepository
            .AnyAsync(f => f.Id == flipRecordId && f.UserId == UserId);

        if (!flipRecordExists)
            throw new NotFoundException($"Flip record with ID {flipRecordId} not found");

        await ValidateAddOnPresets(addOn.AddOnPresetId);

        _flipLeoUnitOfWork.FlipRecordAddOnRepository.Add(new AddOnEntity
        {
            FlipRecordId = flipRecordId,
            AddOnPresetId = addOn.AddOnPresetId,
            Name = addOn.Name,
            Price = addOn.Price,
            Link = addOn.Link,
            ImageUrl = addOn.ImageUrl
        });

        await _flipLeoUnitOfWork.CommitAsync();

        return await GetFlipRecord(flipRecordId);
    }

    public async Task<FlipRecordDto> UpdateAddOn(AddOnDto addOn)
    {
        var addOnToUpdate = await _flipLeoUnitOfWork.FlipRecordAddOnRepository
            .SingleOrDefaultAsync(a => a.Id == addOn.Id && a.FlipRecord.UserId == UserId)
            ?? throw new NotFoundException($"Add-on with ID {addOn.Id} not found");

        addOnToUpdate.Name = addOn.Name;
        addOnToUpdate.Price = addOn.Price;
        addOnToUpdate.Link = addOn.Link;
        addOnToUpdate.ImageUrl = addOn.ImageUrl;

        await _flipLeoUnitOfWork.CommitAsync();

        return await GetFlipRecord(addOnToUpdate.FlipRecordId);
    }

    public async Task<FlipRecordDto> DeleteAddOn(int addOnId)
    {
        var addOnToDelete = await _flipLeoUnitOfWork.FlipRecordAddOnRepository
            .SingleOrDefaultAsync(a => a.Id == addOnId && a.FlipRecord.UserId == UserId)
            ?? throw new NotFoundException($"Add-on with ID {addOnId} not found");

        _flipLeoUnitOfWork.FlipRecordAddOnRepository.Delete(addOnToDelete);
        await _flipLeoUnitOfWork.CommitAsync();

        return await GetFlipRecord(addOnToDelete.FlipRecordId);
    }

    /// <summary>Presets are per user: make sure any preset ids sent belong to the caller.</summary>
    private async Task ValidateAddOnPresets(params int?[] addOnPresetIds)
    {
        var ids = addOnPresetIds.Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();
        if (ids.Count == 0)
            return;

        var ownedCount = await _flipLeoUnitOfWork.AddOnPresetRepository
            .Find(p => ids.Contains(p.Id) && p.UserId == UserId)
            .CountAsync();

        if (ownedCount != ids.Count)
            throw new BadRequestException("One or more add-on presets do not exist.");
    }

    private async Task ValidateAuction(int? auctionId)
    {
        if (auctionId is null)
            return;

        var auctionExists = await _flipLeoUnitOfWork.AuctionRepository
            .AnyAsync(a => a.Id == auctionId && a.UserId == UserId);

        if (!auctionExists)
            throw new BadRequestException($"Auction with ID {auctionId} does not exist.");
    }
}
