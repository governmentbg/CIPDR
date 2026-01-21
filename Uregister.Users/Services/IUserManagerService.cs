using Google.Protobuf.Collections;
using Uregister.Users.Data.Identity;
using URegister.Common;
using URegister.Users;

namespace Uregister.Users.Services
{
    public interface IUserManagerService
    {
        Task<ResultStatus> UpdateUserAsync(UserData userData, Guid userId);

        Task<UpsertUserResponse> CreateUserAsync(UserData userData);

        Task<ResultStatus> RemoveUserLoginAsync(Guid userId);

        Task<AppUser> AuthorizeUserAsync(AuthorizeUserData authorizeUserData);

        Task<ResultStatus> CheckUserRole(RoleData role, Guid userId);

        /// <summary>
        /// Добавяне на роля към потребител
        /// </summary>
        /// <param name="role">Роля</param>
        /// <param name="userId">Идентификатор на потребител</param>
        /// <returns></returns>
        Task<ResultStatus> AssignUserRole(RoleData role, Guid userId);

        /// <summary>
        /// Добавяне на роли към потребител
        /// </summary>
        /// <param name="role">Роля</param>
        /// <param name="userId">Идентификатор на потребител</param>
        /// <returns></returns>
        Task<ResultStatus> AssignUserRoles(IEnumerable<RoleData> roles, Guid userId);

        Task<ResultStatus> UnassignUserRole(RoleData role, Guid userId);

        Task<bool> HasAdministration(Guid administrationId, Guid userId);
        
        Task SetAdministration(Guid administrationId, Guid userId, string administrationName);

        /// <summary>
        /// Връща всички claims от identity_user_claims по userId и urn:io:available_administration
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<ApplicationUserClaim>> GetUserClaimsAvailableAdministrations(Guid userId);

        /// <summary>
        /// Записва в ApplicationUserClaim
        /// </summary>
        /// <param name="userClaimsData"></param>
        /// <returns></returns>
        Task<ResultStatus> AddUserClaims(RepeatedField<UserClaimsData> userClaimsData);

        /// <summary>
        /// Премахване на user claims в ApplicationUserClaim
        /// </summary>
        /// <param name="userClaims"></param>
        /// <returns></returns>
        Task<ResultStatus> RemoveUserClaims(UserClaimsData userClaims);

        /// <summary>
        /// Връща cliams на потребител.
        /// </summary>
        /// <returns></returns>
        Task<List<ApplicationUserClaim>> GetUserClaims(Guid userId);

        /// <summary>
        /// Обновява дадена роля в базата. Обновява само името.
        /// </summary>
        /// <param name="roleData"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        Task<ResultStatus> UpdateRoleAsync(RoleData roleData, Guid roleId);

        /// <summary>
        /// Създава роля в базата.
        /// </summary>
        /// <param name="roleData"></param>
        /// <returns></returns>
        Task<ResultStatus> CreateRoleAsync(RoleData roleData);

        /// <summary>
        /// Изтриване на роля от базата
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        Task<ResultStatus> DeleteRoleAsync(Guid roleId);
        Task<bool> GetReceiveEFormNotification(UserFilter request);
        Task<List<UserListData>> GetUserReceiveEmails(UserReceiveEmailsRequest request, bool eformNotification, bool instructionResponse);
        Task<List<UserListData>> GetUserReceiveEmailsForSrok(UserReceiveEmailsRequest request);
        Task<bool> GetReceiveInstructionResponse(UserFilter request);
    }
}
