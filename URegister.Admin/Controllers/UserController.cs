using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using URegister.Infrastructure.Contracts;
using URegister.Users;

namespace URegister.Admin.Controllers
{
    public class UserController(
        AppUserManager.AppUserManagerClient appUserManagerClient,
        ILogger<UserController> logger,
        IHttpRequester requester,
        IConfiguration config) : BaseController
    {

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var roles = await appUserManagerClient.GetRolesAsync(new Empty());
            
            return Json(roles);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> GetUsers(IDataTablesRequest request)
        //{
        //    try
        //    {
        //        await AttachTokenToRequest();

        //        var usersUrl = string.Format(KeyCloakConstants.GetUsers, "");
        //        var response = await requester.GetAsync("url" + usersUrl);

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            logger.LogWarning($"Неуспешно извличане на потребители. Status Code: {response.StatusCode}");
        //            return BadRequest("Неуспешно извличане на потребители.");
        //        }

        //        var usersJson = await response.Content.ReadAsStringAsync();
        //        var users = JsonSerializer.Deserialize<List<UserResponse>>(usersJson, new JsonSerializerOptions
        //        {
        //            PropertyNameCaseInsensitive = true
        //        });

        //        var protoRequest = request!.GetDataTablesRequestProto();
        //        var userQueryable = users.OrderByDescending(x => x.CreatedTimestamp).AsQueryable();

        //        var filteredQuery = userQueryable.SearchFor(protoRequest.SearchColumn, protoRequest.Filter);

        //        var pagedQuery = request.Length < 0
        //            ? filteredQuery.OrderBy(protoRequest.OrderBy)
        //            : filteredQuery.OrderBy(protoRequest.OrderBy).Skip(request.Start).Take(request.Length);

        //        var pagedData = pagedQuery.ToList();
        //        return request.GetResponseServerPaging(pagedData, users.Count);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Грешка при зареждане на потребители в {nameof(GetUsers)}");
        //        return BadRequest("Грешка при зареждане на потребители.");
        //    }
        //}

        ///// <summary>
        ///// Подробни данни за потребител
        ///// </summary>
        ///// <param name="userId">Идентификатор на потебител</param>
        ///// <returns></returns>
        //[HttpGet]
        //public async Task<IActionResult> UserDetails(Guid userId)
        //{
        //    UserViewModel model = new UserViewModel();
        //    try
        //    {
        //        await AttachTokenToRequest();
        //        var usersUrl = string.Format(KeyCloakConstants.GetUsers + "/" + userId, realm);
        //        var response = await _requester.GetAsync(url + usersUrl);
        //        var users = await response.Content.ReadAsStringAsync();

        //        var userResponse = JsonSerializer.Deserialize<UserResponse>(users, new JsonSerializerOptions
        //        {
        //            PropertyNameCaseInsensitive = true
        //        });

        //        var userGroupsUrl = string.Format(KeyCloakConstants.GetUsers + "/" + userId + "/groups", realm);
        //        var groupsResponse = await _requester.GetAsync(url + userGroupsUrl);
        //        var groups = await groupsResponse.Content.ReadAsStringAsync();

        //        var userGroupsResponse = JsonSerializer.Deserialize<List<GroupResponse>>(groups, new JsonSerializerOptions
        //        {
        //            PropertyNameCaseInsensitive = true
        //        });

        //        model = new UserViewModel
        //        {
        //            Id = userResponse.Id,
        //            Username = userResponse.Username,
        //            FirstName = userResponse.FirstName,
        //            LastName = userResponse.LastName,
        //            CreatedTimestamp = userResponse.CreatedTimestamp,
        //            Email = userResponse.Email,
        //            EmailVerified = userResponse.EmailVerified,
        //            Enabled = userResponse.Enabled,
        //            GroupIds = string.Join(',', userGroupsResponse.Select(x => x.Name))
        //        };
        //        ViewData["GroupList_ddl"] = await SetGroupsViewData();
        //        return View(model);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Грешка при зареждане на детайли за потребител в {nameof(UserDetails)}");
        //        SetErrorMessage("Грешка при зареждане на детайли за потребител.");
        //    }
        //    return View(model);
        //}

        ///// <summary>
        ///// Форма за добавяне на потребител
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet]
        //public async Task<IActionResult> Add()
        //{
        //    try
        //    {
        //        ViewData["GroupList_ddl"] = await SetGroupsViewData();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Грешка при извличане на данни за групи в {nameof(Add)}");
        //        SetErrorMessage("Грешка при извличане на данни за групи.");
        //    }
        //    return View(new UserViewModel());

