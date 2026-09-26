using FlipLeo.Repository.Entities;
using FlipLeo.Repository.Interfaces.Repositories;

namespace FlipLeo.Repository.Repositories;

public class FlipRecordAddOnRepository(FlipLeoContext context)
    : Repository<FlipRecordAddOn, FlipLeoContext>(context), IFlipRecordAddOnRepository
{
}
