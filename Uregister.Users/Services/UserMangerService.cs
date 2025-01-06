using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Uregister.Users.Constants;
using Uregister.Users.Data;
using Uregister.Users.Data.Identity;
using URegister.Common;
using URegister.Users;

namespace Uregister.Users.Services
{
    public class UserMangerService(
        IUserRepository userRepository,
        ILogger<UserMangerService> logger) : IUserManagerService
    {
        public async Task<AppUser> AuthorizeUserAsync(AuthorizeUserData authorizeUserData)
        {
            AppUser appUser = new AppUser()
            {
                Status = new ResultStatus
                {
                    Code = ResultCodes.Ok
                }
            };

            try
            {
                if (string.IsNullOrEmpty(authorizeUserData.Pid?.Trim()))
                {
                    appUser.Status = new ResultStatus
                    {
                        Code = ResultCodes.BadRequest,
                        Message = "PID is required"
                    };
                }
                else
                {
                    Func<ApplicationUserRole, bool> roleFilter = u => u.RegisterCode == UserManagerConstants.ALL_REGISTERS_CODE;

                    if (authorizeUserData.HasRegisterCode)
                    {
                        roleFilter = u => u.RegisterCode == authorizeUserData.RegisterCode
                            || u.RegisterCode == UserManagerConstants.ALL_REGISTERS_CODE;
                    }

                    var user = await userRepository.AllReadonly<ApplicationUser>()
                        .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                        .FirstOrDefaultAsync(u => u.Logins
                            .Any(l => l.ProviderKey == authorizeUserData.Pid 
                                && l.LoginProvider == UserManagerConstants.LOGINS_PROVIDER));

                    if (user == null)
                    {
                        appUser.Status = new ResultStatus
                        {
                            Code = ResultCodes.NotFound
                        };
                    }
                    else
                    {
                        appUser.AdministrationId = user.AdministrationId.ToString();
                        appUser.Email = user.Email;
                        appUser.FirstName = user.FirstName;
                        appUser.LastName = user.LastName;
                        appUser.Roles.AddRange(user.UserRoles
                            .Where(roleFilter)
                            .Select(ur => ur.Role.Name));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error authorizing user");

                appUser.Status = new ResultStatus
                {
                    Code = ResultCodes.InternalServerError,
                    Message = "Error authorizing user"
                };
            }

            return appUser;
        }

        public async Task<ResultStatus> CreateUserAsync(UserData userData)
        {
            ResultStatus result = new ResultStatus
            {
                Code = ResultCodes.Ok
            };

            try
            {
                ApplicationUser applicationUser = new ApplicationUser
                {
                    AdministrationId = Guid.Parse(userData.AdministrationId),
                    Administration = userData.Administration,
                    Email = userData.Email,
                    EmailConfirmed = true,
                    FirstName = userData.FirstName,
                    LastName = userData.LastName,
                    UserName = userData.UserName,
                    Logins = new List<ApplicationUserLogin>
                {
                    new ApplicationUserLogin
                    {
                        LoginProvider = UserManagerConstants.LOGINS_PROVIDER,
                        ProviderDisplayName = UserManagerConstants.LOGINS_PROVIDER_DISPLAY,
                        ProviderKey = userData.Pid
                    }
                },
                    UserRoles = userData.Roles.Select(r => new ApplicationUserRole
                    {
                        RoleId = Guid.Parse(r.RoleId),
                        RegisterCode = r.HasRegisterCode ? r.RegisterCode : UserManagerConstants.ALL_REGISTERS_CODE
                    }).ToList()
                };

                await userRepository.AddAsync(applicationUser);
                await userRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating user");

                result = new ResultStatus
                {
                    Code = ResultCodes.InternalServerError,
                    Message = "Error creating user"
                };
            }

            return result;
        }

        public async Task<ResultStatus> RemoveUserLoginAsync(Guid userId)
        {
            ResultStatus result = new ResultStatus
            {
                Code = ResultCodes.Ok
            };

            try
            {
                await userRepository.DeleteAsNoTrackingAsync<ApplicationUserLogin>(l => l.UserId == userId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error removing user login");

                result = new ResultStatus
                {
                    Code = ResultCodes.InternalServerError,
                    Message = "Error removing user login"
                };
            }

            return result;
        }

        public async Task<ResultStatus> UpdateUserAsync(UserData userData, Guid userId)
        {
            ResultStatus result = new ResultStatus
            {
                Code = ResultCodes.Ok
            };

            ApplicationUser? appUser;

            try
            {
                appUser = await userRepository.All<ApplicationUser>()
                    .Include(u => u.UserRoles)
                    .Include(u => u.Logins)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (appUser == null)
                {
                    result = new ResultStatus
                    {
                        Code = ResultCodes.NotFound,
                        Message = "User not found"
                    };
                }
                else
                {
                    appUser.AdministrationId = Guid.Parse(userData.AdministrationId);
                    appUser.Administration = userData.Administration;
                    appUser.Email = userData.Email;
                    appUser.FirstName = userData.FirstName;
                    appUser.LastName = userData.LastName;
                    appUser.UserName = userData.UserName;

                    appUser.Logins.Clear();
                    appUser.Logins.Add(new ApplicationUserLogin
                    {
                        LoginProvider = UserManagerConstants.LOGINS_PROVIDER,
                        ProviderDisplayName = UserManagerConstants.LOGINS_PROVIDER_DISPLAY,
                        ProviderKey = userData.Pid
                    });

                    appUser.UserRoles.Clear();
                    
                    foreach (var role in userData.Roles)
                    {
                        Guid roleId;

                        if (Guid.TryParse(role.RoleId, out roleId))
                        {
                            appUser.UserRoles.Add(new ApplicationUserRole
                            {
                                RoleId = roleId,
                                RegisterCode = role.HasRegisterCode ? role.RegisterCode : UserManagerConstants.ALL_REGISTERS_CODE
                            });
                        }
                    }

                    await userRepository.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating user");

                result = new ResultStatus
                {
                    Code = ResultCodes.InternalServerError,
                    Message = "Error updating user"
                };
            }

            return result;
        }
    }
}