        //}

        ///// <summary>
        ///// Добавяне на потребител
        ///// </summary>
        ///// <param name="model">Модел на потребител</param>
        ///// <returns></returns>
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Add(UserViewModel model)
        //{
        //    try
        //    {
        //        UserKeyCloak user = new UserKeyCloak
        //        {
        //            Username = model.Username,
        //            Email = model.Email,
        //            Attributes = new AttributesKeyCloak()
        //            {
        //                Locale = string.Empty,
        //                AdministrationId = model.AdministrationId
        //            },
        //            EmailVerified = model.EmailVerified,
        //            FirstName = model.FirstName,
        //            LastName = model.LastName,
        //            Groups = model.GroupIds != null ? model.GroupIds.Split(',').ToList() : new List<string>()
        //        };

        //        await AttachTokenToRequest();
        //        var usersUrl = string.Format(KeyCloakConstants.GetUsers, realm);
        //        var response = await _requester.PostAsync(url + usersUrl, user);
        //        var result = await response.Content.ReadAsStringAsync();
        //        if (response.StatusCode == HttpStatusCode.Created)
        //        {
        //            SetSuccessMessage($"Успешно добавихте потребител {model.Username}");
        //            return RedirectToAction(nameof(Index));
        //        }
        //        SetErrorMessage($"Грешка при добавяне на потребител. Статус код: {response.StatusCode}: {result}");
        //        ViewData["GroupList_ddl"] = await SetGroupsViewData();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Грешка при добавяне на потребител в {nameof(Add)}");
        //        SetErrorMessage("Грешка при добавяне на потребител.");
        //    }
        //    return View(model);
        //}

        ///// <summary>
        ///// Редакция на потребител
        ///// </summary>
        ///// <param name="model">Модел на потребител</param>
        ///// <returns></returns>
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(UserViewModel model)
        //{
        //    try
        //    {
        //        await AttachTokenToRequest();
        //        UserKeyCloak user = new UserKeyCloak
        //        {
        //            Id = model.Id,
        //            Username = model.Username,
        //            Email = model.Email,
        //            Attributes = new AttributesKeyCloak()
        //            {
        //                Locale = string.Empty,
        //                AdministrationId = model.AdministrationId
        //            },
        //            EmailVerified = model.EmailVerified,
        //            FirstName = model.FirstName,
        //            LastName = model.LastName,
        //            Enabled = model.Enabled,
        //            Groups = model.GroupIds != null ? model.GroupIds.Split(',').ToList() : new List<string>()
        //        };

        //        var usersUrl = string.Format(KeyCloakConstants.GetUsers, realm);
        //        var response = await _requester.PutAsync(url + usersUrl + $"/{model.Id}", user);
        //        var result = response.Content.ReadAsStringAsync();
        //        if (response.StatusCode == HttpStatusCode.NoContent)
        //        {
        //            SetWarningMessage($"Успешно редактирахте потребител {model.Username}");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Грешка при редактиране на потребител в {nameof(Edit)}");
        //        SetErrorMessage("Грешка при редактиране на потребител.");
        //    }
        //    ViewData["GroupList_ddl"] = await SetGroupsViewData();
        //    return View(nameof(UserDetails), model);
        //}

        ///// <summary>
        ///// Групи на потребител
        ///// </summary>
        ///// <param name="request">Заявка с информация</param>
        ///// <param name="userId">Идентификатор на потребител</param>
        ///// <returns></returns>
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> GetUserGroups(IDataTablesRequest request, string userId) 
        //{
        //    try
        //    {
        //        await AttachTokenToRequest();
        //        var userGroupsUrl = string.Format(KeyCloakConstants.GetUsers + "/" + userId + "/groups", realm);
        //        var groupsResponse = await _requester.GetAsync(url + userGroupsUrl);
                
        //        if (!groupsResponse.IsSuccessStatusCode)
        //        {
        //            _logger.LogWarning($"Неуспешно извличане на групи. Status Code: {groupsResponse.StatusCode}");
        //            return BadRequest("Неуспешно извличане на групи.");
        //        }
        //        var groups = await groupsResponse.Content.ReadAsStringAsync();
        //        var userGroupsResponse = JsonSerializer.Deserialize<List<GroupResponse>>(groups, new JsonSerializerOptions
        //        {
        //            PropertyNameCaseInsensitive = true
        //        });

