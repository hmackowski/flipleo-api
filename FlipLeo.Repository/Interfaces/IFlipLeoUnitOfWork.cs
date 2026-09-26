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
    ILookupFlipStatusRepository LookupFlipStatusRepository { get; }
    IUserAccountRepository UserAccountRepository { get; }
    IUserAccountTokenRepository UserAccountTokenRepository { get; }

    #endregion

    Task CommitAsync(CancellationToken cancellationToken = default);
}
