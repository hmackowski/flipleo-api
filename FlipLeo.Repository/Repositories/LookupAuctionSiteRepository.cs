using FlipLeo.Repository.Entities;
using FlipLeo.Repository.Interfaces.Repositories;

namespace FlipLeo.Repository.Repositories;

public class LookupAuctionSiteRepository(FlipLeoContext context)
    : Repository<LookupAuctionSite, FlipLeoContext>(context), ILookupAuctionSiteRepository
{
}
