using FlipLeo.Repository.Interfaces.Repositories;

namespace FlipLeo.Repository.Interfaces;

public interface IFlipLeoUnitOfWork
{
    #region Repositories

    IAddOnPresetRepository AddOnPresetRepository { get; }
    IAuctionRepository AuctionRepository { get; }
    IFlipRecordRepository FlipRecordRepository { get; }
    IFlipRecordAddOnRepository FlipRecordAddOnRepository { get; }
    ILookupAuctionSiteRepository LookupAuctionSiteRepository { get; }
    IUserAccountRepository UserAccountRepository { get; }

    #endregion

    Task CommitAsync(CancellationToken cancellationToken = default);
}
