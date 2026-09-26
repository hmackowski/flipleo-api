using FlipLeo.Core.DTOs;

namespace FlipLeo.Services.Interfaces;

public interface ILookupService
{
    Task<AuctionSite[]> GetAuctionSites();
}
