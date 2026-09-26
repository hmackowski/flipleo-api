using System.Linq.Expressions;
using FlipLeo.Core.DTOs;
using FlipLeo.Core.Exceptions;
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

    public FlipRecordService(IFlipLeoUnitOfWork flipLeoUnitOfWork)
    {
        _flipLeoUnitOfWork = flipLeoUnitOfWork ?? throw new ArgumentNullException(nameof(flipLeoUnitOfWork));
    }

    // PartsPrice and Profit are calculated here (in SQL) instead of being stored in the table
    private static readonly Expression<Func<FlipRecordEntity, FlipRecordDto>> ToDto = f => new FlipRecordDto
    {
        Id = f.Id,
        ItemName = f.ItemName,
        BuyPrice = f.BuyPrice,
        SellPrice = f.SellPrice,
        FlipDate = f.FlipDate,
        AuctionId = f.AuctionId,
        PartsPrice = f.AddOns.Sum(a => a.Price),
        Profit = f.SellPrice - f.BuyPrice - f.AddOns.Sum(a => a.Price),
        AddOns = f.AddOns
            .OrderBy(a => a.Id)
            .Select(a => new AddOnDto
            {
                Id = a.Id,
                FlipRecordId = a.FlipRecordId,
                Name = a.Name,
                Price = a.Price,
                Link = a.Link
            })
            .ToList()
    };

    public async Task<FlipRecordDto[]> GetFlipRecords()
    {
        return await _flipLeoUnitOfWork.FlipRecordRepository
            .GetAll()
            .OrderByDescending(f => f.FlipDate)
            .ThenByDescending(f => f.Id)
            .Select(ToDto)
            .ToArrayAsync();
    }

    public async Task<FlipRecordDto> GetFlipRecord(int flipRecordId)
    {
        var flipRecord = await _flipLeoUnitOfWork.FlipRecordRepository
            .Find(f => f.Id == flipRecordId)
            .Select(ToDto)
            .SingleOrDefaultAsync();

        return flipRecord ?? throw new NotFoundException($"Flip record with ID {flipRecordId} not found");
    }

    public async Task<FlipRecordDto> AddFlipRecord(FlipRecordDto flipRecord)
    {
        await ValidateAuction(flipRecord.AuctionId);

        var flipRecordEntity = new FlipRecordEntity
        {
            ItemName = flipRecord.ItemName,
            BuyPrice = flipRecord.BuyPrice,
            SellPrice = flipRecord.SellPrice,
            FlipDate = flipRecord.FlipDate.Date,
            AuctionId = flipRecord.AuctionId,
            AddOns = flipRecord.AddOns
                .Select(a => new AddOnEntity
                {
                    Name = a.Name,
                    Price = a.Price,
                    Link = a.Link
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
            .SingleOrDefaultAsync(f => f.Id == flipRecord.Id)
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
            .Find(f => f.Id == flipRecordId)
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
            .AnyAsync(f => f.Id == flipRecordId);

        if (!flipRecordExists)
            throw new NotFoundException($"Flip record with ID {flipRecordId} not found");

        _flipLeoUnitOfWork.FlipRecordAddOnRepository.Add(new AddOnEntity
        {
            FlipRecordId = flipRecordId,
            Name = addOn.Name,
            Price = addOn.Price,
            Link = addOn.Link
        });

        await _flipLeoUnitOfWork.CommitAsync();

        return await GetFlipRecord(flipRecordId);
    }

    public async Task<FlipRecordDto> UpdateAddOn(AddOnDto addOn)
    {
        var addOnToUpdate = await _flipLeoUnitOfWork.FlipRecordAddOnRepository
            .SingleOrDefaultAsync(a => a.Id == addOn.Id)
            ?? throw new NotFoundException($"Add-on with ID {addOn.Id} not found");

        addOnToUpdate.Name = addOn.Name;
        addOnToUpdate.Price = addOn.Price;
        addOnToUpdate.Link = addOn.Link;

        await _flipLeoUnitOfWork.CommitAsync();

        return await GetFlipRecord(addOnToUpdate.FlipRecordId);
    }

    public async Task<FlipRecordDto> DeleteAddOn(int addOnId)
    {
        var addOnToDelete = await _flipLeoUnitOfWork.FlipRecordAddOnRepository
            .SingleOrDefaultAsync(a => a.Id == addOnId)
            ?? throw new NotFoundException($"Add-on with ID {addOnId} not found");

        _flipLeoUnitOfWork.FlipRecordAddOnRepository.Delete(addOnToDelete);
        await _flipLeoUnitOfWork.CommitAsync();

        return await GetFlipRecord(addOnToDelete.FlipRecordId);
    }

    private async Task ValidateAuction(int? auctionId)
    {
        if (auctionId is null)
            return;

        var auctionExists = await _flipLeoUnitOfWork.AuctionRepository
            .AnyAsync(a => a.Id == auctionId);

        if (!auctionExists)
            throw new BadRequestException($"Auction with ID {auctionId} does not exist.");
    }
}
