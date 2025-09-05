using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using URegister.Admin.Models;
using URegister.Core.Contracts;
using URegister.NomenclaturesCatalog;
using URegister.Users;

namespace URegister.Admin.Controllers;
[Display(Name = "Начало")]
public class HomeController(
    ILogger<HomeController> logger,
    NomenclatureGrpc.NomenclatureGrpcClient nomenclatureGrpcClient,
    IRegisterClientService registerClientService,
    AppUserManager.AppUserManagerClient appUserManagerClient
) : BaseController
{
    [Display(Name = "Зареждане на начална страница")]
    public async Task<IActionResult> Index()
    {
        var registerCount = await registerClientService.GetRegisterCount();
        var administrationsResult = await registerClientService.GetAllAdministrations();
        var userInfo = appUserManagerClient.GetUsersDashboard(new GetUsersRequest { AdministrationId = string.Empty });
        var model = new HomeStatisticsVM
        {
            RegisterCount = registerCount,
            AdministrationCount = administrationsResult.Administrations.Count,
            UserCount = userInfo.UsersCount
        };
        return View(model);
    }

    [Display(Name = "Зареждане на страница за поверителност")]
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [Display(Name = "Зареждане на страница за грешка")]
    [AllowAnonymous]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
