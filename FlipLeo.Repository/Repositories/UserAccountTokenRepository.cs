using FlipLeo.Repository.Entities;
using FlipLeo.Repository.Interfaces.Repositories;

namespace FlipLeo.Repository.Repositories;

public class UserAccountTokenRepository(FlipLeoContext context)
    : Repository<UserAccountToken, FlipLeoContext>(context), IUserAccountTokenRepository
{
}
