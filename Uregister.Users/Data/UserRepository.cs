using Microsoft.EntityFrameworkCore.Infrastructure;
using URegister.Infrastructure.Data.Common;

namespace Uregister.Users.Data
{
    public class UserRepository : Repository, IUserRepository
    {
        public UserRepository(UserDbContext userDbContext)
        {
            Context = userDbContext;
        }

        public DatabaseFacade Db { get => Context.Database; }
    }
}
