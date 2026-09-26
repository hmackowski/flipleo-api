using FlipLeo.Repository.Entities;
using FlipLeo.Repository.Interfaces.Repositories;

namespace FlipLeo.Repository.Repositories;

public class FlipRecordRepository(FlipLeoContext context)
    : Repository<FlipRecord, FlipLeoContext>(context), IFlipRecordRepository
{
}