        //        var protoRequest = request!.GetDataTablesRequestProto();
        //        var userQueryable = userGroupsResponse.OrderByDescending(x => x.Name).AsQueryable();

        //        var filteredQuery = userQueryable.SearchFor(protoRequest.SearchColumn, protoRequest.Filter);

        //        var pagedQuery = request.Length < 0
        //            ? filteredQuery.OrderBy(protoRequest.OrderBy)
        //            : filteredQuery.OrderBy(protoRequest.OrderBy).Skip(request.Start).Take(request.Length);

        //        var pagedData = pagedQuery.ToList();

        //        return request.GetResponseServerPaging(pagedData, userGroupsResponse.Count);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Грешка при извличане на данни за групи в {nameof(GetUserGroups)}");
        //        SetErrorMessage("Грешка при извличане на данни за групи.");
        //    }
        //    return BadRequest("Грешка при извличане на данни за групи.");
        //}

        ///// <summary>
        ///// Добавяне на група
        ///// </summary>
        ///// <param name="name"></param>
        ///// <returns></returns>
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> AddGroup([FromBody] string name)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(name))
        //        {
        //            return BadRequest("Името е празно");
        //        }
        //        if (!string.IsNullOrEmpty(name) && name.Length > 200)
        //        {
        //            return BadRequest("Името не може де по-дълго от 200 символа"); ;
        //        }

        //        GroupsKeyCloak group = new GroupsKeyCloak() { Name = name };
        //        await AttachTokenToRequest();
        //        var groupsUrl = string.Format(KeyCloakConstants.GetGroups, realm);
        //        var response = await _requester.PostAsync(url + groupsUrl, group);
        //        if (response.StatusCode == HttpStatusCode.Created)
        //        {
        //            return Ok($"Успешно добавихте група {group.Name}");
        //        }
        //        if (response.StatusCode == HttpStatusCode.Conflict) 
        //        {
        //            return BadRequest($"Вече съществува група с име: {group.Name}");
        //        }

        //        return BadRequest("Възникна грешка");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Грешка при добавяне на група в {nameof(AddGroup)}");
        //        return BadRequest("Грешка при добавяне на група.");
        //    }
        //}

        ///// <summary>
        ///// Роли на потребител
        ///// </summary>
        ///// <param name="request">Заявка с информация</param>
        ///// <param name="userId">Идентификатор на потребител</param>
        ///// <returns></returns>
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> GetUserRoles(IDataTablesRequest request, string userId)
        //{
        //    try
        //    {
        //        await AttachTokenToRequest();

        //        var rolesUrl = string.Format(KeyCloakConstants.GetUserRoles, realm, userId);
        //        var response = await _requester.GetAsync(url + rolesUrl);
        //        if (!response.IsSuccessStatusCode)
        //        {
        //            _logger.LogWarning($"Неуспешно извличане на роли. Status Code: {response.StatusCode}");
        //            return BadRequest("Неуспешно извличане на роли.");
        //        }
        //        var rolesJson = await response.Content.ReadAsStringAsync();
        //        var userRoles = JsonSerializer.Deserialize<RoleMapping>(rolesJson, new JsonSerializerOptions
        //        {
        //            PropertyNameCaseInsensitive = true
        //        });

        //        if (rolesJson == "{}")
        //        {
        //            userRoles = new RoleMapping 
        //            {
        //                RealmMappings = new List<RoleKeycloak>()
        //            };
        //        }
               
        //        var protoRequest = request!.GetDataTablesRequestProto();
        //        var userQueryable = userRoles.RealmMappings.OrderByDescending(x => x.Name).AsQueryable();

        //        var filteredQuery = userQueryable.SearchFor(protoRequest.SearchColumn, protoRequest.Filter);

        //        var pagedQuery = request.Length < 0
        //            ? filteredQuery.OrderBy(protoRequest.OrderBy)
        //            : filteredQuery.OrderBy(protoRequest.OrderBy).Skip(request.Start).Take(request.Length);

        //        var pagedData = pagedQuery.ToList();

        //        return request.GetResponseServerPaging(pagedData, userRoles.RealmMappings.Count);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Грешка при редактиране на роли на потребител в {nameof(GetUserRoles)}");
        //        SetErrorMessage("Грешка при редактиране на роли на потребител.");
        //    }
        //    return BadRequest("Грешка при извличане на данни за роли.");
        //}

        ///// <summary>
        ///// Роли позволени за потребител
        ///// </summary>
        ///// <param name="userId">Идентификатор на потребител</param>
        ///// <returns></returns>
        //public async Task<IActionResult> GetUserAvailableRoles(string userId)
        //{
        //    try
        //    {
        //        await AttachTokenToRequest();

        //        var rolesUrl = string.Format(KeyCloakConstants.GetАvailableUserRoles, realm, userId);
        //        var response = await _requester.GetAsync(url + rolesUrl);
        //        var rolesJson = await response.Content.ReadAsStringAsync();
        //        return Ok(rolesJson);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Грешка при редактиране на роли на потребител в {nameof(GetUserRoles)}");
        //        SetErrorMessage("Грешка при редактиране на роли на потребител.");
        //    }
        //    return BadRequest(string.Empty);
        //}

        ///// <summary>
        ///// Групи позволени за потребител
        ///// </summary>
        ///// <param name="userId">Идентификатор на потребител</param>
        ///// <returns></returns>
        //public async Task<IActionResult> GetUserAvailableGroups(string userId)
        //{
        //    try
        //    {
        //        await AttachTokenToRequest();

        //        var userGroupsUrl = string.Format(KeyCloakConstants.GetUsers + "/" + userId + "/groups", realm);
        //        var realmGroupsUrl = string.Format(KeyCloakConstants.GetGroups, realm);
        //        var responseUserGroups = await _requester.GetAsync(url + userGroupsUrl);
        //        var responseRealmGroups = await _requester.GetAsync(url + realmGroupsUrl);
        //        var rolesUserGroupsJson = await responseUserGroups.Content.ReadAsStringAsync();
        //        var rolesRealmGroupsJson = await responseRealmGroups.Content.ReadAsStringAsync();
        //        if (responseRealmGroups.StatusCode == HttpStatusCode.OK && responseUserGroups.StatusCode == HttpStatusCode.OK)
        //        {
        //            var userGroups = JsonSerializer.Deserialize<List<GroupResponse>>(rolesUserGroupsJson, new JsonSerializerOptions
        //            {
        //                PropertyNameCaseInsensitive = true
        //            });

        //            var realmGroups = JsonSerializer.Deserialize<List<GroupResponse>>(rolesRealmGroupsJson, new JsonSerializerOptions
        //            {
        //                PropertyNameCaseInsensitive = true
        //            });

        //            var mergedGroups = userGroups
        //            .Where(userGroup => !realmGroups.Any(realmGroup => realmGroup.Id == userGroup.Id))
        //            .Concat(realmGroups.Where(realmGroup => !userGroups.Any(userGroup => userGroup.Id == realmGroup.Id)))
        //            .ToList();
        //            return Ok(mergedGroups);
        //        }
        //        return BadRequest();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Грешка при редактиране на роли на потребител в {nameof(GetUserRoles)}");
        //        SetErrorMessage("Грешка при редактиране на роли на потребител.");
        //    }
        //    return BadRequest(new { message = "Грешка при премахване на група" });
        //}

        ///// <summary>
        ///// Промяна на ролите на потребител
        ///// </summary>
        ///// <param name="request">Заявка с информация</param>
        ///// <returns></returns>
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> UpdateUserRoles([FromBody] UserRoleUpdateRequest request)
        //{
        //    try
        //    {
        //        await AttachTokenToRequest();

        //        var rolesUrl = string.Format(KeyCloakConstants.UpdateUserRoles, realm, request.UserId);
        //        var response = await _requester.PostAsync(url + rolesUrl, request.Roles);
        //        var rolesJson = await response.Content.ReadAsStringAsync();
        //        return Ok(new { message = "Успешно добавяне на роля" });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Грешка при редактиране на роли на потребител в {nameof(GetUserRoles)}");
        //        return BadRequest("Грешка при редактиране на роли на потребител.");
        //    }
        //}

        ///// <summary>
        ///// Промяна на групите на потребител
        ///// </summary>
        ///// <param name="request">Заявка с информация</param>
        ///// <returns></returns>
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> UpdateUserGroups([FromBody] UserGroupsUpdateRequest request)
        //{
        //    try
        //    {
        //        await AttachTokenToRequest();

        //        foreach (var id in request.Groups)
        //        {
        //            var rolesUrl = string.Format(KeyCloakConstants.UpdateUserGroups, realm, request.UserId, id);
        //            var response = await _requester.PutAsync(url + rolesUrl);
        //            if (response.StatusCode != HttpStatusCode.NoContent) 
        //            {
        //                return BadRequest(new { message = "Нещо се обърка" });
        //            }
        //        }
        //        return Ok(new { message = "Успешно добавяне на група" });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Грешка при редактиране на групи на потребител в {nameof(GetUserRoles)}");
        //        return BadRequest("Грешка при редактиране на групи на потребител.");
        //    }
        //}

        //[HttpDelete]
        ////TODO
        ////[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Unassign([FromBody] UserRoleUpdateRequest request) 
        //{
        //    try
        //    {
        //        await AttachTokenToRequest();

        //        var rolesUrl = string.Format(KeyCloakConstants.UpdateUserRoles, realm, request.UserId);
        //        var response = await _requester.DeleteAsync(url + rolesUrl, request.Roles);
        //        if (response.StatusCode == HttpStatusCode.NoContent) 
        //        {
        //            return Ok(new { message = "Успешно премахване на роля" });
        //        }
        //        return BadRequest(new { message = "Грешка про премахване на роля" });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Грешка при редактиране на роли на потребител в {nameof(Unassign)}");
        //        return BadRequest("Грешка при редактиране на роли на потребител.");
        //    }
        //}

        //[HttpDelete]
        ////TODO
        ////[ValidateAntiForgeryToken]
        //public async Task<IActionResult> UnassignGroups([FromBody] UserGroupsUpdateRequest request)
        //{
        //    try
        //    {
        //        await AttachTokenToRequest();

        //        var rolesUrl = string.Format(KeyCloakConstants.UpdateUserGroups, realm, request.UserId, request.GroupId);
        //        var response = await _requester.DeleteAsync(url + rolesUrl);
        //        if (response.StatusCode == HttpStatusCode.NoContent) 
        //        {
        //            return Ok(new { message = "Успешно премахване на група" });
        //        }
        //        return BadRequest(new { message = "Грешка при премахване на група" });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Грешка при премахване на роли на потребител в {nameof(Unassign)}");
        //        return BadRequest("Грешка при редактиране на роли на потребител.");
        //    }
        //}

        //private async Task AttachTokenToRequest()
        //{
        //    var clientId = _config["KeyCloakSettings:ClientId"];
        //    var clientSecret = _config["KeyCloakSettings:ClientSecret"];
        //    var grantType = _config["KeyCloakSettings:GrantType"];

        //    _requester.FormUrlEncodedParams = new Dictionary<string, string>
        //        {
        //            { "client_id", clientId },
        //            { "client_secret", clientSecret },
        //            { "grant_type", grantType }
        //        };

        //    var tokenUrl = string.Format(KeyCloakConstants.Token, realm);
        //    var response = await _requester.PostAsync(url + tokenUrl, null);
        //    var result = await response.Content.ReadAsStringAsync();

        //    var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(result, new JsonSerializerOptions
        //    {
        //        PropertyNameCaseInsensitive = true
        //    });

        //    _requester.FormUrlEncodedParams = null;
        //    _requester.BearerToken = tokenResponse.Token;
        //}

        //private async Task<List<GroupResponse>?> GetGroups()
        //{
        //    await AttachTokenToRequest();
        //    var groupsUrl = string.Format(KeyCloakConstants.GetGroups, realm);
        //    var response = await _requester.GetAsync(url + groupsUrl);
        //    var groups = await response.Content.ReadAsStringAsync();

        //    return JsonSerializer.Deserialize<List<GroupResponse>>(groups, new JsonSerializerOptions
        //    {
        //        PropertyNameCaseInsensitive = true
        //    });
        //}

        //private async Task<List<SelectListItem>> SetGroupsViewData()
        //{
        //    List<GroupResponse>? groups = await GetGroups();
        //    if (groups == null && groups.Count == 0)
        //    {
        //        return new List<SelectListItem>();

        //    }
        //    return groups.Select(g => new SelectListItem(g.Name, g.Name)).ToList();
        //}
    }
}
