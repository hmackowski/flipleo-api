using FlipLeo.Core.DTOs;
using FlipLeo.Repository.Interfaces;
using FlipLeo.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlipLeo.Services;

public class LookupService : ILookupService
{
    private readonly IFlipLeoUnitOfWork _flipLeoUnitOfWork;

    public LookupService(IFlipLeoUnitOfWork flipLeoUnitOfWork)
    {
        _flipLeoUnitOfWork = flipLeoUnitOfWork ?? throw new ArgumentNullException(nameof(flipLeoUnitOfWork));
    }

    public async Task<AuctionSite[]> GetAuctionSites()
    {
        return await _flipLeoUnitOfWork.LookupAuctionSiteRepository
            .Find(s => s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new AuctionSite
            {
                Id = s.Id,
                Name = s.Name,
                WebsiteUrl = s.WebsiteUrl
            })
            .ToArrayAsync();
    }

    public async Task<FlipStatus[]> GetFlipStatuses()
    {
        return await _flipLeoUnitOfWork.LookupFlipStatusRepository
            .Find(s => s.IsActive)
            .OrderBy(s => s.SortOrder)
            .Select(s => new FlipStatus
            {
                Id = s.Id,
                Name = s.Name
            })
            .ToArrayAsync();
    }
}
