using DataTables.AspNet.Core;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using URegister.Common;
using URegister.Core.Contracts;
using URegister.Core.Models.User;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Extensions;
using URegister.RegistersCatalog;
using URegister.Users;

namespace URegister.Areas.Admin.Controllers
{
    [Authorize(Roles = $"{UserRoles.Admin}")]
    [Display(Name = "Потребители")]
    public class AdminController(
    AppUserManager.AppUserManagerClient appUserManagerClient,
    ILogger<AdminController> logger,
    IRegisterClientService registerClient,
    IRegisterService registerService) : BaseController
    {

        [HttpGet]
        [Display(Name = "Преглед на списък с потребители")]
        public async Task<IActionResult> Index(string administrationId)
        {
            var currentRegister = await registerService.GetCurrentRegister();
            var model = new AdministrationViewModel
            {
                SelectedAdministrationId = administrationId,
                RegisterCode = currentRegister.Code
            };

            return View(model);
        }

        /// <summary>
        /// Взема всички потребители в дадена администрация за показване в таблица
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Извличане на списък с потребители за показване в таблица")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetUsers(IDataTablesRequest request, string registerCode)
        {
            try
            {
                var protoRequest = request!.GetDataTablesRequestProto();
                UserFilter filter = new UserFilter
                {
                    AdministrationId = User.Claims.FirstOrDefault(c => c.Type == CustomClaimType.AdministrationId)?.Value ?? string.Empty,
                    Page = request.Start,
                    PageSize = request.Length,
                    DatatableRequest = protoRequest,
                    RegisterCode = registerCode
                };

                UserList users = await appUserManagerClient.GetUserListForLocalAdminAsync(filter);
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
        /// Взема всички потребители в дадена администрация за показване в таблица
        /// </summary>
        /// <param name="request"></param>
        /// <param name="activeUsers"></param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Извличане на списък с потребители за показване в контролно табло")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetUsersDashboard(IDataTablesRequest request, bool? activeUsers)
        {
            try
            {
                var protoRequest = request!.GetDataTablesRequestProto();
                UserFilter filter = new UserFilter
                {
                    AdministrationId = User.Claims.FirstOrDefault(c => c.Type == CustomClaimType.AdministrationId)?.Value ?? string.Empty,
                    Page = request.Start,
                    PageSize = request.Length,
                    DatatableRequest = protoRequest
                };
                
                if (activeUsers != null)
                {
                    filter.ActiveUsers = (bool)activeUsers;
                }
               
                UserList users = await appUserManagerClient.GetUserListForLocalAdminAsync(filter);
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
                logger.LogError(ex, $"Грешка при зареждане на потребители в {nameof(GetUsersDashboard)}");
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
                var administrationId = User.Claims.FirstOrDefault(c => c.Type == CustomClaimType.AdministrationId)?.Value ?? string.Empty;
                var register = await registerService.GetCurrentRegister();
                UserFilter filter = new UserFilter
                {
                    Id = userId.ToString(), 
                    AdministrationId = administrationId,
                    RegisterCode = register.Code,
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
                model.AdministrationId = string.IsNullOrEmpty(administrationId) ? response.User.AdministrationId : administrationId;
                model.AdministrationName = response.User.Administration;
                model.ReceiveEFormNotification = response.User.ReceiveEFormNotification;
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
        [Display(Name = "Зареждане на форма за добавяне на нов потребител")]
        public async Task<IActionResult> Add()
        {
            var model = new UserViewModel();
            try
            {
                var administrationId =
                    User.Claims.FirstOrDefault(c => c.Type == CustomClaimType.AdministrationId)?.Value ?? string.Empty;
                GetAdministrationResponse administration = await registerClient.GetAdministrationById(administrationId);
                
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
        [Display(Name = "Добавяне на нов потребител в системата")]
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
                var register = await registerService.GetCurrentRegister();
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
                    UserName = model.Pid.Substring(0, 4) + model.LastName,
                    ReceiveEFormNotification = model.ReceiveEFormNotification,
                    RegisterCode = register.Code,
                };
                var response = await appUserManagerClient.UpsertUserAsync(user);
                if (response.Status.Code == ResultCodes.Ok)
                {
                    SetSuccessMessage($"Успешно добавихте потребител {model.Username}");
                    return RedirectToAction(nameof(UserDetails), new { userId = response.UserId });
                }
                SetErrorMessage($"Грешка при добавяне на потребител. {response.Status.Message} Статус код: {(int)response.Status.Code}");
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
        [Display(Name = "Редакция на потребител")]
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
                var register = await registerService.GetCurrentRegister();
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
                    Administration = model.AdministrationName,
                    ReceiveEFormNotification = model.ReceiveEFormNotification,
                    RegisterCode = register.Code,
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
        [Display(Name = "Извличане на списък с роли на потребител")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetUserRoles(IDataTablesRequest request, string userId)
        {
            try
            {
                UserFilter filter = new UserFilter
                {
                    Id = userId.ToString()
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
        public async Task<IActionResult> GetAllRoles(string administrationId)
        {
            try
            {
                AppRoles roles = await appUserManagerClient.GetRolesAsync(new Empty());
                RolesRegistriesVM result = new RolesRegistriesVM();
                HashSet<int> existingRegistryIds = new HashSet<int>();
                result.Roles = roles.Roles.Where(r => !r.Name.Equals(UserRoles.GlobalAdmin)).ToList();
                var registriesByAdministration = await registerClient.GetAllRegisterInAdministration(administrationId);
                foreach (var reg in registriesByAdministration)
                {
                    if (existingRegistryIds.Add(reg.Id))
                    {
                        result.Registries.Add(reg);
                    }
                }
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
    }
}