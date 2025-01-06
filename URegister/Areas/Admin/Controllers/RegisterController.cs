using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using URegister.Core.Contracts;
using URegister.Core.Models.CurrentRegister;

namespace URegister.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RegisterController(
        INomenclatureClientService nomenclatureClient,
        IRegisterService registerService,
        ILogger<RegisterController> logger
    ) : BaseController
    {
        /// <summary>
        /// Списък регистри
        /// </summary>
        /// <returns></returns>
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
        public async Task<IActionResult> GetAdministrationList(IDataTablesRequest request)
        {
            return await registerService.GetAdministrationList(request);
        }

        /// <summary>
        /// Редакция на администрация към регистър
        /// </summary>
        /// <param name="nomenclatureType"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> EditAdministration(Guid administrationId)
        {
            await nomenclatureClient.SetViewBagRegister(ViewData);
            var model = await registerService.GetAdministration(administrationId);
            return View(nameof(EditAdministration), model);
        }
        
        /// <summary>
        /// Промяна на администрация
        /// </summary>
        /// <param name="model">Модел на администрация</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAdministration(AdministrationVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await registerService.SaveAdministration(model);
                    SetSuccessMessage("Успешно добавена администрация");
                    return RedirectToAction("IndexAdministration");
                }
                catch (Exception ex)
                {
                    {
                        logger.LogError(ex, "Проблем при запис на администрация");
                        SetErrorMessage($"Проблем при запис!{Environment.NewLine}{ex.Message}");
                    }
                }
            }
            return View(nameof(EditAdministration), model);
        }

        /// <summary>
        /// Страница за редакция на регистър
        /// </summary>
        /// <returns></returns>
        [HttpGet]
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
        /// Стартиране на регистър
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Start()
        {
            var model = new RegisterStartVM();
            ViewBag.Id_ddl = await registerService.GetRegisterNotStartedDdl();
            return View(nameof(Start), model);
        }

        /// <summary>
        /// Запис стартиране на регистър
        /// </summary>
        /// <param name="model">Модел на стартиране на регистър</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(RegisterStartVM model)
        {
            try
            {
                await registerService.StartRegister(model.Id);
                SetSuccessMessage("Успешно стартиране на регистър");
                return RedirectToAction("Index", "Home", new { area = string.Empty });
            }
            catch (Exception ex)
            {
                {
                    logger.LogError(ex, "Проблем при запис на данни за регистър");
                    SetErrorMessage($"Проблем при стартиране на регистър!{Environment.NewLine}{ex.Message}");
                }
            }
            ViewBag.Id_ddl = await registerService.GetRegisterNotStartedDdl();
            return View(nameof(Start), model);
        }
    }
}
