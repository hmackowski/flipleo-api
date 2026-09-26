using FlipLeo.Core.DTOs;

namespace FlipLeo.Services.Interfaces;

public interface IAuctionService
{
    Task<Auction[]> GetAuctions();

    Task<Auction> GetAuction(int auctionId);

    Task<Auction> AddAuction(Auction auction);

    Task<Auction> UpdateAuction(Auction auction);

    Task<SuccessResult> DeleteAuction(int auctionId);
}
