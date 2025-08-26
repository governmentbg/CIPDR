using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using URegister.Core.Contracts;
using URegister.Core.Models.CurrentRegister;
using URegister.Infrastructure.Constants;

namespace URegister.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Manager}, {UserRoles.Editor}")]
    [Display(Name = "Регистър")]
    public class RegisterController(
        INomenclatureClientService nomenclatureClient,
        IRegisterService registerService,
        IRegisterClientService registerClient,
        ILogger<RegisterController> logger
    ) : BaseController
    {
        /// <summary>
        /// Списък регистри
        /// </summary>
        /// <returns></returns>
        [Display(Name = "Зареждане на списък с регистри")]
        public IActionResult IndexAdministration()
        {
            return View();
        }

        /// <summary>
        /// Списък на администрации към регистърa
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Извличане на списък с администрации към регистър")]
        public async Task<IActionResult> GetAdministrationList(IDataTablesRequest request)
        {
            // return await registerService.GetAdministrationList(request);
            var filter = new Core.Models.Register.AdministrationFilterVM
            {
                RegisterId = await registerService.GetCurrentRegisterId()
            };
            return await registerClient.GetAdministrationList(request, filter);
        }

      
        /// <summary>
        /// Страница за редакция на регистър
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Зареждане на форма за редакция на регистър")]
        public async Task<IActionResult> Edit()
        {
            await nomenclatureClient.SetViewBagRegister(ViewData);
            var model = await registerService.GetCurrentRegister();
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Редакция на регистър
        /// </summary>
        /// <param name="model">Модел на регистъра</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Редактиране на регистър")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await registerService.SaveRegister(model);
                    SetSuccessMessage("Успешно редакция");
                    return RedirectToAction("Index", "Home", new { area = string.Empty });
                }
                catch (Exception ex)
                {
                    {
                        logger.LogError(ex, "Проблем при запис на данни за регистър");
                        SetErrorMessage($"Проблем при запис!{Environment.NewLine}{ex.Message}");
                    }
                }
            }
            await nomenclatureClient.SetViewBagRegister(ViewData);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Списък от оторозирани лица
        /// </summary>
        /// <returns></returns>
        [Display(Name = "Зареждане на списък с упълномощени лица")]
        public IActionResult IndexPerson(Guid administrationId)
        {
            var filter = new PersonFilterVM
            {
                AdministrationId = administrationId,
            };
            return View(filter);
        }
        /// <summary>
        /// Списък на оторозирани лица към администрация
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Извличане на списък с упълномощени лица към администрация")]
        public async Task<IActionResult> GetPersonList(IDataTablesRequest request, Core.Models.Register.PersonFilterVM filter)
        {
            return await registerClient.GetPersonList(request, filter);
            //  return await registerService.GetPersonList(request, filter.AdministrationId);
        }

    }
}
