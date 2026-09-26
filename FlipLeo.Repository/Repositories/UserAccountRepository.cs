using FlipLeo.Repository.Entities;
using FlipLeo.Repository.Interfaces.Repositories;

namespace FlipLeo.Repository.Repositories;

public class UserAccountRepository(FlipLeoContext context)
    : Repository<UserAccount, FlipLeoContext>(context), IUserAccountRepository
{
}
