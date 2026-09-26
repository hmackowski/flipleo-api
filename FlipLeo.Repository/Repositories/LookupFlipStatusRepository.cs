using FlipLeo.Repository.Entities;
using FlipLeo.Repository.Interfaces.Repositories;

namespace FlipLeo.Repository.Repositories;

public class LookupFlipStatusRepository(FlipLeoContext context)
    : Repository<LookupFlipStatus, FlipLeoContext>(context), ILookupFlipStatusRepository
{
}
