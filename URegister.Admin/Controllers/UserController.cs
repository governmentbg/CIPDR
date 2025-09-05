using DataTables.AspNet.Core;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using URegister.Common;
using URegister.Core.Contracts;
using URegister.Core.Models.Register;
using URegister.Core.Models.User;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Extensions;
using URegister.RegistersCatalog;
using URegister.Users;

namespace URegister.Admin.Controllers
{
    [Display(Name = "Потребители")]
    public class UserController(
        AppUserManager.AppUserManagerClient appUserManagerClient,
        ILogger<UserController> logger,
        IRegisterClientService registerClient) : BaseController
    {

        [HttpGet]
        [Display(Name = "Зареждане на списък с потребители по администрация")]
        public async Task<IActionResult> Index(string administrationId)
        {
            AppAdministrations administrations = await registerClient.GetAllAdministrations();

            var administrationList = administrations.Administrations.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = t.Name
            }).ToList();

            if (string.IsNullOrEmpty(administrationId) && administrationList.Any())
            {
                administrationId = administrationList.First().Value;
            }

            foreach (var item in administrationList)
            {
                item.Selected = item.Value == administrationId;
            }

            var model = new AdministrationViewModel
            {
                SelectedAdministrationId = administrationId,
                Administrations = administrationList
            };

