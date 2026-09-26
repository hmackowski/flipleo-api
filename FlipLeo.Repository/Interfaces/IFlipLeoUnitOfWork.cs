using FlipLeo.Repository.Interfaces.Repositories;

namespace FlipLeo.Repository.Interfaces;

public interface IFlipLeoUnitOfWork
{
    #region Repositories

    IAuctionRepository AuctionRepository { get; }
    IFlipRecordRepository FlipRecordRepository { get; }
    IFlipRecordAddOnRepository FlipRecordAddOnRepository { get; }
    ILookupAuctionSiteRepository LookupAuctionSiteRepository { get; }

    #endregion

    Task CommitAsync(CancellationToken cancellationToken = default);
}
