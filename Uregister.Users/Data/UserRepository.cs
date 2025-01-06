using URegister.Infrastructure.Data.Common;

namespace Uregister.Users.Data
{
    public class UserRepository : Repository, IUserRepository
    {
        public UserRepository(UserDbContext userDbContext)
        {
            Context = userDbContext;
        }
    }
}
