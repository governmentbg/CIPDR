using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Uregister.Users.Constants;
using Uregister.Users.Data.Identity;
using URegister.Common;
using URegister.Infrastructure.Constants;
using URegister.Users;
using URegister.Infrastructure.Extensions;
using URegister.Infrastructure.Helper;
using System.Linq.Expressions;

namespace Uregister.Users.Services
{
    public class UserService(
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
                        RoleId = r.Id.ToString(),
                        Name = r.Name ?? string.Empty,
                    })
                    .Where(r => r.Name != UserRoles.Manager)//Несъответствие #357492
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
                (!string.IsNullOrWhiteSpace(request.AdministrationId) &&
                Guid.TryParse(request.AdministrationId?.Trim(), out admId) == false))
            {
                users.Status = new ResultStatus
                {
                    Code = ResultCodes.BadRequest,
                    Message = "Идентификатор на администрация трябва да е валиден Guid"
                };
            }
            else
            {
                try
                {
                    var usersQuery = userManager.Users.TagWith(nameof(GetUserList))
                        .Where(u => (Guid.Empty == admId || 
                                    u.AdministrationId == admId ||
                                    (u.Claims.Any(c => c.ClaimType == CustomClaimType.AvailableAdministration && 
                                                       c.ClaimValue == request.AdministrationId))) &&
                                    (string.IsNullOrWhiteSpace(request.FirstName) || EF.Functions.ILike(u.FirstName, "%" + request.FirstName + "%")) &&
                                    (string.IsNullOrWhiteSpace(request.MiddleName) || EF.Functions.ILike(u.MiddleName, "%" + request.MiddleName + "%")) &&
                                    (string.IsNullOrWhiteSpace(request.LastName) || EF.Functions.ILike(u.LastName, "%" + request.LastName + "%")) &&
                                    (string.IsNullOrWhiteSpace(request.Email) || (!string.IsNullOrWhiteSpace(u.Email) && EF.Functions.ILike(u.Email, "%" + request.Email + "%"))) &&
                                    (string.IsNullOrWhiteSpace(request.RoleId) || u.UserRoles.Any(r => r.Role.Id.ToString() == request.RoleId)) &&
                                    (!request.HasReceiveEmailOnError || u.ReceiveEmailOnError == request.ReceiveEmailOnError) &&  
                                    (request.ActiveUsers != true || u.Enable)
                                    )
                        .Select(u => new UserListData
                        {
                            Id = u.Id.ToString(),
                            Email = u.Email ?? string.Empty,
                            FirstName = u.FirstName,
                            MiddleName = u.MiddleName,
                            LastName = u.LastName,
                            Enabled = u.Enable,
                            RoleName = string.Join(", ", u.UserRoles.Select(r => r.Role.Label + "(" + r.RegisterCode + ")"))
                        });

                    users.UserCount = await usersQuery.CountAsync();
                  
                    if (request.DatatableRequest.Length < 0)
                    {
                        usersQuery = usersQuery.OrderBy(request.DatatableRequest.OrderBy);
                    }
                    else
                    {
                        usersQuery = usersQuery.OrderBy(request.DatatableRequest.OrderBy).Skip(request.DatatableRequest.Start).Take(request.DatatableRequest.Length);
                    }

                    users.Users.AddRange(await usersQuery.ToListAsync());
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

        public override async Task<UserList> GetUserListForLocalAdmin(UserFilter request, ServerCallContext context)
        {
            var users = new UserList()
            {
                Status = GetOkResult()
            };

            Guid admId = Guid.Empty;

            if (request.HasAdministrationId == false ||
                (!string.IsNullOrWhiteSpace(request.AdministrationId) &&
                Guid.TryParse(request.AdministrationId?.Trim(), out admId) == false))
            {
                users.Status = new ResultStatus
                {
                    Code = ResultCodes.BadRequest,
                    Message = "Идентификатор на администрация трябва да е валиден Guid"
                };
            }
            else
            {
                try
                {
                    Expression<Func<ApplicationUser, bool>> filterUser = x => true;
                    if (request.ActiveUsers != null)
                    {
                        filterUser = x => x.Enable == request.ActiveUsers;
                    }

                    Expression<Func<ApplicationUser, bool>> filterRegisterCode = x => true;
                    if (request.HasRegisterCode)
                    {
                        filterRegisterCode = x => x.UserRoles
                            .Any(r => r.RegisterCode.ToLower() == request.RegisterCode.ToLower());
                    }

                    var searchPrefix = request.DatatableRequest.Filter ?? string.Empty;
                    var usersQuery = userManager.Users
                        .Where(u => Guid.Empty == admId || u.AdministrationId == admId ||
                                    (u.Claims.Any(c => c.ClaimType == CustomClaimType.AvailableAdministration && c.ClaimValue == request.AdministrationId)))
                        .Where(filterUser)
                        .Where(u => u.UserRoles.All(r => r.Role.Name != UserRoles.GlobalAdmin))
                        .Where(filterRegisterCode)
                        .TagWith(nameof(GetUserList))
                        .Select(u => new UserListData
                        {
                            Id = u.Id.ToString(),
                            Email = u.Email,
                            FirstName = u.FirstName,
                            MiddleName = u.MiddleName,
                            LastName = u.LastName,
                            Enabled = u.Enable,
                            RoleName = string.Join(", ", u.UserRoles.Select(r => r.Role.Label + "(" + r.RegisterCode + ")"))
                        });

                    users.UserCount = await usersQuery.CountAsync();

                    if (string.IsNullOrEmpty(searchPrefix) == false)
                    {
                        searchPrefix = $"%{searchPrefix}%";

                        usersQuery = usersQuery.Where(q => EF.Functions.ILike(q.Email, searchPrefix) ||
                                                 EF.Functions.ILike(q.Email, searchPrefix) ||
                                                 EF.Functions.ILike(q.FirstName, searchPrefix) ||
                                                 EF.Functions.ILike(q.MiddleName, searchPrefix) ||
                                                 EF.Functions.ILike(q.LastName, searchPrefix));

                        users.UserCount = await usersQuery.CountAsync();
                    }

                    if (request.DatatableRequest.Length < 0)
                    {
                        usersQuery = usersQuery.OrderBy(request.DatatableRequest.OrderBy);
                    }
                    else
                    {
                        usersQuery = usersQuery.OrderBy(request.DatatableRequest.OrderBy).Skip(request.DatatableRequest.Start).Take(request.DatatableRequest.Length);
                    }

                    users.Users.AddRange(await usersQuery.ToListAsync());
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
                            Position = string.IsNullOrEmpty(appUser.Position) ? "-----" : appUser.Position,
                            PhoneNumber = string.IsNullOrEmpty(appUser.PhoneNumber) ? "-----" : appUser.PhoneNumber,
                            Enabled = appUser.Enable,
                            FirstName = appUser.FirstName,
                            MiddleName = string.IsNullOrEmpty(appUser.MiddleName) ? "-----" : appUser.MiddleName,
                            LastName = appUser.LastName,
                            AdministrationId = appUser.AdministrationId.ToString(),
                            Administration = appUser.Administration,
                            UserName = appUser.UserName,
                            ReceiveEmailOnError = appUser.ReceiveEmailOnError,
                        };

                        var login = await userManager.GetLoginsAsync(appUser);
                        user.User.Pid = login
                            .FirstOrDefault(l => l.LoginProvider == UserManagerConstants.LOGINS_PROVIDER)?.ProviderKey ?? string.Empty;

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
                        user.User.ReceiveEFormNotification = await userManagerService.GetReceiveEFormNotification(request);
                        user.User.ReceiveInstructionResponse = await userManagerService.GetReceiveInstructionResponse(request);
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

        public override async Task<UpsertUserResponse> UpsertUser(UserData request, ServerCallContext context)
        {
            UpsertUserResponse result = new UpsertUserResponse();

            if (request.HasId && Guid.TryParse(request.Id, out Guid userId))
            {
                result.Status = await userManagerService.UpdateUserAsync(request, userId);
            }
            else
            {
                result = await userManagerService.CreateUserAsync(request);
            }

            return result;
        }

        public override async Task<ResultStatus> UserAddRole(UserRoleData request, ServerCallContext context)
        {
            ResultStatus status = GetOkResult();

            Guid userId = Guid.Empty;

            if (Guid.TryParse(request.UserId.Trim(), out userId) == false)
            {
                status = new ResultStatus
                {
                    Code = ResultCodes.BadRequest,
                    Message = "Потребителки идентификатор е задължителен и трябва да е валиден Guid"
                };
            }
            else
            {
                try
                {
                    var appUser = await userManager.FindByIdAsync(userId.ToString());

                    if (appUser == null)
                    {
                        status = new ResultStatus
                        {
                            Code = ResultCodes.NotFound,
                            Message = "Потребителят не е намерен"
                        };
                    }
                    else
                    {
                        Guid roleId;

                        if (Guid.TryParse(request.Role.RoleId, out roleId) == false)
                        {
                            status = new ResultStatus
                            {
                                Code = ResultCodes.NotFound,
                                Message = "Ролята не е намерена"
                            };
                        }
                        else
                        {
                            var role = await roleManager.FindByIdAsync(roleId.ToString());

                            if (role != null && string.IsNullOrEmpty(role.Name) == false)
                            {
                                ResultStatus roleStatus = await userManagerService.CheckUserRole(request.Role, userId);
                                if (roleStatus.Code == ResultCodes.Ok)
                                {
                                    await userManagerService.AssignUserRole(request.Role, userId);
                                }
                                else
                                {
                                    status.Code = ResultCodes.BadRequest;
                                    status.Message = $"Потребителят е вече в роля {role.Label}"
;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Грешка при добавяне на роля в {nameof(UserAddRole)}");

                    status = new ResultStatus
                    {
                        Code = ResultCodes.InternalServerError,
                        Message = "Грешка при добавяне на роля"
                    };
                }
            }

            return status;
        }

        public override async Task<ResultStatus> UserAddRoles(UserRolesData request, ServerCallContext context)
        {
            ResultStatus status = GetOkResult();

            Guid userId = Guid.Empty;

            if (Guid.TryParse(request.UserId.Trim(), out userId) == false)
            {
                status = new ResultStatus
                {
                    Code = ResultCodes.BadRequest,
                    Message = "Потребителски идентификатор е задължителен и трябва да е валиден Guid"
                };
            }
            else
            {
                try
                {
                    var appUser = await userManager.FindByIdAsync(userId.ToString());

                    if (appUser == null)
                    {
                        status = new ResultStatus
                        {
                            Code = ResultCodes.NotFound,
                            Message = "Потребителят не е намерен"
                        };
                    }
                    else
                    {
                        foreach (RoleData roleData in request.Roles)
                        {
                            Guid roleId;

                            if (!Guid.TryParse(roleData.RoleId, out roleId))
                            {
                                return new ResultStatus
                                {
                                    Code = ResultCodes.NotFound,
                                    Message = "Идентификатора на ролят не е валиден Guid"
                                };
                            }

                            var role = await roleManager.FindByIdAsync(roleId.ToString());

                            if (role != null && string.IsNullOrEmpty(role.Name) == false)
                            {
                                ResultStatus roleStatus = await userManagerService.CheckUserRole(roleData, userId);
                                if (roleStatus.Code != ResultCodes.Ok)
                                {
                                    return new ResultStatus
                                    {
                                        Code = ResultCodes.BadRequest,
                                        Message = $"Потребителят е вече в роля {role.Label}"
                                    };
                                }
                            }
                        }

                        await userManagerService.AssignUserRoles(request.Roles, userId);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Грешка при добавяне на роля в {nameof(UserAddRole)}");

                    status = new ResultStatus
                    {
                        Code = ResultCodes.InternalServerError,
                        Message = "Грешка при добавяне на роля"
                    };
                }
            }

            return status;
        }

        public override async Task<ResultStatus> UserRemoveRole(UserRoleData request, ServerCallContext context)
        {
            ResultStatus status = GetOkResult();

            Guid userId = Guid.Empty;

            if (Guid.TryParse(request.UserId.Trim(), out userId) == false)
            {
                status = new ResultStatus
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
                        status = new ResultStatus
                        {
                            Code = ResultCodes.NotFound,
                            Message = "User not found"
                        };
                    }
                    else
                    {
                        Guid roleId;

                        if (Guid.TryParse(request.Role.RoleId, out roleId) == false)
                        {
                            status = new ResultStatus
                            {
                                Code = ResultCodes.NotFound,
                                Message = "Role not found"
                            };
                        }
                        else
                        {
                            var role = await roleManager.FindByIdAsync(roleId.ToString());

                            if (role != null && string.IsNullOrEmpty(role.Name) == false)
                            {
                                //if (await userManager.IsInRoleAsync(appUser, role.Name))
                                {
                                    var result = await userManagerService.UnassignUserRole(request.Role, userId);

                                    if (result.Code != ResultCodes.Ok)
                                    {
                                        status = new ResultStatus
                                        {
                                            Code = ResultCodes.InternalServerError,
                                            Message = result.Message ?? "Error removing role"
                                        };
                                    }
                                }
                            }


                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error removing role");

                    status = new ResultStatus
                    {
                        Code = ResultCodes.InternalServerError,
                        Message = "Error removing role"
                    };
                }
            }

            return status;
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

        public override async Task<ResultStatus> HasAdministration(HasAdministrationRequest request, ServerCallContext context)
        {
            var reply = CommonGrpcHelper.CreateStatusOK();

            try
            {
                if ((Guid.TryParse(request.AdministrationId, out Guid administrationId) &&
                    Guid.TryParse(request.UserId, out Guid userId)) == false)
                {
                    throw new ArgumentException("AdministrationId and UserId are required and must be a valid Guid");
                }

                bool hasAdministration = await userManagerService.HasAdministration(administrationId, userId);

                if (!hasAdministration)
                {
                    reply = new ResultStatus
                    {
                        Code = ResultCodes.NotFound,
                        Message = "Administration not found."
                    };
                }
                else
                {
                    await userManagerService.SetAdministration(administrationId, userId, request.AdministrationName);
                }
            }
            catch (ArgumentException aex)
            {
                reply = CommonGrpcHelper.CreateStatusBadRequest(aex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "RegisterService/GetAdministrationsByIds");
                reply = CommonGrpcHelper.CreateStatusInternalServerError(ex);
            }

            return reply;
        }

        public override async Task<UserAvaliableAdministrationsId> GetUserAvailableAdministrationsId(UserAvaliableAdministrationsIdRequest request, ServerCallContext context)
        {
            UserAvaliableAdministrationsId reply = new UserAvaliableAdministrationsId
            {
                Status = CommonGrpcHelper.CreateStatusOK()
            };

            try
            {
                if (Guid.TryParse(request.UserId, out Guid userId) == false)
                {
                    throw new ArgumentException("UserId are required and must be a valid Guid");
                }

                var useClaimsAvailableAdministrations = await userManagerService.GetUserClaimsAvailableAdministrations(userId);
                reply.UserClaimsData.AddRange(useClaimsAvailableAdministrations.Select(cl => new UserClaimsData
                {
                    Id = cl.Id,
                    ClaimValue = cl.ClaimValue
                }));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "RegisterService/GetUserAvailableAdministrationsId");
                reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
            }
            return reply;
        }

        public override async Task<ResultStatus> AddUserAdministration(UserClaimsDataRequest request, ServerCallContext context)
        {
            var reply = CommonGrpcHelper.CreateStatusOK();
            Guid userId = Guid.Empty;

            if (Guid.TryParse(request.UserId.Trim(), out userId) == false)
            {
                reply = new ResultStatus
                {
                    Code = ResultCodes.BadRequest,
                    Message = "Потребителски идентификатор е задължителен и трябва да е валиден Guid"
                };
            }
            else
            {
                try
                {
                    reply = await userManagerService.AddUserClaims(request.UserClaimsData);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "RegisterService/AddUserAdministration");
                    reply = CommonGrpcHelper.CreateStatusInternalServerError(ex);
                }
            }
            return reply;
        }

        public override async Task<ResultStatus> UserRemoveAdministration(UserClaimsData request, ServerCallContext context)
        {
            var reply = CommonGrpcHelper.CreateStatusOK();
            Guid userId = Guid.Empty;

            if (Guid.TryParse(request.UserId.Trim(), out userId) == false)
            {
                reply = new ResultStatus
                {
                    Code = ResultCodes.BadRequest,
                    Message = "Потребителски идентификатор е задължителен и трябва да е валиден Guid"
                };
            }
            else
            {
                try
                {
                    reply = await userManagerService.RemoveUserClaims(request);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "RegisterService/UserRemoveAdministration");
                    reply = CommonGrpcHelper.CreateStatusInternalServerError(ex);
                }
            }
            return reply;
        }

        public override async Task<UsersInfoDashboard> GetUsersDashboard(GetUsersRequest request, ServerCallContext context)
        {
            var reply = new UsersInfoDashboard
            {
                Status = CommonGrpcHelper.CreateStatusOK()
            };

            Guid administrationId = Guid.Empty;
            if (!string.IsNullOrEmpty(request.AdministrationId))
            {
                if (Guid.TryParse(request.AdministrationId, out administrationId) == false)
                {
                    reply.Status = new ResultStatus
                    {
                        Code = ResultCodes.BadRequest,
                        Message = "Идентификатор на администрация е задължителен и трябва да е валиден Guid"
                    };
                    return reply;
                }
            }

            try
            {
                Expression<Func<ApplicationUser, bool>> filter = user =>
                    (administrationId == Guid.Empty || user.AdministrationId == administrationId)
                    && user.UserRoles.All(r => r.Role.Name != UserRoles.GlobalAdmin);

                if (!string.IsNullOrEmpty(request.RegisterCode))
                {
                    var regCode = request.RegisterCode.ToLower();
                    filter = user =>
                        (administrationId == Guid.Empty || user.AdministrationId == administrationId)
                        && user.UserRoles.All(r => r.Role.Name != UserRoles.GlobalAdmin)
                        && user.UserRoles.Any(r => r.RegisterCode.ToLower() == regCode);
                }
                reply.UsersCount = await userManager.Users.CountAsync(filter);
                reply.EnableUsersCount = await userManager.Users.Where(filter).CountAsync(u => u.Enable == true);
                reply.DisableUsersCount = await userManager.Users.Where(filter).CountAsync(u => u.Enable == false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "UserService/GetUsersDashboard");
                reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
            }



            return reply;
        }

        public override async Task<UserClaims> GetClaimsByUserId(UserAvaliableAdministrationsIdRequest request,
            ServerCallContext context)
        {
            var reply = new UserClaims
            {
                Status = CommonGrpcHelper.CreateStatusOK()
            };

            Guid userId = Guid.Empty;
            if (!string.IsNullOrEmpty(request.UserId))
            {
                if (Guid.TryParse(request.UserId, out userId) == false)
                {
                    reply.Status = new ResultStatus
                    {
                        Code = ResultCodes.BadRequest,
                        Message = "Идентификатор на потребител е задължителен и трябва да е валиден Guid"
                    };
                    return reply;
                }
            }
            try
            {
                var claims =  await userManagerService.GetUserClaims(userId);
                foreach (var claim in claims)
                {
                    reply.UserClaimsData.Add(new UserClaimsData()
                    {
                        Id = claim.Id,
                        UserId = claim.UserId.ToString(),
                        ClaimValue = claim.ClaimValue,
                        ClaimType = claim.ClaimType
                    });
                }

                return reply;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "UserService/GetClaimsByUserId");
                reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
            }

            return reply;
        }

        public override async Task<UserNamesByGuidsResponse> GetUserNamesByGuids(UserGuidsRequest request, ServerCallContext context)
        {
            var reply = new UserNamesByGuidsResponse
            {
                Status = CommonGrpcHelper.CreateStatusOK()
            };

            if (request == null || request.UserGuids == null || !request.UserGuids.Any())
            {
                reply.Status = CommonGrpcHelper.CreateStatusBadRequest("Невалидна заявка: Не се подават guid-ове на потребители");
                return reply;
            }

            try
            {
                var parsedUserGuids = request.UserGuids
                 .Select(guidString => Guid.TryParse(guidString, out var guid) ? guid : (Guid?)null)
                 .Where(guid => guid.HasValue)
                 .Select(guid => guid.Value)
                 .ToList();

                if (!parsedUserGuids.Any())
                {
                    reply.Status = CommonGrpcHelper.CreateStatusBadRequest("Няма валидни потребителски guid-ове");
                    return reply;
                }

                var users = await userManager.Users
                .Where(u => parsedUserGuids.Contains(u.Id)) // Assumes Id is string
                .Select(u => new
                {
                    Guid = u.Id, // Convert Id to Guid
                    FirstName = u.FirstName,
                    MiddleName = u.MiddleName,
                    LastName = u.LastName
                })
                .ToListAsync();

                // Populate the response
                foreach (var user in users)
                {
                    reply.UserNamesByGuid.Add(new UserNamesByGuid
                    {
                        Guid = user.Guid.ToString(), // Convert Guid back to string
                        FirstName = user.FirstName ?? string.Empty,
                        MiddleName = user.MiddleName ?? string.Empty,
                        LastName = user.LastName ?? string.Empty
                    });
                }

                if (!users.Any())
                {
                    reply.Status = CommonGrpcHelper.CreateStatusBadRequest("Няма намерени потребители с такива guid-ове");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Грешка по време на извличане на имената на потребителите");
                reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex, "Грешка по време на извличане на имената на потребителите");
            }
            return reply;
        }

        public override async Task<UserGuidsResponse> GetUserGuidsByName(UserNameSearchRequest request, ServerCallContext context)
        {
            var reply = new UserGuidsResponse 
            { 
                Status = CommonGrpcHelper.CreateStatusOK() 
            };

            try
            {
                var searchTerm = request.SearchTerm?.ToLower() ?? "";
                var searchWords = searchTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                var users = await userManager.Users
                    .Where(u => searchWords.All(word =>
                        (u.FirstName != null && u.FirstName.ToLower().Contains(word)) ||
                        (u.MiddleName != null && u.MiddleName.ToLower().Contains(word)) ||
                        (u.LastName != null && u.LastName.ToLower().Contains(word))
                    ))
                    .Select(u => u.Id.ToString())
                    .ToListAsync();
                reply.UserGuids.AddRange(users);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка по време на търсене на потребител с име: {request.SearchTerm}", request.SearchTerm);
                reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex,"Грешка по време на търсене на потребител");
            }
            return reply;
        }

        public override async Task<UserCountResponse> GetCurrentRegisterUsersCount(UserCountRequest request, ServerCallContext context)
        {
            var reply = new UserCountResponse
            {
                Status = CommonGrpcHelper.CreateStatusOK()
            };

            try
            {
                if (string.IsNullOrEmpty(request.RegisterCode))
                {
                    reply.Status = CommonGrpcHelper.CreateStatusBadRequest("Липсва код на регистър");
                    return reply;
                }

                DateTime? dateFrom = request.DateFrom?.ToDateTime();
                DateTime? dateTo = request.DateTo?.ToDateTime();
              
                List<ApplicationUser> users = await userManager.Users.Where(u => u.UserRoles.Any(r => r.RegisterCode == request.RegisterCode)).ToListAsync();           

                var activeUsers = users
                    .Where(au => au.Enable == true
                        && (!dateFrom.HasValue || au.CreatedAt >= dateFrom.Value)
                        && (!dateTo.HasValue || au.CreatedAt <= dateTo.Value))
                    .Count();

                var inactiveUsers = users
                    .Where(au => au.Enable == false
                        && (!dateFrom.HasValue || au.CreatedAt >= dateFrom.Value)
                        && (!dateTo.HasValue || au.CreatedAt <= dateTo.Value))
                    .Count();

                var createdUsers = users
                    .Where(au => (!dateFrom.HasValue || au.CreatedAt >= dateFrom.Value)
                        && (!dateTo.HasValue || au.CreatedAt <= dateTo.Value))
                    .Count();

                reply.ActiveUsers = activeUsers;
                reply.CreatedUsers = createdUsers;
                reply.InactiveUsers = inactiveUsers;

                return reply;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка по време на извличане на брой потребители за код на регистър : {request.RegisterCode}");
                reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
                return reply;
            }
        }

        public override async Task<AppRoles> GetAllRoles(GetAllRolesRequest request, ServerCallContext context)
        {
            var roles = new AppRoles()
            {
                Status = GetOkResult()
            };

            try
            {
                var appRoles = await roleManager.Roles.Where(r=>r.Label.Contains(request.Search))
                    .Select(r => new RoleData
                    {
                        Label = r.Label,
                        RoleId = r.Id.ToString(),
                        Name = r.Name ?? string.Empty
                    }).ToListAsync();

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

        public override async Task<ResultStatus> UpsertRole(RoleData request, ServerCallContext context)
        {
            if (Guid.TryParse(request.RoleId, out Guid roleId))
            {
                return await userManagerService.UpdateRoleAsync(request, roleId);
            }

            return await userManagerService.CreateRoleAsync(request);
        }

        public override async Task<ResultStatus> DeleteRole(RoleData request, ServerCallContext context)
        {
            Guid roleId = Guid.Empty;

            if (string.IsNullOrEmpty(request.RoleId?.Trim()) ||
                Guid.TryParse(request.RoleId?.Trim(), out roleId) == false)
            {
                return new ResultStatus
                {
                    Code = ResultCodes.BadRequest,
                    Message = "UserId is required and must be a valid Guid"
                };
            }
            return await userManagerService.DeleteRoleAsync(roleId);
        }

        private ResultStatus GetOkResult()
        {
            return new ResultStatus
            {
                Code = ResultCodes.Ok
            };
        }

        public override async Task<UserReceiveEmailsResponse> UserReceiveEmails(UserReceiveEmailsRequest request, ServerCallContext context)
        {
            var result =  new UserReceiveEmailsResponse
            {
                Status = GetOkResult()
            };

            try
            {
                var users = await userManagerService.GetUserReceiveEmails(request, true, false);
                result.UserData.AddRange(users);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting roles");

                result.Status = new ResultStatus
                {
                    Code = ResultCodes.InternalServerError,
                    Message = "Error getting roles"
                };
            }
            return result;
        }

        public override async Task<UserReceiveEmailsResponse> UserReceiveEmailsInstructionResponse(UserReceiveEmailsRequest request, ServerCallContext context)
        {
            var result = new UserReceiveEmailsResponse
            {
                Status = GetOkResult()
            };

            try
            {
                var users = await userManagerService.GetUserReceiveEmails(request, false, true);
                result.UserData.AddRange(users);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting roles");

                result.Status = new ResultStatus
                {
                    Code = ResultCodes.InternalServerError,
                    Message = "Error getting roles"
                };
            }
            return result;
        }

        public override async Task<UserReceiveEmailsResponse> UserReceiveEmailsForSrok(UserReceiveEmailsRequest request, ServerCallContext context)
        {
            var result = new UserReceiveEmailsResponse
            {
                Status = GetOkResult()
            };

            try
            {
                var users = await userManagerService.GetUserReceiveEmailsForSrok(request);
                result.UserData.AddRange(users);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting roles");

                result.Status = new ResultStatus
                {
                    Code = ResultCodes.InternalServerError,
                    Message = "Error getting roles"
                };
            }
            return result;
        }
    }
}
