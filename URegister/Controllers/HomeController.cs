using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Google.Protobuf.WellKnownTypes;
using URegister.Common;
using URegister.Core.Contracts;
using URegister.Core.Models.Process;
using URegister.Infrastructure.Constants;
using URegister.Users;
using DataTables.AspNet.Core;
using URegister.Infrastructure.Extensions;
using URegister.Models;
using Org.BouncyCastle.Utilities;

namespace URegister.Controllers;

[Authorize(Roles = $"{UserRoles.GlobalAdmin},{UserRoles.Admin},{UserRoles.Manager},{UserRoles.Editor},{UserRoles.Registrator}")]
[Display(Name = "Начало")]
public class HomeController : BaseController
{
    private readonly ILogger<HomeController> logger;
    private readonly IDashboardService dashboardService;
    private readonly IRegisterClientService registerClientService;
    private readonly IRegisterService registerService;
    private readonly IProcessService processService;
    private readonly AppUserManager.AppUserManagerClient appUserManagerClient;
    private readonly IConfiguration configuration;

    public HomeController(
        ILogger<HomeController> logger,
        IDashboardService dashboardService,
        IRegisterClientService registerClientService,
        IRegisterService registerService,
        IProcessService processService,
        AppUserManager.AppUserManagerClient appUserManagerClient,
        IConfiguration configuration)
    {
        this.logger = logger;
        this.dashboardService = dashboardService;
        this.registerClientService = registerClientService;
        this.processService = processService;
        this.appUserManagerClient = appUserManagerClient;
        this.configuration = configuration;
        this.registerService = registerService;
    }

    [Display(Name = "Преглед на начална страница")]
    public async Task<IActionResult> Index()
    {
        DashboardVM model = new DashboardVM();
        try
        {
            model = await dashboardService.GetDashboardData();
            var administrationId = User.Claims.FirstOrDefault(c => c.Type == CustomClaimType.AdministrationId)?.Value ?? string.Empty;
            var administrationResponse = await registerClientService.GetAdministrationById(administrationId);
            
            if (administrationResponse.Status.Code != ResultCodes.Ok)
            {
                SetErrorMessage("Грешка при зареждане на данни за администрация.");
                logger.LogError($"Грешка при зареждане на данни за администрация на контролно табло в {nameof(Index)}");
                return View(model);
            }

            model.CurrentAdministrationName = administrationResponse.Data.Name;
            var currentRegister = await registerService.GetCurrentRegister();
            var usersResponse = appUserManagerClient.GetUsersDashboard(new GetUsersRequest
            {
                AdministrationId = administrationId,
                RegisterCode = currentRegister.Code
            });
            if (usersResponse.Status.Code != ResultCodes.Ok)
            {
                SetErrorMessage("Грешка при зареждане на данни за потребители.");
                logger.LogError($"Грешка при зареждане на данни на потребители за контролно табло в {nameof(Index)}");
                return View(model);
            }

            model.Users.UsersCount = usersResponse.UsersCount;
            model.Users.EnableUsersCount = usersResponse.EnableUsersCount;
            model.Users.DisableUsersCount = usersResponse.DisableUsersCount;
            model.Users.AdminisrationName = administrationResponse.Data.Name;
            var baseUrl = configuration["RegisterBaseURL"];
            model.RegisterBaseURL = baseUrl;
            model.UserAssignedProcessCount = await processService.GetUserAssignedProcessCount();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Грешка при зареждане на данни за контролно табло в {nameof(Index)}");
            SetErrorMessage("Грешка при зареждане на данни за контролно табло.");
        }
        return View(model);
    }

    [Display(Name = "Самоназначаване на необработено заявление")]
    public async Task<IActionResult> GenerateItems()
    {
        var userAssignedProcessCount = await processService.GetUserAssignedProcessCount();

        if (userAssignedProcessCount <= 0)
        {
            var process = await processService.GetAssignableProcess();
            if (process != null)
            {
               await processService.AssignProcess(process.Id);
            }
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Display(Name = "Преглед на списък с регистри в текущата администрация")]
    public async Task<IActionResult> GetRegisters(IDataTablesRequest request)
    {
        try
        {
            var userClaims = User.Claims;
            var administrationId = userClaims.FirstOrDefault(a => a.Type == CustomClaimType.AdministrationId)?.Value;
            if (administrationId != null)
            {
                var registries = await registerClientService.GetAllRegisterInAdministration(administrationId);
                return request.GetResponseServerPaging(registries, registries.Count);
            }
            return BadRequest("Грешка при зареждане на данни за регистри.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Грешка при зареждане на данни за регистри {nameof(GetRegisters)}");
            return BadRequest("Грешка при зареждане на данни за регистри.");
        }
    }
}
