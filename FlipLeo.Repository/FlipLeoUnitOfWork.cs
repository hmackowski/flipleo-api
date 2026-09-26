using FlipLeo.Core.Interfaces;
using FlipLeo.Repository.Interfaces;
using FlipLeo.Repository.Interfaces.Repositories;
using FlipLeo.Repository.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FlipLeo.Repository;

public class FlipLeoUnitOfWork(FlipLeoContext context, ICurrentUserService currentUserService) : IFlipLeoUnitOfWork
{
    #region Repositories

    private IAddOnPresetRepository? _addOnPresetRepository;
    public IAddOnPresetRepository AddOnPresetRepository =>
        _addOnPresetRepository ??= new AddOnPresetRepository(context);

    private IAuctionRepository? _auctionRepository;
    public IAuctionRepository AuctionRepository =>
        _auctionRepository ??= new AuctionRepository(context);

    private IFlipRecordRepository? _flipRecordRepository;
    public IFlipRecordRepository FlipRecordRepository =>
        _flipRecordRepository ??= new FlipRecordRepository(context);

    private IFlipRecordAddOnRepository? _flipRecordAddOnRepository;
    public IFlipRecordAddOnRepository FlipRecordAddOnRepository =>
        _flipRecordAddOnRepository ??= new FlipRecordAddOnRepository(context);

    private ILookupAuctionSiteRepository? _lookupAuctionSiteRepository;
    public ILookupAuctionSiteRepository LookupAuctionSiteRepository =>
        _lookupAuctionSiteRepository ??= new LookupAuctionSiteRepository(context);

    private IUserAccountRepository? _userAccountRepository;
    public IUserAccountRepository UserAccountRepository =>
        _userAccountRepository ??= new UserAccountRepository(context);

    #endregion Repositories

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        UpdateSoftDeleteStatuses();
        ProcessEntityChanges();

        // A single SaveChangesAsync runs in its own transaction: all staged changes save, or none do.
        await context.SaveChangesAsync(cancellationToken);
    }

    #region Internal Methods

    /// <summary>Stamps the audit columns on anything being added or modified (same as PivotalUnitOfWork).</summary>
    internal void ProcessEntityChanges()
    {
        var now = DateTime.UtcNow;
        var userId = currentUserService.GetUserId();
        var username = currentUserService.GetUsername();

        foreach (var entry in context.ChangeTracker.Entries<IEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = userId;
                    entry.Entity.CreatedByUsername = username;
                    entry.Entity.CreatedDate = now;
                    entry.Entity.UpdatedBy = userId;
                    entry.Entity.UpdatedByUsername = username;
                    entry.Entity.UpdatedDate = now;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedBy = userId;
                    entry.Entity.UpdatedByUsername = username;
                    entry.Entity.UpdatedDate = now;

                    // Never let an update overwrite who/when a row was created
                    entry.Property(nameof(IEntity.CreatedBy)).IsModified = false;
                    entry.Property(nameof(IEntity.CreatedByUsername)).IsModified = false;
                    entry.Property(nameof(IEntity.CreatedDate)).IsModified = false;
                    break;
            }
        }
    }

    /// <summary>Turns deletes of ISoftDeletable entities into IsActive = false.</summary>
    internal void UpdateSoftDeleteStatuses()
    {
        foreach (var entry in context.ChangeTracker.Entries<ISoftDeletable>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.IsActive = true;
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsActive = false;
                    break;
            }
        }
    }

    #endregion Internal Methods
}
