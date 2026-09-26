using FlipLeo.Repository.Entities;
using FlipLeo.Repository.Interfaces.Repositories;

namespace FlipLeo.Repository.Repositories;

public class AddOnPresetRepository(FlipLeoContext context)
    : Repository<AddOnPreset, FlipLeoContext>(context), IAddOnPresetRepository
{
}
