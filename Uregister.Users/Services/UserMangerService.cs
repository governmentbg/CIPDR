using Amazon.Runtime.Internal;
using Google.Protobuf.Collections;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Uregister.Users.Constants;
using Uregister.Users.Data;
using Uregister.Users.Data.Identity;
using Uregister.Users.Data.Models;
using URegister.Common;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Extensions;
using URegister.Infrastructure.Helper;
using URegister.Users;
using static FastExpressionCompiler.ExpressionCompiler;

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
                        Message = "Идентификаторът е задължителен"
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
                        .Include(u => u.Claims)
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
                        appUser.Id = user.Id.ToString();
                        appUser.AdministrationId = user.AdministrationId.ToString();
                        appUser.Email = user.Email;
                        appUser.FirstName = user.FirstName;
                        appUser.LastName = user.LastName;
                        appUser.Roles.AddRange(user.UserRoles
                            .Where(roleFilter)
                            .Select(ur => ur.Role.Name));
                        appUser.Claims.AddRange(user.Claims
                            .Select(c => new AppClaim()
                            {
                                Type = c.ClaimType,
                                Value = c.ClaimValue
                            })
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при упълномощаване на потребител {ex.InnerException?.Message} pid: {authorizeUserData.Pid ?? "липсва"}, registerCode: {authorizeUserData.RegisterCode ?? "липсва"}");
                

                appUser.Status = new ResultStatus
                {
                    Code = ResultCodes.InternalServerError,
                    Message = "Грешка при упълномощаване на потребител"
                };
            }

            return appUser;
        }

        public async Task<UpsertUserResponse> CreateUserAsync(UserData userData)
        {
            UpsertUserResponse result = new UpsertUserResponse
            {
                Status = new ResultStatus { Code = ResultCodes.Ok }
            };

            try
            {
                if (await userRepository.AllReadonly<ApplicationUserLogin>().AnyAsync(u => u.ProviderKey == userData.Pid))
                {
                    result.Status = new ResultStatus
                    {
                        Code = ResultCodes.NotSet,
                        Message = "Потребител с това ЕГН вече съществува."
                    };
                    return result;
                }

                ApplicationUser applicationUser = new ApplicationUser
                {
                    AdministrationId = Guid.Parse(userData.AdministrationId),
                    Administration = userData.Administration,
                    Email = userData.Email,
                    PhoneNumber = userData.PhoneNumber,
                    EmailConfirmed = true,
                    FirstName = userData.FirstName,
                    MiddleName = userData.MiddleName,
                    LastName = userData.LastName,
                    UserName = userData.UserName,
                    Position = userData.Position,
                    Enable = false,
                    CreatedAt = DateTime.UtcNow,
                    ReceiveEmailOnError = userData.ReceiveEmailOnError,
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
                ApplicationUserClaim claims = new ApplicationUserClaim
                {
                    User = applicationUser,
                    ClaimType = CustomClaimType.AvailableAdministration,
                    ClaimValue = applicationUser.AdministrationId.ToString()
                };
                applicationUser.Claims.Add(claims);
                if (applicationUser.Administration.Equals("всички администрации", StringComparison.InvariantCultureIgnoreCase))
                {
                    var globalAdmin = await userRepository.AllReadonly<ApplicationRole>().FirstOrDefaultAsync(r => r.Name == UserRoles.GlobalAdmin);

                    applicationUser.UserRoles.Add(new ApplicationUserRole
                    {
                        RoleId = globalAdmin.Id,
                        RegisterCode = UserManagerConstants.ALL_REGISTERS_CODE
                    });
                }

                await userRepository.AddAsync(applicationUser);
                if (userData.HasRegisterCode && userData.HasReceiveEFormNotification)
                {
                    await SaveReceiveEFormNotification(applicationUser.Id,
                                                       userData.AdministrationId.ToGuid() ?? Guid.Empty,
                                                       userData.RegisterCode,
                                                       userData.ReceiveEFormNotification);
                }
                await userRepository.SaveChangesAsync();
                result.UserId = applicationUser.Id.ToString();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Грешка при създаване на потребител");

                result.Status = new ResultStatus
                {
                    Code = ResultCodes.InternalServerError,
                    Message = "Грешка при създаване на потребител"
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
                logger.LogError(ex, "Грешка при премахване на входа на потребителя");

                result = new ResultStatus
                {
                    Code = ResultCodes.InternalServerError,
                    Message = "Грешка при премахване на входа на потребителя"
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
                        Message = "Потребителят не е намерен"
                    };
                }
                else
                {
                    appUser.AdministrationId = Guid.Parse(userData.AdministrationId);
                    appUser.Administration = userData.Administration;
                    appUser.Email = userData.Email;
                    appUser.PhoneNumber = userData.PhoneNumber;
                    appUser.Position = userData.Position;
                    appUser.FirstName = userData.FirstName;
                    appUser.MiddleName = userData.MiddleName;
                    appUser.LastName = userData.LastName;
                    appUser.UserName = userData.UserName;
                    appUser.Enable = userData.Enabled;
                    appUser.ReceiveEmailOnError = userData.ReceiveEmailOnError;
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
                    if (userData.HasRegisterCode && userData.HasReceiveEFormNotification)
                    {
                        await SaveReceiveEFormNotification(appUser.Id,
                                                           userData.AdministrationId.ToGuid() ?? Guid.Empty,
                                                           userData.RegisterCode,
                                                           userData.ReceiveEFormNotification);
                    }
                    await userRepository.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Грешка при актуализиране на потребителя");

                result = new ResultStatus
                {
                    Code = ResultCodes.InternalServerError,
                    Message = "Грешка при актуализиране на потребителя"
                };
            }

            return result;
        }

        public async Task<ResultStatus> CheckUserRole(RoleData role, Guid userId)
        {
            ResultStatus status = new ResultStatus
            {
                Code = ResultCodes.Ok
            };

            try
            {
                Guid roleId;

                if (Guid.TryParse(role.RoleId, out roleId) == false)
                {
                    status = new ResultStatus
                    {
                        Code = ResultCodes.BadRequest,
                        Message = "Невалиден Guid идентификатор на роля"
                    };
                }
                if (await userRepository.AllReadonly<ApplicationUserRole>().AnyAsync(x => x.UserId == userId && x.RoleId == roleId && x.RegisterCode == role.RegisterCode))
                {
                    status.Code = ResultCodes.BadRequest;
                    status.Message = "Потребителят вече има тази роля";
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Грешка при проверка на ролята на потребителя");

                status = new ResultStatus
                {
                    Code = ResultCodes.InternalServerError,
                    Message = "Грешка при проверка на ролята на потребителя"
                };
            }
            return status;
        }

        /// <summary>
        /// Добавяне на роля към потребител
        /// </summary>
        /// <param name="role">Роля</param>
        /// <param name="userId">Идентификатор на потребител</param>
        /// <returns></returns>
        public async Task<ResultStatus> AssignUserRole(RoleData role, Guid userId)
        {
            ResultStatus status = new ResultStatus
            {
                Code = ResultCodes.Ok
            };

            try
            {
                Guid roleId;

                if (Guid.TryParse(role.RoleId, out roleId) == false)
                {
                    status = new ResultStatus
                    {
                        Code = ResultCodes.BadRequest,
                        Message = "Невалиден Guid идентификатор на роля"
                    };
                }

                await userRepository.AddAsync(new ApplicationUserRole
                {
                    UserId = userId,
                    RoleId = roleId,
                    RegisterCode = role.RegisterCode
                });
                await userRepository.SaveChangesAsync();
                status.Message = "Ролята е присвоена успешно";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Грешка при присвояване на роля на потребител");

                status = new ResultStatus
                {
                    Code = ResultCodes.InternalServerError,
                    Message = "Грешка при добавяне на роля"
                };
            }
            return status;
        }

        /// <summary>
        /// Добавяне на роли към потребител
        /// </summary>
        /// <param name="roles">Роли</param>
        /// <param name="userId">Идентификатор на потребител</param>
        /// <returns></returns>
        public async Task<ResultStatus> AssignUserRoles(IEnumerable<RoleData> roles, Guid userId)
        {
            ResultStatus status = new ResultStatus
            {
                Code = ResultCodes.Ok
            };

            try
            {
                List<ApplicationUserRole> appRolesToAdd = new List<ApplicationUserRole>();

                foreach (var role in roles)
                {
                    Guid roleId;

                    if (!Guid.TryParse(role.RoleId, out roleId))
                    {
                        return new ResultStatus
                        {
                            Code = ResultCodes.BadRequest,
                            Message = $"Невалиден Guid идентификатор на роля {role.Label}"
                        };
                    }

                    appRolesToAdd.Add(new ApplicationUserRole
                    {
                        UserId = userId,
                        RoleId = roleId,
                        RegisterCode = role.RegisterCode
                    });
                }

                await userRepository.AddRangeAsync(appRolesToAdd);
                await userRepository.SaveChangesAsync();
                status.Message = "Ролити са добавени успешно.";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Грешка при присвояване на роля на потребител");

                return new ResultStatus
                {
                    Code = ResultCodes.InternalServerError,
                    Message = "Грешка при присвояване на роля на потребител"
                };
            }

            return status;
        }

        public async Task<ResultStatus> UnassignUserRole(RoleData role, Guid userId)
        {
            ResultStatus status = new ResultStatus
            {
                Code = ResultCodes.Ok
            };

            try
            {
                Guid roleId;

                if (Guid.TryParse(role.RoleId, out roleId) == false)
                {
                    status = new ResultStatus
                    {
                        Code = ResultCodes.BadRequest,
                        Message = "Невалиден Guid идентификатор на роля."
                    };
                }

                userRepository.Delete(new ApplicationUserRole
                {
                    UserId = userId,
                    RoleId = roleId,
                    RegisterCode = role.RegisterCode
                });
                await userRepository.SaveChangesAsync();
                status.Message = "Ролята е премахната успешно";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Грешка при премахване на роля");

                status = new ResultStatus
                {
                    Code = ResultCodes.InternalServerError,
                    Message = "Грешка при премахване на роля"
                };
            }
            return status;
        }

        public async Task<bool> HasAdministration(Guid administrationId, Guid userId)
        {
            string admId = administrationId.ToString();
            return await userRepository.AllReadonly<ApplicationUserClaim>()
                .AnyAsync(c => c.UserId == userId && c.ClaimValue == admId && c.ClaimType == CustomClaimType.AvailableAdministration);
        }

        public async Task SetAdministration(Guid administrationId, Guid userId, string administrationName)
        {
            await userRepository.All<ApplicationUser>(u => u.Id == userId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(u => u.AdministrationId, administrationId)
                    .SetProperty(u => u.Administration, administrationName));
        }

        public async Task<List<ApplicationUserClaim>> GetUserClaimsAvailableAdministrations(Guid userId)
        {
            return await userRepository.AllReadonly<ApplicationUserClaim>()
                .Where(c => c.UserId == userId && c.ClaimType == CustomClaimType.AvailableAdministration).ToListAsync();
        }

        public async Task<ResultStatus> AddUserClaims(RepeatedField<UserClaimsData> userClaimsData)
        {
            var result = CommonGrpcHelper.CreateStatusOK();
            foreach (var claimsData in userClaimsData)
            {
                Guid userId = Guid.Empty;

                if (Guid.TryParse(claimsData.UserId.Trim(), out userId) == false)
                {
                    result = new ResultStatus
                    {
                        Code = ResultCodes.BadRequest,
                        Message = "Потребителски идентификатор е задължителен и трябва да е валиден Guid"
                    };
                    return result;
                }

                await userRepository.AddAsync(new ApplicationUserClaim
                {
                    UserId = userId,
                    ClaimType = claimsData.ClaimType,
                    ClaimValue = claimsData.ClaimValue
                });
                await userRepository.SaveChangesAsync();
            }

            return result;
        }

        public async Task<ResultStatus> RemoveUserClaims(UserClaimsData userClaims)
        {
            var result = CommonGrpcHelper.CreateStatusOK();
            Guid userId = Guid.Empty;

            if (Guid.TryParse(userClaims.UserId.Trim(), out userId) == false)
            {
                result = new ResultStatus
                {
                    Code = ResultCodes.BadRequest,
                    Message = "Потребителски идентификатор е задължителен и трябва да е валиден Guid"
                };
                return result;
            }

            await userRepository.All<ApplicationUserClaim>(c =>
                    c.UserId == userId &&
                    c.ClaimType == userClaims.ClaimType &&
                    c.ClaimValue == userClaims.ClaimValue)
                .ExecuteDeleteAsync();

            return result;
        }

        public async Task<List<ApplicationUserClaim>> GetUserClaims(Guid userId)
        {
            return await userRepository.AllReadonly<ApplicationUserClaim>().Where(cl => cl.UserId == userId).ToListAsync();
        }

        public async Task<ResultStatus> UpdateRoleAsync(RoleData roleData, Guid roleId)
        {
            ResultStatus result = new ResultStatus
            {
                Code = ResultCodes.Ok
            };
            try
            {
                ApplicationRole? appRole = await userRepository.All<ApplicationRole>().FirstOrDefaultAsync(u => u.Id == roleId);
                if (await userRepository.AllReadonly<ApplicationRole>().AnyAsync(u => u.Label.ToLower() == roleData.Label.ToLower()))
                {
                    result = new ResultStatus
                    {
                        Code = ResultCodes.NotSet,
                        Message = "Роля с това име вече съществува."
                    };
                    return result;
                }
                if (appRole == null)
                {
                    result = new ResultStatus
                    {
                        Code = ResultCodes.NotFound,
                        Message = "Ролята не е намерена"
                    };
                }
                else
                {
                    appRole.Label = roleData.Label;
                    await userRepository.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Грешка при актуализиране на роля");

                result = new ResultStatus
                {
                    Code = ResultCodes.InternalServerError,
                    Message = "Грешка при актуализиране на роля"
                };
            }

            return result;
        }

        public async Task<ResultStatus> CreateRoleAsync(RoleData roleData)
        {
            ResultStatus result = new ResultStatus
            {
                Code = ResultCodes.Ok
            };

            try
            {
                if (await userRepository.AllReadonly<ApplicationRole>().AnyAsync(u => u.Label.ToLower() == roleData.Label.ToLower()))
                {
                    result = new ResultStatus
                    {
                        Code = ResultCodes.NotSet,
                        Message = "Роля с това име вече съществува."
                    };
                    return result;
                }

                ApplicationRole appRole = new ApplicationRole()
                {
                    Id = Guid.NewGuid(),
                    Label = roleData.Label
                };
                appRole.Name = appRole.Id.ToString();
                appRole.NormalizedName = appRole.Id.ToString();
                await userRepository.AddAsync(appRole);
                await userRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Грешка при създаване на роля");

                result = new ResultStatus
                {
                    Code = ResultCodes.InternalServerError,
                    Message = "Грешка при създаване на роля"
                };
            }

            return result;
        }

        public async Task<ResultStatus> DeleteRoleAsync(Guid roleId)
        {
            ResultStatus result = new ResultStatus
            {
                Code = ResultCodes.Ok
            };

            try
            {
                await userRepository.DeleteAsync<ApplicationRole>(roleId);
                await userRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Грешка при създаване на роля");

                result = new ResultStatus
                {
                    Code = ResultCodes.InternalServerError,
                    Message = "Грешка при създаване на роля"
                };
            }

            return result;
        }

        public async Task<bool> GetReceiveEFormNotification(UserFilter request)
        {
            if (!string.IsNullOrEmpty(request.RegisterCode) && !string.IsNullOrEmpty(request.AdministrationId))
            {
                var administrationId = request.AdministrationId.ToGuid();
                var userId = request.Id.ToGuid();
                return await userRepository.AllReadonly<UserEMailReceive>()
                                                 .Where(x => x.UserId == userId &&
                                                             x.RegisterCode == request.RegisterCode &&
                                                             x.AdministrationId == administrationId)
                                                 .Select(x => x.ReceiveEFormNotification)
                                                 .FirstOrDefaultAsync();
            }
            return false;
        }

        public async Task SaveReceiveEFormNotification(
            Guid userId, 
            Guid administrationId, 
            string registerCode,
            bool receiveEFormNotification)
        {
            var data = await userRepository.All<UserEMailReceive>()
                                             .Where(x => x.UserId ==  userId &&
                                                         x.RegisterCode == registerCode &&
                                                         x.AdministrationId == administrationId)
                                             .FirstOrDefaultAsync();
            if (data == null)
            {
                data = new UserEMailReceive
                {
                    UserId = userId,
                    RegisterCode = registerCode,
                    AdministrationId = administrationId,
                };
                await userRepository.AddAsync(data);
            }
            data.ReceiveEFormNotification = receiveEFormNotification;
            data.ModifiedOn = DateTime.UtcNow;
            data.ModifiedByUserId = userId;
        }

        public async Task<List<UserListData>> GetUserReceiveEmails(UserReceiveEmailsRequest request)
        {
            var administrationId = request.AdministrationId.ToGuid();
            var mails = userRepository.AllReadonly<UserEMailReceive>()
                                      .Where(x => x.RegisterCode == request.RegisterCode &&
                                                  x.AdministrationId == administrationId);
            return await userRepository.AllReadonly<ApplicationUser>()
                                       .Where(x => mails.Any(m => m.UserId == x.Id))
                                       .Select(u => new UserListData
                                       {
                                           Id = u.Id.ToString(),
                                           Email = u.Email,
                                           FirstName = u.FirstName,
                                           MiddleName = u.MiddleName,
                                           LastName = u.LastName,
                                           Enabled = u.Enable,
                                           RoleName = string.Join(", ", u.UserRoles.Select(r => r.Role.Label + "(" + r.RegisterCode + ")"))
                                       })
                                       .ToListAsync();
        }
    }
}