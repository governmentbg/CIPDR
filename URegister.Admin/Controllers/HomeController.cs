using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using URegister.Admin.Models;
using URegister.Core.Models;
using URegister.NomenclaturesCatalog;

namespace URegister.Admin.Controllers;

public class HomeController(
    ILogger<HomeController> logger,
    NomenclatureGrpc.NomenclatureGrpcClient nomenclatureGrpcClient
) : BaseController
{
 
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult Test()
    {
        var result = nomenclatureGrpcClient.AddNomenclatureType(new NomenclatureTypeRequest
        {
            Type = "Test",
            Name = "Test",
        });
        return View();
    }
}