            return View(model);
        }

        /// <summary>
        /// Взема всички потребители в дадена администрация за показване в таблица
        /// </summary>
        /// <param name="request"></param>
        /// <param name="administrationId"></param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Извличане на списък с потребители в администрация")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetUsers(IDataTablesRequest request, string administrationId)
        {
            try
            {
                var protoRequest = request!.GetDataTablesRequestProto();
                UserFilter filter = new UserFilter
                {
                    AdministrationId = administrationId ?? string.Empty,
                    Page = request.Start,
                    PageSize = request.Length,
                    DatatableRequest = protoRequest
                };

                UserList users = await appUserManagerClient.GetUserListAsync(filter);
                foreach (var user in users.Users)
                {
                    List<string> userRoles = await registerClient.FormatUserRoles(user);

                    if (userRoles.Count > 0)
                    {
                        user.RoleName = string.Join(", ", userRoles);
                    }
                }
                return request.GetResponseServerPaging(users.Users, users.UserCount);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при зареждане на потребители в {nameof(GetUsers)}");
                return BadRequest("Грешка при зареждане на потребители.");
            }
        }


        /// <summary>
        /// Подробни данни за потребител
        /// </summary>
        /// <param name="userId">Идентификатор на потебител</param>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Зареждане на подробни данни за потребител")]
        public async Task<IActionResult> UserDetails(Guid userId)
        {
            UserViewModel model = new UserViewModel();
            try
            {
                UserFilter filter = new UserFilter
                {
                    Id = userId.ToString()
                };

                var response = await appUserManagerClient.GetUserByIdAsync(filter);
                model.Id = response.User.Id;
                model.FirstName = response.User.FirstName;
                model.MiddleName = response.User.MiddleName;
                model.LastName = response.User.LastName;
                model.Email = response.User.Email;
                model.PhoneNumber = response.User.PhoneNumber;
                model.Position = response.User.Position;
                model.Pid = response.User.Pid;
                model.Enabled = response.User.Enabled;
                model.Username = response.User.UserName;
                model.AdministrationId = response.User.AdministrationId;
                model.AdministrationName = response.User.Administration;

                if (response.User.Roles.Any(r => r.Label.Equals("Администратор МЕУ")))
                {
                    model.IsGlobalAdmin = true;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при зареждане на детайли за потребител в {nameof(UserDetails)}");
                SetErrorMessage("Грешка при зареждане на детайли за потребител.");
            }
            return View(model);
        }

        /// <summary>
        /// Форма за добавяне на потребител
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Зареждане на форма за добавяне на потребител")]
        public async Task<IActionResult> Add(string? administrationId = null)
        {
            var model = new UserViewModel();
            try
            {
                GetAdministrationResponse administration = new GetAdministrationResponse();
                if (string.IsNullOrEmpty(administrationId))
                {
                    administration = await registerClient.GetAdminAdministration();
                }
                else
                {
                    administration = await registerClient.GetAdministrationById(administrationId);
                }

                if (administration.Status.Code != ResultCodes.Ok)
                {
                    SetErrorMessage("Грешка при взимане на администрация.");
                    return View(model);
                }
                administrationId = administration.Data.Id;
                model.AdministrationId = administrationId;
                model.AdministrationName = administration.Data.Name;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при взимане на администрация в {nameof(Add)}");
                SetErrorMessage("Грешка при взимане на администрация.");
            }
            return View(model);
        }

        /// <summary>
        /// Добавяне на потребител
        /// </summary>
        /// <param name="model">Модел на потребител</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Добавяне на нов потребител")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.PhoneNumber = model.PhoneNumber.Trim();
            if (model.PhoneNumber.StartsWith('0'))
            {
                model.PhoneNumber = "+359" + model.PhoneNumber.Remove(0, 1);
            }

            try
            {
                var user = new UserData
                {
                    AdministrationId = model.AdministrationId,
                    Administration = model.AdministrationName,
                    FirstName = model.FirstName,
                    MiddleName = model.MiddleName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Position = model.Position,
                    Pid = model.Pid,
                    UserName = model.Pid.Substring(0, 4) + model.LastName
                };
                var response = await appUserManagerClient.UpsertUserAsync(user);
                if (response.Status.Code == ResultCodes.Ok)
                {
                    SetSuccessMessage($"Успешно добавихте потребител {model.Username}");
                    return RedirectToAction(nameof(UserDetails), new { userId = response.UserId });
                }
                if (response.Status.Code == ResultCodes.NotSet)
                {
                    SetErrorMessage(response.Status.Message);
                    return View(model);
                }
                SetErrorMessage($"Грешка при добавяне на потребител. Статус код: {response.Status.Code}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при добавяне на потребител в {nameof(Add)}");
                SetErrorMessage("Грешка при добавяне на потребител.");
            }
            return View(model);
        }

        /// <summary>
        /// Редакция на потребител
        /// </summary>
        /// <param name="model">Модел на потребител</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Редактиране на потребител")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(nameof(UserDetails), model);
            }

            model.PhoneNumber = model.PhoneNumber.Trim();
            if (model.PhoneNumber.StartsWith('0'))
            {
                model.PhoneNumber = "+359" + model.PhoneNumber.Remove(0, 1);
            }

            try
            {
                UserData userData = new UserData
                {
                    Id = model.Id,
                    UserName = model.Pid.Substring(0, 4) + model.LastName,
                    FirstName = model.FirstName,
                    MiddleName = model.MiddleName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Position = model.Position,
                    PhoneNumber = model.PhoneNumber,
                    Pid = model.Pid,
                    Enabled = model.Enabled,
                    AdministrationId = model.AdministrationId,
                    Administration = model.AdministrationName
                };

                var userRoleResponse = await appUserManagerClient.GetUserByIdAsync(new UserFilter { Id = model.Id });
                if (userRoleResponse.Status.Code != ResultCodes.Ok)
                {
                    SetErrorMessage($"Грешка при взимане на роли на потребител {model.Username}");
                    return View(nameof(UserDetails), model);
                }
                userData.Roles.AddRange(userRoleResponse.User.Roles);

                var response = await appUserManagerClient.UpsertUserAsync(userData);
                if (response.Status.Code == ResultCodes.Ok)
                {
                    SetSuccessMessage($"Успешно редактиране на потребител {model.Username}");
                    return RedirectToAction("Index", new { administrationId = model.AdministrationId });
                }
                SetErrorMessage($"Грешка при редактиране на потребител. Статус код: {response.Status.Code}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при редактиране на потребител в {nameof(Edit)}");
                SetErrorMessage("Грешка при редактиране на потребител.");
            }
            return View(nameof(UserDetails), model);
        }

        /// <summary>
        /// Роли на потребител
        /// </summary>
        /// <param name="request">Заявка с информация</param>
        /// <param name="userId">Идентификатор на потребител</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Извличане на роли на потребител")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetUserRoles(IDataTablesRequest request, string userId)
        {
            try
            {
                UserFilter filter = new UserFilter
                {
                    Id = userId
                };
                var reponse = await appUserManagerClient.GetUserByIdAsync(filter);
                var userRoles = reponse.User.Roles;
                var protoRequest = request!.GetDataTablesRequestProto();
                var userQueryable = userRoles.OrderByDescending(x => x.Label).AsQueryable();

                var filteredQuery = userQueryable.SearchFor(protoRequest.SearchColumn, protoRequest.Filter);

                var pagedQuery = request.Length < 0
                    ? filteredQuery.OrderBy(protoRequest.OrderBy)
                    : filteredQuery.OrderBy(protoRequest.OrderBy).Skip(request.Start).Take(request.Length);

                var pagedData = pagedQuery.ToList();

                var response = await registerClient.GetRegisterFullList();

                if (response.Status.Code != ResultCodes.Ok)
                {
                    logger.LogError($"Грешка при извличане на списък регистри от {nameof(registerClient.GetRegisterFullList)} в {nameof(GetUserRoles)}");
                    return BadRequest("Грешка при извличане на данни за роли.");
                }

                var result = pagedData.Select(item => new
                {
                    item.Label,
                    item.RegisterCode,
                    item.HasRegisterCode,
                    item.RoleId,
                    RegisterName = response.Data.Where(d => d.Code == item.RegisterCode).FirstOrDefault()?.Name
                }).ToList();

                return request.GetResponseServerPaging(result, userRoles.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при редактиране на роли на потребител в {nameof(GetUserRoles)}");
                SetErrorMessage("Грешка при редактиране на роли на потребител.");
            }
            return BadRequest("Грешка при извличане на данни за роли.");
        }

        /// <summary>
        /// Всички роли
        /// </summary>
        /// <returns></returns>
        [Display(Name = "Извличане на всички налични роли")]
        public async Task<IActionResult> GetAllRoles(string administrationId, string userId)
        {
            try
            {
                AppRoles roles = await appUserManagerClient.GetRolesAsync(new Empty());

                var userClaimResponse = await appUserManagerClient.GetClaimsByUserIdAsync(new UserAvaliableAdministrationsIdRequest(){UserId = userId});
                if (userClaimResponse.Status.Code != ResultCodes.Ok)
                {
                    logger.LogError($"Грешка при извличане на данни за роли на потребител {nameof(appUserManagerClient.GetClaimsByUserIdAsync)} в {nameof(GetAllRoles)}");
                    return BadRequest("Грешка при извличане на данни за роли на потребител.");
                }

                List<RegisterVM> registries = new List<RegisterVM>();
                HashSet<int> existingRegistryIds = new HashSet<int>();

                foreach (var claims in userClaimResponse.UserClaimsData)
                {
                    if (claims.ClaimType == CustomClaimType.AvailableAdministration)
                    {
                        var registriesByAdministration = await registerClient.GetAllRegisterInAdministration(claims.ClaimValue);

                        foreach (var reg in registriesByAdministration)
                        {
                            if (existingRegistryIds.Add(reg.Id))
                            {
                                registries.Add(reg);
                            }
                        }
                    }
                }

                RolesRegistriesVM result = new RolesRegistriesVM();
                result.Roles = roles.Roles.Where(r => !r.Name.Equals(UserRoles.GlobalAdmin)).ToList();
                result.Registries = registries;
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при редактиране на роли на потребител в {nameof(GetUserRoles)}");
                SetErrorMessage("Грешка при редактиране на роли на потребител.");
            }
            return BadRequest(string.Empty);
        }

        /// <summary>
        /// Промяна на ролите на потребител
        /// </summary>
        /// <param name="userId">Идентификатор на потребител</param>
        /// <param name="roleIds">Идентификатор на роля</param>
        /// <param name="registerCode">Код на регистър</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Актуализиране на роли на потребител")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserRoles(
            string userId,
            string[] roleIds,
            string registerCode)
        {
            try
            {
                if (string.IsNullOrEmpty(registerCode))
                {
                    return BadRequest(new { message = "Изберете регистър." });
                }

                if (!roleIds.Any())
                {
                    return BadRequest(new { message = "Изберете роля." });
                }

                UserRolesData userRoleData = new UserRolesData()
                {
                    UserId = userId,
                };

                foreach (string roleId in roleIds)
                {
                    userRoleData.Roles.Add(new RoleData
                    {
                        RegisterCode = registerCode,
                        RoleId = roleId
                    });
                }

                ResultStatus result = await appUserManagerClient.UserAddRolesAsync(userRoleData);
                if (result.Code == ResultCodes.Ok)
                {
                    return Ok(new { message = "Успешно добавяне на роля" });
                }
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при редактиране на роли на потребител в {nameof(GetUserRoles)}");
                return BadRequest("Грешка при редактиране на роли на потребител.");
            }
        }

        /// <summary>
        /// Отписване от роля на потребител.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Премахване на роля от потребител")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnassignRole(UserRoleUpdateRequest request)
        {
            try
            {
                UserRoleData userRoleData = new UserRoleData
                {
                    UserId = request.UserId,
                    Role = new RoleData
                    {
                        RoleId = request.RoleId,
                        RegisterCode = request.RegisterCode
                    }
                };

                ResultStatus result = await appUserManagerClient.UserRemoveRoleAsync(userRoleData);
                if (result.Code == ResultCodes.Ok)
                {
                    return Ok(new { message = "Успешно премахване на роля" });
                }
                return BadRequest(new { message = "Грешка при премахване на роля" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при редактиране на роли на потребител в {nameof(UnassignRole)}");
                return BadRequest("Грешка при редактиране на роли на потребител.");
            }
        }

        /// <summary>
        /// Добавяне на роля GLobalAdmin на потребител
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Добавяне на роля 'Администратор МЕУ' на потребител")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignGlobalAdminRole(string userId)
        {
            try
            {
                UserRolesData userRoleData = new UserRolesData()
                {
                    UserId = userId,
                };
                AppRoles roles = await appUserManagerClient.GetRolesAsync(new Empty());
                var adminRole = roles.Roles.First(r => r.Name.Equals(UserRoles.GlobalAdmin));
                userRoleData.Roles.Add(new RoleData
                {
                    RegisterCode = "R00000",
                    RoleId = adminRole.RoleId
                });

                ResultStatus result = await appUserManagerClient.UserAddRolesAsync(userRoleData);
                if (result.Code == ResultCodes.Ok)
                {
                    return Ok(new { message = "Успешно добавяне на роля" });
                }
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при добавяне на администратор роля на потребител в {nameof(AssignGlobalAdminRole)}");
                return BadRequest("Грешка при добавяне на администратор роля на потребител");
            }
        }

        /// <summary>
        /// Отписване от роля GlobalAdmin на потребител
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Премахване на роля 'Администратор МЕУ' от потребител")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnassignGlobalAdminRole(string userId)
        {
            try
            {
                AppRoles roles = await appUserManagerClient.GetRolesAsync(new Empty());
                var adminRole = roles.Roles.First(r => r.Name.Equals(UserRoles.GlobalAdmin));
                UserRoleData userRoleData = new UserRoleData
                {
                    UserId = userId,
                    Role = new RoleData
                    {
                        RoleId = adminRole.RoleId,
                        RegisterCode = "R00000"
                    }
                };

                ResultStatus result = await appUserManagerClient.UserRemoveRoleAsync(userRoleData);
                if (result.Code == ResultCodes.Ok)
                {
                    return Ok(new { message = "Успешно премахване на роля" });
                }
                return BadRequest(new { message = "Грешка при премахване на роля" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при редактиране на роли на потребител в {nameof(UnassignGlobalAdminRole)}");
                return BadRequest("Грешка при редактиране на роли на потребител.");
            }
        }

        /// <summary>
        /// Връща всички администрации на потребител.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [Display(Name = "Извличане на администрации на потребител")]
        public async Task<IActionResult> GetUserAdministrations(IDataTablesRequest request, string userId)
        {
            try
            {
                var administrations = await registerClient.GetAllAdministrations();

                var userClaims = await appUserManagerClient.GetUserAvailableAdministrationsIdAsync(new UserAvaliableAdministrationsIdRequest
                {
                    UserId = userId
                });

                var userAdministrationIds = userClaims.UserClaimsData.Select(cl => cl.ClaimValue).ToList();

                var userAdministrations = administrations.Administrations
                    .Where(admin => userAdministrationIds.Contains(admin.Id) && admin.Uic != "000000000").AsQueryable();

                return request.GetResponse(userAdministrations, fromDatabase: false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при взимане на администрации на потребител в {nameof(GetUserAdministrations)}");
                return BadRequest("Грешка при взимане на администрации на потребител.");
            }
        }

        /// <summary>
        /// Връща админидтрации които са възможни за добавяне.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [Display(Name = "Извличане на администрации за присвояване на потребител")]
        public async Task<IActionResult> GetAdministrationsForAssign(string userId)
        {
            try
            {
                var administrations = await registerClient.GetAllAdministrations();

                var userClaims = await appUserManagerClient.GetUserAvailableAdministrationsIdAsync(
                    new UserAvaliableAdministrationsIdRequest
                    {
                        UserId = userId
                    });

                var userAdministrationIds = userClaims.UserClaimsData.Select(cl => cl.ClaimValue).ToList();

                var userAdministrations = administrations.Administrations
                    .Where(admin => !userAdministrationIds.Contains(admin.Id)).AsQueryable();

                return Ok(userAdministrations);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при взимане на администрации за добавяне към потребител в {nameof(GetAdministrationsForAssign)}");
                return BadRequest("Грешка при взимане на администрации за добавяне към потребител.");
            }
        }

        /// <summary>
        /// Добавяне на администрация на потребител
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="administrationIds"></param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Добавяне на администрации към потребител")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUserAdministrations(
            string userId,
            string[] administrationIds)
        {
            try
            {
                if (!administrationIds.Any())
                {
                    return BadRequest(new { message = "Изберете администрация." });
                }

                UserClaimsDataRequest request = new UserClaimsDataRequest()
                {
                    UserId = userId
                };

                foreach (string administrationId in administrationIds)
                {
                    request.UserClaimsData.Add(new UserClaimsData()
                    {
                        UserId = userId,
                        ClaimType = CustomClaimType.AvailableAdministration,
                        ClaimValue = administrationId
                    });
                }

                ResultStatus result = await appUserManagerClient.AddUserAdministrationAsync(request);
                if (result.Code == ResultCodes.Ok)
                {
                    return Ok(new { message = "Успешно добавяне на администрация" });
                }
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при добавяне на админисрация на потребител в {nameof(GetUserRoles)}");
                return BadRequest("Грешка при добавяне на админисрация на потребител.");
            }
        }

        [HttpPost]
        [Display(Name = "Премахване на администрация от потребител")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAdministration(string userId, string administrationId, string administrationName)
        {
            try
            {
                UserClaimsData userAdministrationData = new UserClaimsData
                {
                    UserId = userId,
                    ClaimValue = administrationId,
                    ClaimType = CustomClaimType.AvailableAdministration
                };

                ResultStatus result = await appUserManagerClient.UserRemoveAdministrationAsync(userAdministrationData);
                if (result.Code == ResultCodes.Ok)
                {
                    return Ok(new { message = $"Успешно премахване на администрация '{administrationName}'" });
                }
                return BadRequest(new { message = $"Грешка при премахване на администрация '{administrationName}'" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при премахване на администрация на потребител в {nameof(RemoveAdministration)}");
                return BadRequest("Грешка при премахване на администрация на потребител.");
            }
        }

        [HttpGet]
        [Display(Name = "Управляване на роли в системата")]
        public IActionResult Roles()
        {
            return View();
        }

        [HttpPost]
        [Display(Name = "Извличане на всички роли")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetAllRolesForDataTable(IDataTablesRequest request)
        {
            try
            {
                var searchPrefix = request.Search.Value ?? string.Empty;
                AppRoles roles = await appUserManagerClient.GetAllRolesAsync(new GetAllRolesRequest
                {
                    Search = searchPrefix
                });
               
                var protoRequest = request!.GetDataTablesRequestProto();
                var roleQueryable = roles.Roles.OrderBy(x => x.Name).AsQueryable();
                
                var pagedQuery = request.Length < 0
                    ? roleQueryable.OrderBy(protoRequest.OrderBy)
                    : roleQueryable.OrderBy(protoRequest.OrderBy).Skip(request.Start).Take(request.Length);

                var pagedData = pagedQuery.ToList();

                var result = pagedData.Select(item => new
                {
                    item.RoleId,
                    item.Label,
                    item.Name
                }).ToList();

                return request.GetResponseServerPaging(result, roles.Roles.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при редактиране на роли на потребител в {nameof(GetUserRoles)}");
                SetErrorMessage("Грешка при редактиране на роли на потребител.");
            }
            return BadRequest("Грешка при извличане на данни за роли.");
        }

        [HttpPost]
        [Display(Name = "Добавяне на роля в системата")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRole(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
            {
                return BadRequest("Името на роля е задължително.");
            }

            RoleData role = new RoleData()
            {
                Label = roleName
            };

            var result = await appUserManagerClient.UpsertRoleAsync(role);
            if (result.Code == ResultCodes.Ok)
            {
                return Ok($"Успешно създадохте роля {roleName} в системата.");
            }
            return BadRequest(result.Message);
        }

        [HttpPost]
        [Display(Name = "Редактиране на роля в системата")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole(string roleId, string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
            {
                return BadRequest("Името на роля е задължително.");
            }

            RoleData role = new RoleData()
            {
                RoleId = roleId,
                Label = roleName
            };

            var result = await appUserManagerClient.UpsertRoleAsync(role);
            if (result.Code == ResultCodes.Ok)
            {
                return Ok($"Успешно създадохте роля {roleName} в системата.");
            }
            return BadRequest(result.Message);
        }

        [HttpPost]
        [Display(Name = "Изтриване на роля в системата")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRole(string roleId, string roleName)
        {
            if (string.IsNullOrEmpty(roleId))
            {
                return BadRequest("Липсва идентификатор на ролята");
            }

            RoleData role = new RoleData()
            {
                RoleId = roleId,
                Name = roleName
            };

            var result = await appUserManagerClient.DeleteRoleAsync(role);
            if (result.Code == ResultCodes.Ok)
            {
                return Ok($"Успешно изтрихте роля {roleName} от системата.");
            }
            return BadRequest(result.Message);
        }
    }
}
