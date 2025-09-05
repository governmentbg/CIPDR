using Microsoft.EntityFrameworkCore.Infrastructure;
using URegister.Infrastructure.Data.Common;

namespace Uregister.Users.Data
{
    public interface IUserRepository : IRepository
    {
        public DatabaseFacade Db { get; }
    }
}
