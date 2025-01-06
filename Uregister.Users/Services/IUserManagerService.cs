using URegister.Common;
using URegister.Users;

namespace Uregister.Users.Services
{
    public interface IUserManagerService
    {
        Task<ResultStatus> UpdateUserAsync(UserData userData, Guid userId);

        Task<ResultStatus> CreateUserAsync(UserData userData);

        Task<ResultStatus> RemoveUserLoginAsync(Guid userId);

        Task<AppUser> AuthorizeUserAsync(AuthorizeUserData authorizeUserData);
    }
}
