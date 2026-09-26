using FlipLeo.Repository.Entities;
using FlipLeo.Repository.Interfaces.Repositories;

namespace FlipLeo.Repository.Repositories;

public class AuctionRepository(FlipLeoContext context)
    : Repository<Auction, FlipLeoContext>(context), IAuctionRepository
{
}
