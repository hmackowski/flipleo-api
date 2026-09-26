using System.Linq.Expressions;
using FlipLeo.Core.DTOs;
using FlipLeo.Core.Exceptions;
using FlipLeo.Repository.Interfaces;
using FlipLeo.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using AuctionDto = FlipLeo.Core.DTOs.Auction;
using AuctionEntity = FlipLeo.Repository.Entities.Auction;

namespace FlipLeo.Services;

public class AuctionService : IAuctionService
{
    private readonly IFlipLeoUnitOfWork _flipLeoUnitOfWork;

    public AuctionService(IFlipLeoUnitOfWork flipLeoUnitOfWork)
    {
        _flipLeoUnitOfWork = flipLeoUnitOfWork ?? throw new ArgumentNullException(nameof(flipLeoUnitOfWork));
    }

    // One projection shared by every read, so the entity -> DTO mapping lives in one place
    private static readonly Expression<Func<AuctionEntity, AuctionDto>> ToDto = a => new AuctionDto
    {
        Id = a.Id,
        Name = a.Name,
        AuctionSiteId = a.AuctionSiteId,
        AuctionSiteName = a.AuctionSite.Name,
        Link = a.Link,
        ImageUrl = a.ImageUrl,
        CurrentPrice = a.CurrentPrice,
        StartTime = a.StartTime,
        EndTime = a.EndTime,
        Notes = a.Notes
    };

    public async Task<AuctionDto[]> GetAuctions()
    {
        return await _flipLeoUnitOfWork.AuctionRepository
            .GetAll()
            .OrderBy(a => a.EndTime)
            .Select(ToDto)
            .ToArrayAsync();
    }

    public async Task<AuctionDto> GetAuction(int auctionId)
    {
        var auction = await _flipLeoUnitOfWork.AuctionRepository
            .Find(a => a.Id == auctionId)
            .Select(ToDto)
            .SingleOrDefaultAsync();

        return auction ?? throw new NotFoundException($"Auction with ID {auctionId} not found");
    }

    public async Task<AuctionDto> AddAuction(AuctionDto auction)
    {
        await Validate(auction);

        var auctionEntity = new AuctionEntity
        {
            Name = auction.Name,
            AuctionSiteId = auction.AuctionSiteId,
            Link = auction.Link,
            ImageUrl = auction.ImageUrl,
            CurrentPrice = auction.CurrentPrice,
            StartTime = auction.StartTime,
            EndTime = auction.EndTime,
            Notes = auction.Notes
        };

        _flipLeoUnitOfWork.AuctionRepository.Add(auctionEntity);
        await _flipLeoUnitOfWork.CommitAsync();

        return await GetAuction(auctionEntity.Id);
    }

    public async Task<AuctionDto> UpdateAuction(AuctionDto auction)
    {
        var auctionToUpdate = await _flipLeoUnitOfWork.AuctionRepository
            .SingleOrDefaultAsync(a => a.Id == auction.Id)
            ?? throw new NotFoundException($"Auction with ID {auction.Id} not found");

        await Validate(auction);

        auctionToUpdate.Name = auction.Name;
        auctionToUpdate.AuctionSiteId = auction.AuctionSiteId;
        auctionToUpdate.Link = auction.Link;
        auctionToUpdate.ImageUrl = auction.ImageUrl;
        auctionToUpdate.CurrentPrice = auction.CurrentPrice;
        auctionToUpdate.StartTime = auction.StartTime;
        auctionToUpdate.EndTime = auction.EndTime;
        auctionToUpdate.Notes = auction.Notes;

        await _flipLeoUnitOfWork.CommitAsync();

        return await GetAuction(auctionToUpdate.Id);
    }

    public async Task<SuccessResult> DeleteAuction(int auctionId)
    {
        var auctionToDelete = await _flipLeoUnitOfWork.AuctionRepository
            .SingleOrDefaultAsync(a => a.Id == auctionId)
            ?? throw new NotFoundException($"Auction with ID {auctionId} not found");

        // Soft delete: the unit of work turns this into IsActive = false
        _flipLeoUnitOfWork.AuctionRepository.Delete(auctionToDelete);
        await _flipLeoUnitOfWork.CommitAsync();

        return new SuccessResult { Success = true, Detail = "Auction deleted!" };
    }

    private async Task Validate(AuctionDto auction)
    {
        if (auction.EndTime < auction.StartTime)
            throw new BadRequestException("End time must be after the start time.");

        var siteExists = await _flipLeoUnitOfWork.LookupAuctionSiteRepository
            .AnyAsync(s => s.Id == auction.AuctionSiteId && s.IsActive);

        if (!siteExists)
            throw new BadRequestException($"Auction site with ID {auction.AuctionSiteId} does not exist.");
    }
}
