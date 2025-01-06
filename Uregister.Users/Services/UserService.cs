using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Uregister.Users.Data.Identity;
using URegister.Common;
using URegister.Users;

namespace Uregister.Users.Services
{
    public class UserService (
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IUserManagerService userManagerService,
        ILogger<UserService> logger) : AppUserManager.AppUserManagerBase
    {
        public override async Task<AppRoles> GetRoles(Empty request, ServerCallContext context)
        {
            var roles = new AppRoles()
            {
                Status = GetOkResult()
            };

            try
            {
                var appRoles = await roleManager.Roles
                    .Select(r => new RoleData
                    {
                        Label = r.Label,
                        RoleId = r.Id.ToString()
                    })
                    .ToListAsync();

                roles.Roles.AddRange(appRoles);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting roles");
                
                roles.Status = new ResultStatus
                {
                    Code = ResultCodes.InternalServerError,
                    Message = "Error getting roles"
                };
            }

            return roles;
        }

        public override async Task<UserList> GetUserList(UserFilter request, ServerCallContext context)
        {
            var users = new UserList()
            {
                Status = GetOkResult()
            };

            Guid admId = Guid.Empty;

            if (request.HasAdministrationId == false || 
                string.IsNullOrEmpty(request.AdministrationId?.Trim()) || 
                Guid.TryParse(request.AdministrationId?.Trim(), out admId) == false)
            {
                users.Status = new ResultStatus
                {
                    Code = ResultCodes.BadRequest,
                    Message = "AdministrationId is required and must be a valid Guid"
                };
            }
            else
            {
                try
                {
                    var usersQuery = userManager.Users
                        .Where(u => u.AdministrationId == admId)
                        .Select(u => new UserListData
                        {
                            Id = u.Id.ToString(),
                            Email = u.Email,
                            FirstName = u.FirstName,
                            LastName = u.LastName
                        })
                        .OrderBy(u => u.FirstName)
                        .ThenBy(u => u.LastName);

                    IEnumerable<UserListData> usersList;

                    if (request.PageSize > 0 && request.Page > 0)
                    {
                        usersList = await usersQuery
                            .Skip((request.Page - 1) * request.PageSize)
                            .Take(request.PageSize)
                            .ToListAsync();
                    }
                    else
                    {
                        usersList = await usersQuery
                            .ToListAsync();
                    }

                    users.Users.AddRange(usersList);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error getting users");

                    users.Status = new ResultStatus
                    {
                        Code = ResultCodes.InternalServerError,
                        Message = "Error getting users"
                    };
                }
            }

            return users;
        }

        public override async Task<UserResult> GetUserById(UserFilter request, ServerCallContext context)
        {
            UserResult user = new UserResult()
            {
                Status = GetOkResult()
            };

            Guid userId = Guid.Empty;

            if (request.HasId == false ||
                string.IsNullOrEmpty(request.Id?.Trim()) ||
                Guid.TryParse(request.Id?.Trim(), out userId) == false)
            {
                user.Status = new ResultStatus
                {
                    Code = ResultCodes.BadRequest,
                    Message = "UserId is required and must be a valid Guid"
                };
            }
            else
            {
                try
                {
                    var appUser = await userManager.FindByIdAsync(userId.ToString());

                    if (appUser == null)
                    {
                        user.Status = new ResultStatus
                        {
                            Code = ResultCodes.NotFound,
                            Message = "User not found"
                        };
                    }
                    else
                    {
                        user.User = new UserData
                        {
                            Id = appUser.Id.ToString(),
                            Email = appUser.Email,
                            FirstName = appUser.FirstName,
                            LastName = appUser.LastName,
                            AdministrationId = appUser.AdministrationId.ToString(),
                            Administration = appUser.Administration,
                            UserName = appUser.UserName,
                            Pid = appUser.Logins.FirstOrDefault()?.ProviderKey ?? string.Empty
                        };

                        var roles = await userManager.Users
                            .Where(u => u.Id == userId)
                            .SelectMany(u => u.UserRoles)
                            .Select(ur => new RoleData()
                            {
                                Label = ur.Role.Label,
                                RoleId = ur.RoleId.ToString(),
                                RegisterCode = ur.RegisterCode
                            })
                            .ToListAsync();

                        user.User.Roles.AddRange(roles);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error getting user");

                    user.Status = new ResultStatus
                    {
                        Code = ResultCodes.InternalServerError,
                        Message = "Error getting user"
                    };
                }
            }

            return user;
        }

        public override async Task<ResultStatus> UpsertUser(UserData request, ServerCallContext context)
        {
            ResultStatus result;

            if (request.HasId && Guid.TryParse(request.Id, out Guid userId))
            {
                result = await userManagerService.UpdateUserAsync(request, userId);
            }
            else
            {
                result = await userManagerService.CreateUserAsync(request);
            }

            return result;
        }

        public override async Task<AppUser> AuthorizeUser(AuthorizeUserData request, ServerCallContext context)
        {
            return await userManagerService.AuthorizeUserAsync(request);
        }

        public override async Task<ResultStatus> RemoveUserLogin(UserFilter request, ServerCallContext context)
        {
            ResultStatus result = GetOkResult();

            Guid userId;

            if (request.HasId == false ||
                string.IsNullOrEmpty(request.Id?.Trim()) ||
                Guid.TryParse(request.Id?.Trim(), out userId) == false)
            {
                result = new ResultStatus
                {
                    Code = ResultCodes.BadRequest,
                    Message = "UserId is required and must be a valid Guid"
                };
            }
            else
            {
                result = await userManagerService.RemoveUserLoginAsync(userId);
            }

            return result;
        }

        private ResultStatus GetOkResult()
        {
            return new ResultStatus
            {
                Code = ResultCodes.Ok
            };
        }
    }
}
